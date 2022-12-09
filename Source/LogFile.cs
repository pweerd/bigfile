/*
 * Licensed to De Bitmanager under one or more contributor
 * license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright
 * ownership. De Bitmanager licenses this file to you under
 * the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitmanager.Core;
using Bitmanager.IO;
using Bitmanager.Query;
using Bitmanager.BigFile.Query;
using System.Runtime;
using System.IO.Compression;
using System.Linq;
using System.Collections.ObjectModel;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Constants for the flags and masks in the line offset field (long)
   /// This long is build like [offset][search_mask][flags]
   /// - The flags part is 4 bits
   /// - There are 20 mask bits. Each mask-bit correspond to a search term hit
   /// - The offset part is 64-24=40 bits
   /// </summary>
   public abstract class LineFlags {
      public const int CONTINUATION = 0x01;
      public const int MATCHED = 0x02;
      public const int SELECTED = 0x04;
      public const int ALL_EVALUATED = 0x08;
      public const int RESETTABLE_FLAGS = MATCHED | SELECTED | ALL_EVALUATED;

      public const int MASK0 = 0x10; //first mask: should be 1 bit further than the last flag

      public const int NUM_FLAGS = 4;
      public const int NUM_MASKS = 20;

      public const int FLAGS_SHIFT = NUM_FLAGS + NUM_MASKS;
      public const int FLAGS_MASK = (1 << FLAGS_SHIFT) - 1;
   }


   /// <summary>
   /// LogFile is responsible for loading the data and splitting it into lines.
   /// </summary>
   public class LogFile {
      static public String DbgStr = "gzip";
      static readonly Logger logger = Globals.MainLogger.Clone ("logfile");

      private ThreadContext threadCtx;
      private readonly List<long> partialLines;
      private List<int> lines;
      private ZipEntries zipEntries;
      public ZipEntries ZipEntries { get { return zipEntries; } }
      public int LongestPartialIndex => largestPartialLine == null ? 0 : largestPartialLine.Length;
      public int LongestLineIndex => largestPartialLine == null ? -1 : largestPartialLine.Index;
      public int PartialLineCount { get { return partialLines.Count - 1; } }
      public long Size { get { return partialLines[partialLines.Count - 1] >> LineFlags.FLAGS_SHIFT; } }
      public int LineCount { get { return lines == null ? partialLines.Count - 1 : lines.Count - 1; } }
      public String FileName { get { return fileName; } }
      private FileEncoding detectedEncoding;
      public FileEncoding DetectedEncoding => detectedEncoding;

      private Encoding encoding = Encoding.UTF8;

      private PartialLineLength[] largestPartialLines;
      private PartialLineLength largestPartialLine;
      public PartialLineLength[] LargestPartialLines => largestPartialLines;

      public IDirectStream DirectStream { get { return threadCtx == null ? null : threadCtx.DirectStream; } }

      public Encoding SetEncoding (Encoding c) {
         var old = encoding;
         encoding = c != null ? c : Encoding.UTF8;
         if (threadCtx != null) threadCtx.Encoding = encoding;
         return old;
      }

      public void SyncSettings (Settings settings, Encoding enc) {
         this.settings = settings;
         SetEncoding (enc);
      }

      private String fileName;
      private readonly ILogFileCallback cb;

      #region reflected_settings
      private Settings settings;
      private readonly int maxPartialSize;
      #endregion

      private long maxLoadSize;
      private CancellationToken ct;
      private bool disposed;
      public bool Disposed { get { return disposed; } }
      private bool partialsEncountered;
      private void checkCancelled () {
         if (disposed || ct.IsCancellationRequested)
            throw new TaskCanceledException ();
      }
      private void checkCancelled (long pos) {
         if (disposed || ct.IsCancellationRequested || pos >= maxLoadSize)
            throw new TaskCanceledException ();
      }

      public int Checked;
      public LogFile (ILogFileCallback cb, Settings settings, Encoding enc, int maxPartialSize, long maxLoadSize=0) {
         this.maxLoadSize = maxLoadSize > 0 ? maxLoadSize : long.MaxValue;

         if (maxPartialSize <= 0) this.maxPartialSize = int.MaxValue;
         else if (maxPartialSize < 256) this.maxPartialSize = 256;
         else if (maxPartialSize > 4096) this.maxPartialSize = 4096;
         else this.maxPartialSize = maxPartialSize;

         this.cb = cb;

         SyncSettings (settings, enc);
         partialLines = new List<long> ();
      }

      private LogFile (LogFile other) {
         this.cb = other.cb;
         this.settings = other.settings;
         this.maxPartialSize = other.maxPartialSize;

         this.fileName = other.fileName;
         this.partialsEncountered = other.partialsEncountered;
         this.partialLines = new List<long> (other.partialLines);
         this.maxPartialSize = other.maxPartialSize;
         this.encoding = other.encoding;
         this.detectedEncoding = other.detectedEncoding;
         this.zipEntries = other.zipEntries;
         this.maxLoadSize = other.maxLoadSize;
         int maxBufferSize = finalizeAdministration ();

         threadCtx = other.threadCtx.NewInstanceForThread (maxBufferSize);
      }

      /// <summary>
      /// Returns true if both logFile's are reflecting the same. This is the case when
      /// 1) the file is the same
      /// 2) the selected zip-entry is the same (in case of a zip-file)
      /// </summary>
      public bool IsSameFile (LogFile other) {
         if (other == null || disposed || other.disposed) return false;
         if (fileName != other.fileName) return false;
         if (zipEntries == null && other.zipEntries == null) return true;
         if (zipEntries != null && other.zipEntries != null)
            return zipEntries.SelectedEntry == other.zipEntries.SelectedEntry;
         return false;
      }


      /// <summary>
      /// Given a partial line number, get the next partial line number. 
      /// If the line is beyond the end, PartialLineCount is returned.
      /// If a partialFilter is supplied, only lines within that filter are used.
      /// </summary>
      public int NextPartialLineNumber (int partialIndex, List<int> partialFilter) {
         if (partialIndex < -1) partialIndex = -1;
         int lineCount = PartialLineCount;
         logger.Log ("nextPartialLineNumber ({0})", partialIndex, lineCount);
         ++partialIndex;
         if (partialIndex >= lineCount) return int.MaxValue;
         if (partialFilter == null) return partialIndex;

         //Invariant: arr[i] < arg && arr[j] >= arg
         int i = -1;
         int j = partialFilter.Count;
         while (j - i > 1) {
            int m = (i + j) / 2;
            if (partialFilter[m] >= partialIndex) j = m; else i = m;
         }
         logger.Log ("-- finding in filter: i={0}, j={1}, cnt={2}", i, j, partialFilter.Count);
         if (j >= partialFilter.Count)
            logger.Log ("-- at end");
         else
            logger.Log ("-- next partial: {0}", partialFilter[j]);

         dumpFilter (partialFilter, i);
         return j >= partialFilter.Count ? int.MaxValue : partialFilter[j];
      }

      /// <summary>
      /// Given a line number, get the previous line number. 
      /// If the line is before the start, -1 is returned.
      /// If a partialFilter is supplied, only lines within that filter are used.
      /// </summary>
      public int PrevPartialLineNumber (int partialIndex, List<int> partialFilter) {
         int lineCount = PartialLineCount;
         if (partialIndex > lineCount) partialIndex = lineCount;
         logger.Log ("prevPartialLineNumber ({0})", partialIndex, lineCount);
         if (partialIndex <= 0) return -1;
         if (partialFilter == null) return partialIndex - 1;

         int i = -1;
         int j = partialFilter.Count;
         while (j - i > 1) {
            int m = (i + j) / 2;
            if (partialFilter[m] >= partialIndex) j = m; else i = m;
         }
         logger.Log ("-- finding in filter: i={0}, j={1}, cnt={2}", i, j, partialFilter.Count);
         if (i < 0)
            logger.Log ("-- at top");
         else
            logger.Log ("-- prev partial: {0}", partialFilter[i]);
         //dumpFilter(partialFilter, i);
         return i < 0 ? -1 : partialFilter[i];
      }


      /// <summary>
      /// Given a line number, get the next line number. 
      /// If the line is beyond the end, LineCount is returned.
      /// If a partialFilter is supplied, only lines within that filter are used.
      /// </summary>
      public int NextLineNumber (int line, List<int> partialFilter) {
         int lineCount = LineCount;
         logger.Log ("nextLineNumber ({0})", line, lineCount);

         if (line < 0) line = 0; else line++;
         if (line >= lineCount) return int.MaxValue;
         if (partialFilter == null) return line;

         int nextPartial = NextPartialLineNumber (PartialFromLineNumber (line) - 1, partialFilter);
         return PartialToLineNumber (nextPartial);
      }

      /// <summary>
      /// Given a line number, get the previous line number. 
      /// If the line is before the start, -1 is returned.
      /// If a partialFilter is supplied, only lines within that filter are used.
      /// </summary>
      public int PrevLineNumber (int line, List<int> partialFilter) {
         int lineCount = LineCount;
         logger.Log ("prevLineNumber ({0})", line, lineCount);

         if (line > lineCount) line = lineCount - 1; else line--;
         if (line < 0) return -1;
         if (partialFilter == null) return line;

         int prevPartial = PrevPartialLineNumber (PartialFromLineNumber (line) + 1, partialFilter);
         return PartialToLineNumber (prevPartial);
      }

      private void dumpFilter (List<int> partialFilter, int i) {
         int from = i - 5;
         if (from < 0) from = 0;

         int until = i + 6;
         if (until > partialFilter.Count) until = partialFilter.Count;

         for (int j = from; j < until; j++) {
            logger.Log ("-- Filter [{0}] = {1}, {2} (real) ", j, partialFilter[j], PartialToLineNumber (partialFilter[j]));
         }
      }

      /// <summary>
      /// Administrates the longest (partial) line and returns the buffersize that is needed to hold the max (partial)line. 
      /// </summary>
      private int finalizeAdministration () {
         if (partialsEncountered) {
            lines = new List<int> ();
            lines.Add (0);
         } else
            lines = null;

         if (partialLines.Count == 0) partialLines.Add (0);

         var prique = new LargestLines ();

         long prevPartial = partialLines[0];
         long prevLine = partialLines[0];
         int maxPartialLen = 0;
         int maxPartialIdx = 0;
         int maxLineLen = 0;
         int maxLineIdx = 0;
         int len;
         for (int i = 1; i < partialLines.Count; i++) {
            long offset = partialLines[i];
            int flags = (int)offset;
            offset >>= LineFlags.FLAGS_SHIFT;
            if (lines != null) {
               if ((flags & LineFlags.CONTINUATION) == 0) {
                  len = (int)(offset - prevLine);
                  if (len > maxLineLen) {
                     maxLineIdx = lines.Count - 1;
                     maxLineLen = len;
                  }
                  prevLine = offset;
                  lines.Add (i);
               }
            }

            len = (int)(offset - prevPartial);
            prevPartial = offset;

            if (len > prique.Min.Length) prique.Add (new PartialLineLength(i-1, len));
         }
         largestPartialLines = prique.ToArray ();
         PartialLineLength max = largestPartialLines[0];
         for (int i = largestPartialLines.Length - 1; i > 0; i--) 
            if (largestPartialLines[i].Length > max.Length) max = largestPartialLines[i];
         largestPartialLine = max;
         return max.Length;
      }


      /// <summary>
      /// NextPartial hit: start searching at partial line idxPartial+1
      /// Returns int.MaxValue if not found
      /// </summary>
      public int NextPartialHit (int idxPartial) {
         if (idxPartial < -1) idxPartial = -1;
         long mask = LineFlags.MATCHED;
         for (int i = idxPartial + 1; i < partialLines.Count - 1; i++) {
            if ((partialLines[i] & mask) != 0) return i;
         }
         return int.MaxValue;
      }

      /// <summary>
      /// PrevPartialHit hit: start searching at partial line idxPartial-1
      /// Returns -1 if not found
      /// </summary>
      public int PrevPartialHit (int idxPartial) {
         if (idxPartial > PartialLineCount) idxPartial = PartialLineCount;
         long mask = LineFlags.MATCHED;
         for (int i = idxPartial - 1; i >= 0; i--) {
            if ((partialLines[i] & mask) != 0) return i;
         }
         return -1;
      }


      /// <summary>
      /// Get a list of indexes for partial lines that are matched 
      /// </summary>
      public List<int> GetMatchedList (int contextLines) {
         var ret = new List<int> ();

         if (contextLines > 0) {
            int prevMatch = -1;
            for (int i = 0; i < partialLines.Count - 1; i++) {
               if ((GetLineFlags (i) & LineFlags.MATCHED) == 0) continue;
               if (contextLines > 0) {
                  int endPrevContext = prevMatch + 1 + contextLines;
                  if (endPrevContext > i) endPrevContext = i;
                  int startContext = i - contextLines;
                  if (startContext <= endPrevContext)
                     startContext = endPrevContext;
                  for (int j = prevMatch + 1; j < endPrevContext; j++)
                     ret.Add (j);
                  for (int j = startContext; j < i; j++)
                     ret.Add (j);
               }
               ret.Add (i);
               prevMatch = i;
            }
            if (contextLines > 0) {
               int endPrevContext = prevMatch + 1 + contextLines;
               if (endPrevContext > partialLines.Count - 1) endPrevContext = partialLines.Count - 1;
               for (int j = prevMatch + 1; j < endPrevContext; j++)
                  ret.Add (j);
            }
            return ret;
         }

         //No context lines
         for (int i = 0; i < partialLines.Count - 1; i++) {
            if ((GetLineFlags (i) & LineFlags.MATCHED) == 0) continue;
            ret.Add (i);
         }
         return ret;
      }

      public List<int> GetUnmatchedList (int contextLines) {
         var ret = new List<int> ();

         if (contextLines > 0) {
            int prevMatch = -1;
            for (int i = 0; i < partialLines.Count - 1; i++) {
               if ((GetLineFlags (i) & LineFlags.MATCHED) != 0) continue;
               if (contextLines > 0) {
                  int endPrevContext = prevMatch + 1 + contextLines;
                  if (endPrevContext > i) endPrevContext = i;
                  int startContext = i - contextLines;
                  if (startContext <= endPrevContext)
                     startContext = endPrevContext;
                  for (int j = prevMatch + 1; j < endPrevContext; j++)
                     ret.Add (j);
                  for (int j = startContext; j < i; j++)
                     ret.Add (j);
               }
               ret.Add (i);
               prevMatch = i;
            }
            if (contextLines > 0) {
               int endPrevContext = prevMatch + 1 + contextLines;
               if (endPrevContext > partialLines.Count - 1) endPrevContext = partialLines.Count - 1;
               for (int j = prevMatch + 1; j < endPrevContext; j++)
                  ret.Add (j);
            }
            return ret;
         }

         //No context lines
         for (int i = 0; i < partialLines.Count - 1; i++) {
            if ((GetLineFlags (i) & LineFlags.MATCHED) != 0) continue;
            ret.Add (i);
         }
         return ret;
      }

      static int cbCmpEntryLen (ZipEntry a, ZipEntry b) {
         if (a.Length > b.Length) return -1;
         if (a.Length < b.Length) return 1;
         return String.CompareOrdinal (a.Name, b.Name);
      }
      static ZipArchiveEntry getZipArchiveEntry (ReadOnlyCollection<ZipArchiveEntry> entries, ZipEntry e) {
         foreach (var zae in entries) {
            if (zae.FullName == e.FullName) return zae;
         }
         throw new BMException ("Cannot find '{0}' in archive '{1}'.", e.FullName, e.ArchiveName);
      }
      /// <summary>
      /// Load a .zip file.
      /// This is done by taking the largest file and stream that into memory
      /// </summary>
      private void loadZipFile (String fn, CancellationToken ct, String zipEntryName) {
         using (var fs = new FileStream (fn, FileMode.Open, FileAccess.Read, FileShare.Read))
         using (ZipArchive archive = new ZipArchive (fs, ZipArchiveMode.Read, false, Encoding.UTF8)) {
            var entries = archive.Entries;
            zipEntries = new ZipEntries ();
            foreach (var e in entries) zipEntries.Add (new ZipEntry (fn, e));
            zipEntries.Sort (cbCmpEntryLen);

            if (zipEntries.Count > 0) {
               if (zipEntryName == null)
                  zipEntries.SelectedEntry = 0;
               else {
                  zipEntries.SelectedEntry = zipEntries.FindIndex (x => x.FullName == zipEntryName);
                  if (zipEntries.SelectedEntry < 0) throw new BMException ("Requested entry '{0}' not found in archive '{1}'.", zipEntryName, fn);
               }
               using (var entryStrm = getZipArchiveEntry (entries, zipEntries[zipEntries.SelectedEntry]).Open ())
               using (var threadedReader = new ThreadedIOBlockReader (entryStrm, true, 64 * 1024))
                  loadStreamIntoMemory (threadedReader, new LoadProgress (this, -1), false);
            }
         }
      }

      /// <summary>
      /// Load a gz file.
      /// This is done by unzipping it into memory and serving the UI from the memorystream
      /// Unzip is preferrable done by starting gzip, and otherwise by using sharpzlib
      /// </summary>
      private void loadGZipFile (String fn, CancellationToken ct) {
         Stream strm = null;
         Thread cur = Thread.CurrentThread;
         bool isBackground = cur.IsBackground;
         cur.IsBackground = false;  //Make sure that the process is kept alive as long as this thread is alive
         try {
            if (Globals.CanInternalGZip && (DbgStr == null || DbgStr == "intern")) {
               logger.Log ("Loading '{0}' via internal Zlib.DLL", fn);
               strm = new FileStream (fn, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024);
               var gz = new GZipDecompressStream (strm, false, 64 * 1024);
               strm = gz;
            }

            if (strm == null) {
               logger.Log ("Loading '{0}' via SharpZipLib.", fn);
               strm = new FileStream (fn, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
               var gz = new ICSharpCode.SharpZipLib.GZip.GZipInputStream (strm);
               gz.IsStreamOwner = true;
               strm = gz;
            }
            using (var rdr = new ThreadedIOBlockReader (strm, false, 64 * 1024))
               loadStreamIntoMemory (rdr, new LoadProgress (this, -1), false);
         } catch (Exception e) {
            throw IOUtils.WrapFilenameInException (e, fn);
         } finally {
            Utils.FreeAndNil (ref strm);
            cur.IsBackground = isBackground; //Restore the background state
         }
      }

      /// <summary>
      /// Tries to load a stream into a compressed memory buffer.
      /// If compression fails, we fallback to a non-compressed memory buffer.
      /// If the size of the file is too large, and we load from a filestream, we simply exit
      /// and let our caller do the rest
      /// </summary>
      private bool loadStreamIntoMemory (IBlockReader strm, LoadProgress loadProgress, bool allowDegradeToNonMem) {
         const int chunksize = 256 * 1024;
         Logger compressLogger = Globals.MainLogger.Clone ("compress");
         Stream mem;
         if (settings.CompressMemoryIfBigger <= 0)
            mem = new CompressedChunkedMemoryStream (chunksize, compressLogger);
         else
            mem = new ChunkedMemoryStream (chunksize);

         threadCtx = new ThreadContext (encoding, mem as IDirectStream, this.partialLines);

         IOBlock buffer = null;
         long position = 0;
         bool checkCompress = true;
         bool checkDegrade = allowDegradeToNonMem;
         while (true) {
            buffer = strm.GetNextBuffer (buffer);
            if (buffer == null) break;

            mem.Write (buffer.Buffer, 0, buffer.Length);
            if (detectedEncoding == null) {
               detectedEncoding = new FileEncoding (buffer);
               AddLine (detectedEncoding.PreambleBytes);
            }
            position = addLinesForBuffer (buffer.Position, buffer.Buffer, buffer.Length);
            if (!loadProgress.HandleProgress (position)) continue;


            logger.Log (_LogType.ltProgress, "Handle progress pos={0}", Pretty.PrintSize (position));
            if (mem is ChunkedMemoryStream) {
               if (checkCompress && position > settings.CompressMemoryIfBigger) {
                  logger.Log (_LogType.ltWarning, "Switching to compressed since size {0} > {1}.", Pretty.PrintSize (position), Pretty.PrintSize (settings.CompressMemoryIfBigger));
                  checkCompress = false;
                  mem = ((ChunkedMemoryStream)mem).CreateCompressedChunkedMemoryStream (compressLogger);
                  threadCtx = new ThreadContext (encoding, mem as IDirectStream, this.partialLines);
               }
               continue;
            }

            //then it is a CompressedChunkedMemoryStream
            if (checkDegrade) {
               var compress = mem as CompressedChunkedMemoryStream;
               if (compress.IsCompressionEnabled) continue;
               logger.Log ("Checking degrade: compr={0}, fs={1}", compress.IsCompressionEnabled, loadProgress.FileSize);
               long limit = settings.AvailablePhysicalMemory - 800 * 1024 * 1024;
               if (loadProgress.FileSize > limit) {
                  logger.Log (_LogType.ltWarning, "Switching back to uncached loading, since the filesize({0}) more than the available memory {1}",
                     Pretty.PrintSize (loadProgress.FileSize), Pretty.PrintSize (limit));
                  goto EXIT_DEGRADED;
               }
               checkDegrade = false; //Don't check this again
            }

            if (loadProgress.ShouldDegrade ()) goto EXIT_DEGRADED;
         }

         addSentinelForLastPartial (position);
         return true;


      EXIT_DEGRADED:;
         mem.Dispose ();
         mem = null;
         GC.Collect (GC.MaxGeneration, GCCollectionMode.Forced, true);
         return false;
      }

      private void dumpOffsets (int N) {
         if (partialLines.Count < N) N = partialLines.Count;
         logger.Log ();
         logger.Log ("Dumping {0} of {1} partial lines. Shift={2}, mask={3:X}", N, partialLines.Count, LineFlags.FLAGS_SHIFT, LineFlags.FLAGS_MASK);
         long prev = 0;
         for (int i = 0; i < N; i++) prev = dumpOffset (prev, i, null);
      }
      private long dumpOffset (long prev, int i, String why) {
         String data = GetPartialLine (i);
         int N = Math.Min (32, data.Length);
         String start = data.Substring (0, N);
         String end = data.Substring (data.Length-N);
         long x = partialLines[i];
         long offs = x >> LineFlags.FLAGS_SHIFT;
         long mask = x & LineFlags.FLAGS_MASK;
         logger.Log ("-- {0}: o={1} (0x{1:X}), len={2}, flags=0x{3:X},    start={4},    end={5},    rsn={6}", i, offs, offs-prev, mask, start, end, why);
         return offs;
      }
      private void loadNormalFile (string fn, CancellationToken ct) {
         var directStream = new DirectFileStreamWrapper (fn, 4096);
         var fileStream = directStream.BaseStream;
         var rdr = new ThreadedIOBlockReader (fileStream, true, 64 * 1024);

         long totalLength = fileStream.Length;
         var loadProgress = new LoadProgress (this, fileStream.Length);

         logger.Log ("Loading '{0}' via FileStream.", fn);
         if (totalLength >= settings.LoadMemoryIfBigger) {
            long limit = settings.AvailablePhysicalMemory - 800 * 1024 * 1024;
            if (totalLength >= settings.CompressMemoryIfBigger) limit *= 3;
            if (totalLength < limit) {
               logger.Log ("-- Loading into memory since '{0}' is between {1} and {2}.",
                  Pretty.PrintSize (totalLength),
                  Pretty.PrintSize (settings.LoadMemoryIfBigger),
                  Pretty.PrintSize (limit));
               if (loadStreamIntoMemory (rdr, loadProgress, true)) return;
               //Fallthrough, since the compression failed. We fallback to filestream loading
            }
         }


         //Loading the complete file (or last piece of the file) from the filestream itself.
         //This involves creating a directStream from it.
         this.threadCtx = new ThreadContext (encoding, directStream, partialLines);
         long position = fileStream.Position;
         IOBlock buffer = null;
         while (true) {
            buffer = rdr.GetNextBuffer (buffer);
            if (buffer == null) break;

            if (detectedEncoding == null) {
               detectedEncoding = new FileEncoding (buffer);
               AddLine (detectedEncoding.PreambleBytes);
              //pw buffer.Position = detectedEncoding.PreambleBytes;
            }

            position = addLinesForBuffer (buffer.Position, buffer.Buffer, buffer.Length);
            loadProgress.HandleProgress (position);
         }
         addSentinelForLastPartial (position);
      }

      private void addSentinelForLastPartial (long position) {
         long o2 = partialLines[partialLines.Count - 1] >> LineFlags.FLAGS_SHIFT;
         if (o2 != position)
            AddLine (position);
      }

      private bool onProgress (double perc) {
         if (disposed) return false;
         cb.OnProgress (this, (int)perc);
         return true;
      }

      static byte[] setLimiters (byte[] arr, int v, String seps) {
         for (int i=0; i<seps.Length; i++) {
            arr[(int)seps[i]] = (byte)v;   
         }
         return arr;
      }
      static byte[] seps = setLimiters (setLimiters (new byte[256], 1, ">:"), 2, " \t.,;");
      
      
      //Optimized line splitter
      //We use fixed to avoid array limit checking
      //A separator byte[] is used to remove a switch statement
      //The 1st chance limiters are important for json and xml. We don't use a comma or a dot here, since
      //that would potentialy result in splitted numbers
      private unsafe long addLinesForBuffer (long position, byte[] buf, int count) {
         long prev = partialLines[partialLines.Count - 1] >> LineFlags.FLAGS_SHIFT;
         int partialLimit = maxPartialSize - (int)(position - prev);  //limit from where to split
         fixed (byte* p = buf) {
            int i=0;
            int N;
            while (true) {
               N = Math.Min (partialLimit, count);
               for (; i < N; i++) {
                  if (p[i] != (byte)10) continue;
                  AddLine (position + i + 1);
                  partialLimit = maxPartialSize + i + 1;
                  N = Math.Min (partialLimit, count);
               }
               if (i >= count) break; //Line too long and spans buffers: exit

               //OK, we have an unvoluntary line break
               int end = i;
               int limit = i - 32;
               if (limit < 0) limit = 0;
               fixed (byte* q = seps) {
                  //1st chance limiters
                  for (; end >= limit; end--) {
                     if (q[p[end]] == 1) goto PARTIAL_ADD;
                  }
                  //2nd chance limiters
                  for (end = i; end >= limit; end--) {
                     if (q[p[end]] == 2) goto PARTIAL_ADD;
                  }
               }
               end = i-1; //compensate for the +1 in partial add

            PARTIAL_ADD:
               ++end;
               AddPartialLine (position + end);
               partialLimit = maxPartialSize + end;
               i++;
            }
         }
         return position + count;
      }


      /// <summary>
      /// Load a (zip) file
      /// </summary>
      public Task Load (string fn, CancellationToken ct, String zipEntry = null) {
         zipEntries = null;
         return Task.Run (() => {
            DateTime startTime = DateTime.UtcNow;
            Exception err = null;
            partialsEncountered = false;
            this.fileName = Path.GetFullPath (fn);
            this.ct = ct;
            try {
               if (String.Equals (".gz", Path.GetExtension (fileName), StringComparison.OrdinalIgnoreCase))
                  loadGZipFile (fileName, ct);
               else if (zipEntry != null || String.Equals (".zip", Path.GetExtension (fileName), StringComparison.OrdinalIgnoreCase))
                  loadZipFile (fileName, ct, zipEntry);
               else
                  loadNormalFile (fileName, ct);
               logger.Log ("-- Loaded. Size={0}, #Lines={1}", Pretty.PrintSize (GetPartialLineOffset (partialLines.Count - 1)), partialLines.Count - 1);
            } catch (Exception ex) {
               err = ex;
               Logs.ErrorLog.Log (ex, "Exception during load: {0}", ex.Message);
            } finally {
               this.ct = CancellationToken.None;
               try {
                  if (threadCtx != null) {
                     var c = threadCtx.DirectStream as CompressedChunkedMemoryStream;
                     if (c != null) {
                        c.FinalizeCompressor (true);
                        logger.Log ("File len={0}, compressed={1}, ratio={2:F1}%", c.Length, c.GetCompressedSize (), 100.0 * c.GetCompressedSize () / c.Length);
                     }
                  }

                  int maxBufferSize = finalizeAdministration ();
                  logger.Log ("SetMaxBufferSize ({0}, {1})", Pretty.PrintSize (maxBufferSize), maxBufferSize);
                  if (threadCtx != null) threadCtx.SetMaxBufferSize (maxBufferSize);
               } catch (Exception e2) {
                  Logs.ErrorLog.Log (e2, "Exception after load: {0}", e2.Message);
                  if (err == null) err = e2;
                  if (threadCtx != null) threadCtx.CloseInstance ();
               }
               if (!disposed)
                  cb.OnLoadComplete (new Result (this, startTime, err));
               //dumpOffsets (100);
            }
         });
      }

      public void Dispose () {
         disposed = true;

         this.partialLines.Clear ();
         this.lines = null;
         if (this.threadCtx != null) {
            this.threadCtx.Close ();
         }
      }


      public void ResetMatches () {
         resetFlags (LineFlags.MATCHED);
      }
      public void ResetMatchesAndFlags () {
         resetFlags (LineFlags.RESETTABLE_FLAGS);
      }
      private void resetFlags (int mask) {
         long notmask = ~(long)mask;
         logger.Log ("resetFlags ({0:X}: not={1:X})", mask, notmask);
         for (int i = 0; i < partialLines.Count; i++) {
            partialLines[i] = notmask & partialLines[i];
         }
      }

      public List<int> GetSelectedPartialLines (int maxCount, out bool truncated) {
         var ret = new List<int> ();
         truncated = false;
         for (int i = 0; i < partialLines.Count; i++) {
            if ((partialLines[i] & LineFlags.SELECTED) == 0) continue;
            if (ret.Count >= maxCount) {
               truncated = true; break;
            }
            ret.Add (i);
         }
         return ret;
      }

      public void MarkSelected (int from, int to) {
         for (int i = from; i < to; i++) {
            partialLines[i] |= LineFlags.SELECTED;
         }
      }

      public void MarkUnselected (int from, int to) {
         logger.Log (_LogType.ltTimerStart, "MarkUnselected"); //PW clear later
         long mask = ~LineFlags.SELECTED;
         for (int i = from; i < to; i++) {
            partialLines[i] &= mask;
         }
         logger.Log (_LogType.ltTimerStop, "MarkUnselected took");
      }

      public void ToggleSelected (int from, int to) {
         long mask = LineFlags.SELECTED;
         for (int i = from; i < to; i++) {
            partialLines[i] ^= mask;
         }
      }

      public void SelectAllMatched () {
         for (int i = 0; i < PartialLineCount; i++) {
            if ((LineFlags.MATCHED & (int)partialLines[i]) != 0)
               partialLines[i] |= LineFlags.SELECTED;
         }
      }
      public void SelectAllNonMatched () {
         for (int i = 0; i < PartialLineCount; i++) {
            if ((LineFlags.MATCHED & (int)partialLines[i]) == 0)
               partialLines[i] |= LineFlags.SELECTED;
         }
      }
      public void UnselectAllMatched () {
         long mask = ~(long)LineFlags.SELECTED;
         for (int i = 0; i < PartialLineCount; i++) {
            if ((LineFlags.MATCHED & (int)partialLines[i]) != 0)
               partialLines[i] &= mask;
         }
      }
      public void UnselectAllNonMatched () {
         long mask = ~(long)LineFlags.SELECTED;
         for (int i = 0; i < PartialLineCount; i++) {
            if ((LineFlags.MATCHED & (int)partialLines[i]) == 0)
               partialLines[i] &= mask;
         }
      }

      public SearchResult SearchAllSingleLines () {
         DateTime started = DateTime.UtcNow;
         long mask = LineFlags.MATCHED;
         long notmask = ~mask;
         int count = 0;
         int firstHit = -1;
         if (lines == null) {
            count = partialLines.Count - 1;
            if (count > 0) firstHit = 0;
            for (int i = partialLines.Count - 1; i >= 0; i--) partialLines[i] |= mask;
         } else {
            for (int i = lines.Count - 1; i > 0; i--) {
               int pix = lines[i - 1];
               if (pix == lines[i] - 1) {
                  partialLines[pix] |= mask;
                  count++;
                  firstHit = pix;
               } else {
                  partialLines[pix] &= notmask;
               }
            }
         }
         return new SearchResult (this, started, null, count, firstHit, 0);
      }
      public SearchResult SearchAllMultiLines () {
         DateTime started = DateTime.UtcNow;
         long mask = LineFlags.MATCHED;
         long notmask = ~mask;
         int count=0;
         int firstHit=-1;
         if (lines==null) {
            for (int i = partialLines.Count - 1; i >= 0; i--) partialLines[i] &= notmask;
         } else {
            for (int i = lines.Count - 1; i > 0; i--) {
               int pix = lines[i - 1];
               if (pix == lines[i] - 1) {
                  partialLines[pix] &= notmask;
               } else {
                  partialLines[pix] |= mask;
                  count++;
                  firstHit = pix;
               }
            }
         }
         return new SearchResult (this, started, null, count, firstHit, 0);
      }

      public Task Search (ParserNode<SearchContext> query, CancellationToken ct) {
         if (threadCtx == null) return Task.CompletedTask;
         threadCtx.DirectStream.PrepareForNewInstance ();
         return Task.Run (() => {
            this.ct = ct;
            logger.Log ("Search: phase1 starting with {0} threads", settings.SearchThreads);
            logger.Log ("Search: encoding: {0}", encoding);
            var ctx = new SearchContext (query);
            foreach (var x in ctx.LeafNodes)
               logger.Log ("-- {0}", x);

            DateTime start = DateTime.UtcNow;
            Exception err = null;
            int matches = 0;
            int firstHit = -1;

            try {
               Task<int>[] tasks = new Task<int>[settings.SearchThreads - 1];
               int N = partialLines.Count - 1;
               int M = N / settings.SearchThreads;
               for (int i = 0; i < tasks.Length; i++) {
                  int end = N;
                  N -= M;
                  tasks[i] = Task.Run (() => _search (ctx.NewInstanceForThread (), end - M, end));
               }
               int matches0 = matches = _search (ctx, 0, N);
               for (int i = 0; i < tasks.Length; i++) {
                  matches += tasks[i].Result;
                  tasks[i].Dispose ();
               }

               if (matches0 == 0 && matches > 0) {
                  firstHit = this.NextPartialHit (N-1);
                  if (firstHit >= 0)
                     cb.OnSearchPartial (this, firstHit);
               }

               ctx.MarkComputed ();
               logger.Log ("Search: phase1 ended. Total #matches={0}", matches);
            } catch (Exception e) {
               err = e;
            } finally {
               this.ct = CancellationToken.None;
               if (!disposed)
                  cb.OnSearchComplete (new SearchResult (this, start, err, matches, firstHit, ctx.LeafNodes.Count));
            }
         });
      }

      private int _search (SearchContext ctx, int start, int end) {
         const int MOD = 50000;
         int matches = 0;
         var query = ctx.Query;

         logger.Log ("Search: thread start: from {0} to {1} ", start, end);
         if (ctx.NeedLine) goto NEEDLINE;
         for (ctx.Index = start; ctx.Index < end; ctx.Index++) {
            if (ctx.Index % MOD == 0) {
               checkCancelled ();
               if (start == 0 && !onProgress ((100.0 * ctx.Index) / end)) break;
            }

            ctx.SetLine (partialLines[ctx.Index], null);
            if (!query.Evaluate (ctx)) {
               partialLines[ctx.Index] = ctx.OffsetAndFlags;
               continue;
            }

            partialLines[ctx.Index] = ctx.OffsetAndFlags | LineFlags.MATCHED;

            if (++matches == 1 && !disposed && start == 0)
               cb.OnSearchPartial (this, ctx.Index);
         }
         goto EXIT_RTN;


      NEEDLINE:;
         var threadCtx = this.threadCtx.NewInstanceForThread ();
         try {
            for (ctx.Index = start; ctx.Index < end; ctx.Index++) {
               if (ctx.Index % MOD == 0) {
                  if (ct.IsCancellationRequested) break;
                  if (start == 0 && !onProgress ((100.0 * ctx.Index) / end)) break;
               }

               ctx.SetLine (partialLines[ctx.Index], threadCtx.ReadPartialLineInBuffer (ctx.Index, ctx.Index + 1));
               if (!query.EvaluateDeep (ctx)) {
                  partialLines[ctx.Index] = ctx.OffsetAndFlags; ;
                  continue;
               }

               partialLines[ctx.Index] = ctx.OffsetAndFlags | LineFlags.MATCHED;

               if (++matches == 1 && !disposed && start == 0)
                  cb.OnSearchPartial (this, ctx.Index);
            }
         } finally {
            threadCtx.CloseInstance ();
         }
         goto EXIT_RTN;

      EXIT_RTN:
         logger.Log ("Search: thread end: from {0} to {1}: {2} out of {3}", start, end, matches, end - start);
         return matches;
      }

      /// <summary>
      /// Convert a list of indexes to partial lines into a list of lines-indexes
      /// </summary>
      public List<int> ConvertToLines (List<int> partialIndexes) {
         if (lines == null)
            return partialIndexes;

         var ret = new List<int> (1024);

         int n = 0; int m = 0;
         int N = partialIndexes.Count;
         int M = lines.Count - 1;
         while (n < N && m < M) {
            if (partialIndexes[n] < lines[m]) {
               n++;
               continue;
            }
            if (partialIndexes[n] < lines[m + 1]) {
               ret.Add (m);
               ++m;
               continue;
            }
            ++m;
         }
         return ret;
      }

      /// <summary>
      /// Export all lines to the supplied filePath
      /// This will be one by just copying the bytes. So, no encoding is involved
      /// </summary>
      public Task Export (string filePath, CancellationToken ct) {
         return Export (null, filePath, ct);
      }

      /// <summary>
      /// Export the selected lines to the supplied filePath
      /// If selectedLines==null, all lines will be copied
      /// This will be one by just copying the bytes. So, no encoding is involved
      /// 
      /// SelectedLines should contain line-indexes, not partial line indexes
      /// </summary>
      [Flags]
      private enum _ExportFlags { Lines = 1, Filter = 2 };
      public Task Export (List<int> _selectedlines, string filePath, CancellationToken _ct) {
         return Task.Run (() => {
            var ct = _ct;
            var selectedlines = _selectedlines;
            _ExportFlags flags = 0;
            if (selectedlines != null) flags |= _ExportFlags.Filter;
            if (lines != null) flags |= _ExportFlags.Lines;
            Exception err = null;
            DateTime startTime = DateTime.UtcNow;
            var ctx = threadCtx.NewInstanceForThread ();

            int N = 0;
            switch (flags) {
               default: flags.ThrowUnexpected (); break; //To check that we only have 4 possibilities
               case 0:
                  N = PartialLineCount; break;
               case _ExportFlags.Lines:
                  N = LineCount; break;
               case _ExportFlags.Filter:
               case _ExportFlags.Filter | _ExportFlags.Lines:
                  N = selectedlines.Count; break;
            }

            try {
               using (Stream fs = new FileStream (filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 16 * 1024)) {
                  Stream strm = fs;
                  if (String.Equals (Path.GetExtension (filePath), ".gz", StringComparison.OrdinalIgnoreCase))
                     strm = wrapToGZipCompressStream (fs);

                  byte[] endLine = new byte[2] { 13, 10 };
                  int perc = 0;
                  int nextPercAt = 0;

                  this.ct = ct;
                  for (int i = 0; i < N; i++) {
                     int ix, start, end;
                     switch (flags) {
                        default:
                           start = i; end = i + 1; break;
                        case _ExportFlags.Lines:
                           start = lines[i]; end = lines[i + 1]; break;
                        case _ExportFlags.Filter:
                           ix = selectedlines[i];
                           start = ix; end = ix + 1; break;
                        case _ExportFlags.Filter | _ExportFlags.Lines:
                           ix = selectedlines[i];
                           start = lines[ix]; end = lines[ix + 1]; break;
                     }
                     //Write partial lines one by one (to prevent huge buffer usage)
                     for (int j = start; j < end; j++) {
                        int len = ctx.ReadPartialLineBytesInBuffer (j, j + 1);
                        strm.Write (ctx.ByteBuffer, 0, len);
                     }
                     // Add \r\n
                     strm.Write (endLine, 0, 2);

                     if (i < nextPercAt) continue;

                     checkCancelled ();
                     cb.OnProgress (this, perc);
                     perc++;
                     nextPercAt = (int)(perc * N / 100.0);
                  }
                  if (strm != fs)
                     closeGZipCompressStream (strm);
               }
            } catch (Exception e) {
               err = e;
            } finally {
               this.ct = CancellationToken.None;
               ctx.CloseInstance ();
               if (!disposed) cb.OnExportComplete (new ExportResult (this, startTime, err, N));
            }
         });
      }

      private Stream wrapToGZipCompressStream (Stream fs) {
         if (Globals.CanInternalGZip) {
            logger.Log ("Export via internal ZLib");
            return new GZipCompressStream (fs, false, ZLibCompressionLevel.Default, 4 * 1024);
         }

         logger.Log ("Saving via SharpZipLib.");
         var gz = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream (fs);
         gz.IsStreamOwner = true;
         return gz;
      }

      private void closeGZipCompressStream (Stream strm) {
         var gz2 = strm as ICSharpCode.SharpZipLib.GZip.GZipOutputStream;
         if (gz2 != null) {
            gz2.Finish ();
            gz2.Close ();
            return;
         }
         strm.Close ();
      }


      private void AddLine (long offset) {
         //logger.Log ("AddLine({0}: {1} (0x{1:X})", partialLines.Count, offset);
         partialLines.Add (offset << LineFlags.FLAGS_SHIFT);
         //long x = partialLines[partialLines.Count-1];
         //long offs = x >> LineFlags.FLAGS_SHIFT;
         //long mask = x & LineFlags.FLAGS_MASK;
         //logger.Log ("-- o={0} (0x{0:X}), flags=0x{1:X}", offs, mask);
      }
      private void AddPartialLine (long offset) {
         //logger.Log ("AddPartial({0}: {1} (0x{1:X})", partialLines.Count, offset);
         partialLines.Add ((offset << LineFlags.FLAGS_SHIFT) | LineFlags.CONTINUATION);
         partialsEncountered = true;
         //long x = partialLines[partialLines.Count - 1];
         //long offs = x >> LineFlags.FLAGS_SHIFT;
         //long mask = x & LineFlags.FLAGS_MASK;
         //logger.Log ("-- o={0} (0x{0:X}), flags=0x{1:X}", offs, mask);
      }

      public long GetPartialLineOffset (int line) {
         //logger.Log("PartialLineOffset[{0}]: raw={1:X}, shift={2:X}", line, partialLines[line], (int)(partialLines[line] >> FLAGS_SHIFT));
         return (partialLines[line] >> LineFlags.FLAGS_SHIFT);
      }
      public long GetPartialLineOffsetAndFlags (int line) {
         return partialLines[line];
      }
      public long GetLineOffset (int line) {
         if (lines != null) line = lines[line];
         return (int)(partialLines[line] >> LineFlags.FLAGS_SHIFT);
      }
      public int LineToPartialLineIndex (int line) {
         return lines == null ? line : lines[line];
      }



      /// <summary>
      /// Get a partial line
      /// </summary>
      public string GetPartialLine (int index, int maxChars = -1, ICharReplacer replacer = null) {
         //dumpOffset (index, "GetPartialLine");
         //logger.Log("GetPartialLine: index={0}, count={1}", index, partialLines.Count - 1);
         if (index < 0 || index >= partialLines.Count - 1) return String.Empty;
         return threadCtx.GetPartialLine (index, index + 1, maxChars, replacer);
      }

      /// <summary>
      /// Get length of a partial line in #bytes
      /// </summary>
      public int GetPartialLineLengthInBytes (int index) {
         if (index < 0 || index >= partialLines.Count - 1) return 0;
         return (int)((partialLines[index + 1] >> LineFlags.FLAGS_SHIFT) - (partialLines[index] >> LineFlags.FLAGS_SHIFT));
      }

      /// <summary>
      /// Get length of a partial line in #chars
      /// </summary>
      public int GetPartialLineLengthInChars (int index) {
         if (index < 0 || index >= partialLines.Count - 1) return 0;
         String tmp = threadCtx.GetPartialLine (index, index + 1, -1, null);
         return tmp.Length;
      }


      /// <summary>
      /// Get length of a line in #chars
      /// </summary>
      public int GetLineLengthInBytes (int index) {
         if (lines == null) return GetPartialLineLengthInBytes (index);
         if (index < 0 || index >= lines.Count - 1) return 0;
         return (int)((partialLines[lines[index + 1]] >> LineFlags.FLAGS_SHIFT) - (partialLines[lines[index]] >> LineFlags.FLAGS_SHIFT));
      }


      /// <summary>
      /// Get length of a line in #chars
      /// </summary>
      public int GetLineLengthInChars (int index) {
         if (lines == null) return GetPartialLineLengthInChars (index);
         if (index < 0 || index >= lines.Count - 1) return 0;
         String tmp = threadCtx.GetLine (index, index + 1, -1, out bool truncated);
         return tmp.Length;
      }
      public int GetLineLengthInChars (int index, out bool truncated) {
         truncated = false;
         if (lines == null) return GetPartialLineLengthInChars (index);
         if (index < 0 || index >= lines.Count - 1) return 0;
         String tmp = threadCtx.GetLine (index, index + 1, -1, out truncated);
         return tmp.Length;
      }

      /// <summary>
      /// Get a complete line
      /// </summary>
      public String GetLine (int index, out bool truncated) {
         truncated = false;
         if (index < 0) return String.Empty;
         if (lines != null) {
            if (index >= lines.Count - 1) return String.Empty;
            return threadCtx.GetLine (lines[index], lines[index + 1], settings.MaxLineLength, out truncated);
         }
         if (index >= partialLines.Count - 1) return String.Empty;
         return threadCtx.GetLine (index, index + 1, settings.MaxLineLength, out truncated);
      }

      /// <summary>
      /// Get a complete line
      /// </summary>
      public String GetLine (int index) {
         bool truncated;
         return GetLine (index, out truncated);
      }

      public int GetLineFlags (int line) {
         return (LineFlags.FLAGS_MASK & (int)partialLines[line]);
      }


      /// <summary>
      /// Returns the index of the partialline in the filter
      /// </summary>
      public int PartialToLogicalIndex (int partialIndex, List<int> partialFilter) {
         if (partialFilter == null) return partialIndex;
         int i = -1;
         int j = partialFilter.Count;
         while (j - i > 1) {
            int m = (i + j) / 2;
            if (partialFilter[m] >= partialIndex) j = m; else i = m;
         }
         return j;
      }

      /// <summary>
      /// For a given line, return the partial line index where it start
      /// </summary>
      public int PartialFromLineNumber (int line) {
         if (lines == null) return line;
         if (line >= 0)
            line = (line >= LineCount) ? PartialLineCount : lines[line];
         return line;
      }

      /// <summary>
      /// For a given partial line number, return the line that contains the partial
      /// </summary>
      public int PartialToLineNumber (int partial) {
         if (lines == null) return partial;
         int i = -1;
         int j = lines.Count;
         while (j - i > 1) {
            int m = (i + j) / 2;
            if (lines[m] > partial) j = m; else i = m;
         }
         return i;
      }
      public int GetOptRealLineNumber (int line) {
         if (lines == null) return line;
         return ((int)partialLines[line] & LineFlags.CONTINUATION) == 0 ? PartialToLineNumber (line) : -1;
      }

      public void CopyStateBitsFrom (LogFile lf) {
         var oldList = lf.partialLines;
         var newList = partialLines;

         if (newList.Count < oldList.Count || FileName != lf.fileName)
            throw new BMException ("Cannot assign flags: old={0} ({1} lines), new={2} ({3} lines)", lf.fileName, oldList.Count, fileName, newList.Count);

         for (int i=oldList.Count-1; i>=0; i--) {
            newList[i] = (newList[i] & ~LineFlags.FLAGS_MASK) | (oldList[i] & LineFlags.FLAGS_MASK);
         }
      }

      public PartialLineLength GetLongestLineInBytes () {
         return this.largestPartialLine;
      }
      public PartialLineLength GetLongestLineInChars () {
         if (lines == null) return largestPartialLine;
         PartialLineLength ret= PartialLineLength.ZERO;

         for (int i =0; i< largestPartialLines.Length; i++) {
            var ll = largestPartialLines[i];
            int tmp = threadCtx.GetPartialLineLengthInChars (ll.Index);
            logger.Log ("Longest partial line: at {0}, bytes={1}, chars={2} ({3})", ll.Index, GetPartialLineLengthInBytes (ll.Index), tmp, GetPartialLineLengthInChars (ll.Index));
            if (ret==null || tmp > ret.Length) {
               ret = new PartialLineLength (ll.Index, tmp);
            }
         }
         return ret;
      }



      /// <summary>
      /// Helper class to handle the progress during loading
      /// </summary>
      private class LoadProgress {
         const long ticksPerSeconds = TimeSpan.TicksPerMillisecond * 1000;
         protected readonly LogFile parent;
         public readonly long FileSize;
         protected readonly long startTime;
         protected long reloadAt;
         protected long deltaReloadAt;

         private int prevPerc;
         //private Logger logger = Globals.MainLogger.Clone("progress");

         public LoadProgress (LogFile parent, long fileSize) {
            this.parent = parent;
            this.FileSize = fileSize;
            prevPerc = -1;
            startTime = DateTime.UtcNow.Ticks;
            deltaReloadAt = 2 * ticksPerSeconds;
            reloadAt = startTime + ticksPerSeconds;
         }

         public virtual bool ShouldDegrade () {
            return false;
         }
         public virtual bool HandleProgress (long pos) {
            parent.checkCancelled (pos);
            long ticks = DateTime.UtcNow.Ticks;
            int cur;
            if (FileSize > 0)
               cur = perc (pos, FileSize);
            else {
               cur = (int)(20 * (ticks - startTime) / ticksPerSeconds) % 201;
               if (cur > 100) cur = 200 - cur;
            }
            if (cur != prevPerc) {
               parent.cb.OnProgress (parent, cur);
               prevPerc = cur;
            }

            if (ticks > reloadAt) {
               reloadAt += deltaReloadAt;
               deltaReloadAt = (long)(1.5 * deltaReloadAt);
               parent.threadCtx.DirectStream.PrepareForNewInstance ();
               parent.cb.OnLoadCompletePartial (new LogFile (parent));
               return true;
            }
            return false;
         }

         static int perc (long partial, long all) {
            if (all == 0) return 0;
            return (int)((100.0 * partial) / all);
         }

      }
   }

}