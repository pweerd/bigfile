/*
 * Copyright 2022, De Bitmanager
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
using System.IO.IsolatedStorage;
using Bitmanager.Storage;
using System.Reflection;

namespace Bitmanager.BigFile {

   /// <summary>
   /// LogFile is responsible for loading the data and splitting it into lines.
   /// </summary>
   public class LogFile {
      static public String DbgStr = "gzip";
      static readonly Logger logger = Globals.MainLogger.Clone ("logfile");
      static readonly byte[] EMPTY_BYTES = new byte[0];

      private ThreadContext threadCtx;
      private readonly List<long> partialLines;
      private List<int> lines;
      private ZipEntries zipEntries;
      public ZipEntries ZipEntries { get { return zipEntries; } }
      public PartialLineStats LongestPartialLine => largestPartialLine;
      public int LongestLineIndex => largestLineIndex;
      public int PartialLineCount { get { return partialLines.Count - 1; } }
      public long Size { get { return partialLines[partialLines.Count - 1] >> LineFlags.FLAGS_SHIFT; } }
      public int LineCount { get { return lines == null ? partialLines.Count - 1 : lines.Count - 1; } }
      public String FileName { get { return fileName; } }
      private FileEncoding detectedEncoding;
      public FileEncoding DetectedEncoding => detectedEncoding;

      private Encoding encoding = Encoding.UTF8;

      private PartialLineStats[] largestPartialLines;
      private PartialLineStats largestPartialLine;
      private int largestLineIndex;

      private int toSkip;
      private int skippedLines;
      private readonly long skipSize;
      private long loadedDataOffset;
      private enum SkipMode { None, Lines, Size};
      private SkipMode skipMode;
      public bool IsSkipping => skipMode != SkipMode.None;
      public long SkippedSize => partialLines[0] >> LineFlags.FLAGS_SHIFT;


      public PartialLineStats[] LargestPartialLines => largestPartialLines;
      public int SkippedLines => skippedLines;

      public IDirectStream DirectStream { get { return threadCtx?.DirectStream; } }

      public Encoding SetEncoding (Encoding c) {
         var old = encoding;
         encoding = c ?? Encoding.UTF8;
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

      private readonly long initialMaxLoadSize; //maxLoadSize interferes with skipping. Only after skipping, the maxLoadSize is known.
      private long actualMaxLoadSize;

      private CancellationToken ct;
      private bool disposed;
      public bool Disposed { get { return disposed; } }
      private bool partialsEncountered;
      private void checkCancelled () {
         if (disposed || ct.IsCancellationRequested)
            throw new TaskCanceledException ();
      }
      private void checkCancelled (long pos) {
         if (disposed || ct.IsCancellationRequested || pos >= actualMaxLoadSize)
            throw new TaskCanceledException ();
      }

      public LogFile (ILogFileCallback cb, Settings settings, Encoding enc, int maxPartialSize, long maxLoadSize=0, long toSkip = 0) {
         this.initialMaxLoadSize = maxLoadSize > 0 ? maxLoadSize : long.MaxValue;
         this.actualMaxLoadSize = long.MaxValue;
         if (toSkip == 0) {
            skipMode = SkipMode.None;
            actualMaxLoadSize = initialMaxLoadSize;
         }  else if (toSkip < 0) {
            skipMode = SkipMode.Size;
            skipSize = -toSkip;
         } else {
            skipMode = SkipMode.Lines;
            this.toSkip = (int)toSkip;
         }

         if (maxPartialSize <= 0) this.maxPartialSize = (int.MaxValue - 1024*1024); //min 1MB, otherwise there will be an overflow in addLinesForBuffer
         else if (maxPartialSize < 256) this.maxPartialSize = 256;
         else if (maxPartialSize > 4096) this.maxPartialSize = 4096;
         else this.maxPartialSize = maxPartialSize;

         this.cb = cb;

         SyncSettings (settings, enc);
         partialLines = new List<long> ();
         loadedDataOffset = -1;
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
         this.initialMaxLoadSize = other.initialMaxLoadSize;
         this.actualMaxLoadSize = other.actualMaxLoadSize;
         this.skippedLines = other.skippedLines;
         this.skipMode= other.skipMode;
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
            return zipEntries.SelectedItemIndex == other.zipEntries.SelectedItemIndex;
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
         //dumpOffsets (20);
         if (partialsEncountered) {
            lines = new List<int> ();
            lines.Add (0);
         } else
            lines = null;

         if (partialLines.Count == 0) partialLines.Add (0);

         largestPartialLine = null;
         largestLineIndex = -1;

         var prique = new LargestLines ();

         long prevPartial = partialLines[0] >> LineFlags.FLAGS_SHIFT;
         long prevLine = prevPartial;
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

            if (len > prique.Min.Length) prique.Add (new PartialLineStats(i-1, len));
         }

         if (prique.Count>0) {
            largestPartialLines = prique.ToArray ();
            //for (int i = largestPartialLines.Length - 1; i >= 0; i--) logger.Log ("prique[{0}]: {1}", i, largestPartialLines[i]);

            PartialLineStats max = largestPartialLines[0];
            for (int i = largestPartialLines.Length - 1; i > 0; i--)
               if (largestPartialLines[i].Length > max.Length) max = largestPartialLines[i];
            largestPartialLine = max;
            largestLineIndex = lines != null ? maxLineIdx : max.Index;

            return max.Length;
         }
         return 0;
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

      static ZipArchiveEntry getZipArchiveEntry (ReadOnlyCollection<ZipArchiveEntry> entries, ZipEntry e) {
         foreach (var zae in entries) {
            if (zae.FullName == e.FullName) return zae;
         }
         throw new BMException ("Cannot find '{0}' in archive '{1}'.", e.FullName, e.ArchiveName);
      }
      static ICSharpCode.SharpZipLib.Zip.ZipEntry getZipArchiveEntry (ICSharpCode.SharpZipLib.Zip.ZipFile entries, ZipEntry e) {
         foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry zae in entries) {
            if (zae.Name == e.FullName) return zae;
         }
         throw new BMException ("Cannot find '{0}' in archive '{1}'.", e.FullName, e.ArchiveName);
      }


      /// <summary>
      /// Load a .zip file.
      /// This is done by taking the largest file and stream that into memory
      /// </summary>
      private void loadZipFile (String fn, String zipEntryName) {
         CachedArchive ca = ArchiveCache.Instance.Get (fn);
         if (ca != null && !(ca.Archive is ZipArchive)) {
            loadZipFileViaSharpZlib (fn, zipEntryName);
            return;
         }
         FileStream fs = null;
         ZipArchive archive = null;
         try {
            if (ca == null) {
               fs = new FileStream (fn, FileMode.Open, FileAccess.Read, FileShare.Read);
               archive = new ZipArchive (fs, ZipArchiveMode.Read, false, Encoding.UTF8);
               zipEntries = new ZipEntries ();
               foreach (var e in archive.Entries) zipEntries.Add (new ZipEntry (fn, e));

               if (zipEntries.Count > 0) zipEntries.SortAndSelect (fn, null);
               ca = new CachedArchive (fn, archive, zipEntries);
               ArchiveCache.Instance.Add (ca);

               archive = null;
               fs = null;
            }
         } finally {
            archive?.Dispose ();
            fs?.Dispose ();
         }

         try {
            archive = (ZipArchive)ca.Archive;
            zipEntries = ca.Entries;
            zipEntries.Select (fn, zipEntryName);
            if (zipEntries.Count == 0)
               loadEmpty ();
            else {
               zipEntries.Select (fn, zipEntryName);
               var entry = getZipArchiveEntry (archive.Entries, zipEntries.SelectedItem);
               using (var entryStrm = entry.Open ())
               using (var threadedReader = new ThreadedIOBlockReader (entryStrm, true, 64 * 1024))
                  loadStreamIntoMemory (threadedReader, new LoadProgress (this, -1), false);
            }
         } catch (Exception err) {
            logger.Log (err, "Error while reading zip [{0}]. Fallback to SharpZLib.", fn);
            loadZipFileViaSharpZlib (fn, zipEntryName);
         }
      }

      private void loadZipFileViaSharpZlib (String fn, String zipEntryName) {
         CachedArchive ca = ArchiveCache.Instance.Get (fn);
         if (ca != null && !(ca.Archive is ICSharpCode.SharpZipLib.Zip.ZipFile)) ca = null;
         FileStream fs = null;
         ICSharpCode.SharpZipLib.Zip.ZipFile archive = null;
         try {
            if (ca == null) {
               fs = new FileStream (fn, FileMode.Open, FileAccess.Read, FileShare.Read);
               archive = new ICSharpCode.SharpZipLib.Zip.ZipFile (fs, false);
               zipEntries = new ZipEntries ();
               foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry e in archive) zipEntries.Add (new ZipEntry (fn, e));

               if (zipEntries.Count > 0) zipEntries.SortAndSelect (fn, null);
               ca = new CachedArchive (fn, archive, zipEntries);
               ArchiveCache.Instance.Add (ca);

               archive = null;
               fs = null;
            }
         } finally {
            (archive as IDisposable)?.Dispose ();
            fs?.Dispose ();
         }

         archive = (ICSharpCode.SharpZipLib.Zip.ZipFile)ca.Archive;
         zipEntries = ca.Entries;
         zipEntries.Select (fn, zipEntryName);
         if (zipEntries.Count == 0)
            loadEmpty ();
         else {
            zipEntries.Select (fn, zipEntryName);
            var entry = getZipArchiveEntry (archive, zipEntries.SelectedItem);
            using (var entryStrm = archive.GetInputStream (entry))
            using (var threadedReader = new ThreadedIOBlockReader (entryStrm, true, 64 * 1024))
               loadStreamIntoMemory (threadedReader, new LoadProgress (this, -1), false);
         }
      }

      /// <summary>
      /// Load a .7zip file.
      /// We reset the background state to foreground, since, if we are terminated, 7z is still running
      /// </summary>
      private void loadSevenZipFile (String fn, String zipEntryName) {
         Thread cur = Thread.CurrentThread;
         bool isBackground = cur.IsBackground;
         cur.IsBackground = false;  //Make sure that the process is kept alive as long as this thread is alive
         try {
            CachedArchive ca = ArchiveCache.Instance.Get (fn);
            if (ca == null) {
               var entries = SevenZipInputStream.GetEntries (fn);
               zipEntries = new ZipEntries ();
               foreach (var e in entries) zipEntries.Add (new ZipEntry (fn, e));

               if (zipEntries.Count > 0) zipEntries.SortAndSelect (fn, null);
               ca = new CachedArchive (fn, null, zipEntries);
               ArchiveCache.Instance.Add (ca);
            }

            zipEntries = ca.Entries;
            if (zipEntries.Count == 0)
               loadEmpty ();
            else {
               zipEntries.SortAndSelect (fn, zipEntryName);
               using (var entryStrm = new SevenZipInputStream (fn + "::" + zipEntries.SelectedItem.FullName))
               using (var blockRdr = new ThreadedIOBlockReader (entryStrm, true, 4 * 1024, 32))
                  loadStreamIntoMemory (blockRdr, new LoadProgress (this, -1), false);
            }
         } finally {
            cur.IsBackground = isBackground;
         }
      }

      /// <summary>
      /// Load a gz file.
      /// Unzip is preferrable done by starting gzip, and otherwise by using sharpzlib
      /// </summary>
      private void loadGZipFile (String fn) {
         if (!Globals.CanInternalGZip || (DbgStr != null && DbgStr != "intern")) {
            loadGZipFileViaSharpZlib (fn);
         } else {
            Stream strm = null;
            try {
               logger.Log ("Loading '{0}' via internal Zlib.DLL", fn);
               strm = new FileStream (fn, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
               var gz = new GZipDecompressStream (strm, false, 64 * 1024);
               strm = gz;
               using (var rdr = new ThreadedIOBlockReader (strm, false, 64 * 1024))
                  loadStreamIntoMemory (rdr, new LoadProgress (this, -1), false);
            } finally {
               strm?.Dispose ();
            }
         }
      }

      private void loadGZipFileViaSharpZlib (String fn) {
         Stream strm = null;
         try {
            logger.Log ("Loading '{0}' via SharpZipLib.", fn);
            strm = new FileStream (fn, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
            var gz = new ICSharpCode.SharpZipLib.GZip.GZipInputStream (strm, 64 * 1024);
            gz.IsStreamOwner = true;
            strm = gz;
            using (var rdr = new ThreadedIOBlockReader (strm, false, 64 * 1024))
               loadStreamIntoMemory (rdr, new LoadProgress (this, -1), false);
         } finally {
            strm?.Dispose ();
         }
      }

      private void loadEmpty () {
         addSentinelForLastPartial (0);
      }

      private void loadStorage (string fn, string zipEntryName) {
         CachedArchive ca = ArchiveCache.Instance.Get (fn);
         FileStorage stor = null;
         FileEntry fileEntry;
         try {
            if (ca == null) {
               stor = new FileStorage (fn, FileOpenMode.Read);

               zipEntries = new ZipEntries ();
               int i = 0;
               foreach (var e in stor.Entries) {
                  if (++i < 50000) zipEntries.Add (new ZipEntry (fn, e));
               }
               if (zipEntries.Count > 0) zipEntries.SortAndSelect (fn, null);

               ca = new CachedArchive (fn, stor, zipEntries);
               ArchiveCache.Instance.Add (ca);
               stor = null;
            }
         } finally {
            stor?.Dispose ();
         }

         stor = (FileStorage)ca.Archive;
         zipEntries = ca.Entries;

         if (zipEntryName != null) {
            fileEntry = stor.GetFileEntry (zipEntryName);
            if (fileEntry == null) throw new BMException ("Requested entry '{0}' not found in archive '{1}'.", zipEntryName, fn);
            zipEntries.SelectedItemIndex = zipEntries.IndexOf (zipEntryName);
            if (zipEntries.SelectedItemIndex < 0) {
               zipEntries.SelectedItemIndex = zipEntries.Count;
               zipEntries.Add (new ZipEntry (fn, fileEntry));
            }
         }

         using (var entryStrm = stor.GetStream (zipEntries.SelectedItem.FullName))
         using (var blockRdr = new ThreadedIOBlockReader (entryStrm, true, 4 * 1024, 32))
            loadStreamIntoMemory (blockRdr, new LoadProgress (this, -1), false);
      }

      private FileEncoding detectEncodingAndAddFirstLine (IOBlock buffer) {
         var fileEncoding =new FileEncoding (buffer);
         detectedEncoding = fileEncoding;
         encoding = fileEncoding.Current;
         threadCtx.Encoding = fileEncoding.Current;
         AddLine (fileEncoding.PreambleBytes);
         return fileEncoding;
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

            if (detectedEncoding == null) detectEncodingAndAddFirstLine (buffer);
            

            position = addLinesForBuffer (buffer.Position, buffer.Buffer, buffer.Length);
            if (skipMode == SkipMode.None) {
               mem.Write (buffer.Buffer, 0, buffer.Length);
               if (loadedDataOffset < 0) {
                  loadedDataOffset = buffer.Position;
                  ((IDirectStream)mem).SetOffsetOfFirstBuffer (buffer.Position);
                  //long offs = partialLines[0] >> LineFlags.FLAGS_SHIFT;
                  //Globals.StreamLogger.Log ("First line={0}, offset={1}", offs, offs-buffer.Position);
               }
            }

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
         for (int i = 0; i < N; i++) prev = dumpOffset (prev, i);
      }
      private long dumpOffset (long prev, int i) {
         //String data = GetPartialLine (i);
         //int N = Math.Min (32, data.Length);
         //String start = data.Substring (0, N);
         //String end = data.Substring (data.Length-N);
         long x = partialLines[i];
         long offs = x >> LineFlags.FLAGS_SHIFT;
         long mask = x & LineFlags.FLAGS_MASK;
         logger.Log ("-- {0}: o={1} (0x{1:X}), len={2}, flags=0x{3:X}", i, offs, offs - prev, mask);
         return offs;
      }


      private void loadNormalFile (string fn) {
         var directStream = new DirectFileStreamWrapper (fn, 4096);
         var fileStream = directStream.BaseStream;
         var rdr = new ThreadedIOBlockReader (fileStream, true, 64 * 1024, 4);

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

            if (detectedEncoding == null) detectEncodingAndAddFirstLine (buffer);

            position = addLinesForBuffer (buffer.Position, buffer.Buffer, buffer.Length);
            loadProgress.HandleProgress (position);
         }
         addSentinelForLastPartial (position);
      }

      /// <summary>
      /// Finalize loading by adding the sentinel.
      /// Also make sure that there is at least a detected encoding
      /// </summary>
      private void addSentinelForLastPartial (long position) {
         if (detectedEncoding == null) detectedEncoding = new FileEncoding ();
         if (partialLines.Count == 0)
            AddLine (0);
         else {
            long o2 = partialLines[partialLines.Count - 1] >> LineFlags.FLAGS_SHIFT;
            if (o2 != position) {
               if (position == o2+1) { //possible unicode issue
                  if (detectedEncoding.Current.CodePage == FileEncoding.CP_UTF16) {
                     var tmp = new byte[1];
                     var len = DirectStream.Read (o2, tmp, 0, 1);
                     if (len==1 && tmp[0] == 0)
                        return; //skip that last 0x00 byte
                  }
               }
               AddLine (position);
            }
         }
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

      //Diagnostic stuff
      int lineNo = -1;
      private void dbgIncLineNo () {
         ++lineNo;
      }
      static String dbgGetLine (byte[] buf, int offset) {
         int max = buf.Length - offset;
         if (max > 128) max = 128;
         return Encoding.Latin1.GetString (buf, offset, max);
      }

      /// <summary>
      /// Optimized line splitter
      ///
      /// A separator byte[] is used to remove a switch statement
      /// The 1st chance limiters are important for json and xml. We don't use a comma or a dot here, since
      /// that would potentialy result in splitted numbers
      ///
      /// Always returns position+count
      /// </summary>
      private unsafe long addLinesForBuffer (long position, byte[] buf, int count) {
         long skipSize = this.skipSize;
         long prev = partialLines[^1] >> LineFlags.FLAGS_SHIFT;
         int partialPosLimit = maxPartialSize - (int)(position - prev);  //limit from where to split
         int toSkip = this.toSkip;
         int orgToSkip = this.toSkip;
         fixed (byte* p = buf) {
            int i=0;

            //First check if we still need to skip lines...
            if (skipMode == SkipMode.None) goto NORMAL_LINE_PROCESSING;
            if (skipMode == SkipMode.Lines) {
               for (; i < count; i++) {
                  if (p[i] != (byte)10) continue;
                  ++i; //skip the \n
                  if (--toSkip <= 0) break;
               }
               this.toSkip = toSkip;
               this.skippedLines += (orgToSkip - toSkip);
               if (toSkip <=0) goto TERMINATE_SKIPPING;
               partialLines[0] = (position + i) << LineFlags.FLAGS_SHIFT;
               goto EXIT_RTN;
            }

            if (skipMode == SkipMode.Size) {
               for (; i < count; i++) {
                  if (p[i] != (byte)10) continue;
                  ++i; //skip the \n
                  ++this.skippedLines;
                  if (position + i >= skipSize) goto TERMINATE_SKIPPING;
               }
               partialLines[0] = (position + i) << LineFlags.FLAGS_SHIFT;
               goto EXIT_RTN;
            }

         TERMINATE_SKIPPING:
            skipMode = SkipMode.None;
            this.actualMaxLoadSize = initialMaxLoadSize < (long.MaxValue - (position + i)) ? initialMaxLoadSize + (position + i) : long.MaxValue;
            partialLines[0] = (position + i) << LineFlags.FLAGS_SHIFT;
            partialPosLimit = maxPartialSize + i;
            goto NORMAL_LINE_PROCESSING;

            //Normal line-processing
            NORMAL_LINE_PROCESSING:
            while (true) {
               int N = Math.Min (partialPosLimit, count);
               while (i < N) {
                  if (p[i++] != (byte)10) continue;
                  //dbgIncLineNo();
                  AddLine (position + i);
                  partialPosLimit = maxPartialSize + i;  //PartialLimit is the end-position
                  N = Math.Min (partialPosLimit, count);
               }
               if (i >= count) break; //Line too long and spans buffers: exit

               //Its is possible that the linebreak was exactly at the end of the partial line
               if (p[i] == (byte)10) {
                  //dbgIncLineNo ();
                  ++i; //Skip the lf itself
                  AddLine (position + i);
                  partialPosLimit = maxPartialSize + i;  //PartialLimit is the end-position
                  N = Math.Min (partialPosLimit, count);
                  continue;
               }

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
               partialPosLimit = maxPartialSize + end;
               //dbgIncLineNo ();
            }
         }
         EXIT_RTN:
         return position + count;
      }


      /// <summary>
      /// Load a (zip) file
      /// </summary>
      public Task Load (string fn, CancellationToken ct, String zipEntry = null) {
         zipEntries = null;
         largestPartialLine = PartialLineStats.ZERO;
         largestLineIndex= -1;

         return Task.Run (() => {
            DateTime startTime = DateTime.UtcNow;
            Exception err = null;
            partialsEncountered = false;
            this.fileName = Path.GetFullPath (fn);
            this.ct = ct;
            try {
               switch (settings.LoaderSelector.GetLoaderFor(FileName)) {
                  case LoaderSelector.Loader.NativeGZip:
                     loadGZipFile (fileName);
                     break;
                  case LoaderSelector.Loader.SevenZip:
                     loadSevenZipFile (fileName, zipEntry);
                     break;
                  case LoaderSelector.Loader.NativeZip:
                     loadZipFile (fileName, zipEntry);
                     break;
                  case LoaderSelector.Loader.FileStorage:
                     loadStorage (FileName, zipEntry);
                     break;
                  case LoaderSelector.Loader.SharpGZip:
                     loadGZipFileViaSharpZlib (FileName);
                     break;
                  case LoaderSelector.Loader.SharpZip:
                     loadZipFileViaSharpZlib (fileName, zipEntry);
                     break;

                  default:
                     loadNormalFile (fileName);
                     break;
               }
               logger.Log ("-- Loaded. Size={0}, #Lines={1}", Pretty.PrintSize (GetPartialLineOffset (partialLines.Count - 1)), partialLines.Count - 1);
            } catch (Exception ex) {
               err = IOUtils.WrapFilenameInException (ex, fn);
               Logs.ErrorLog.Log (ex, "Exception during load: {0}", err.Message);
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
                  threadCtx?.SetMaxBufferSize (maxBufferSize);
               } catch (Exception e2) {
                  Logs.ErrorLog.Log (e2, "Exception after load: {0}", e2.Message);
                  if (err == null) err = e2;
                  threadCtx?.CloseInstance ();
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
         this.threadCtx?.Close ();
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


      public void CheckNewlines () {
         logger.Log ("Check new lines.");
         threadCtx.DirectStream.PrepareForNewInstance ();
         threadCtx.SetMaxBufferSize (1024 * 1024);
         int detected = 0;
         if (lines != null) {
            logger.Log ("Check new lines via lines (partials encountered).");
            for (int i=0; i < lines.Count-1; i++) {
               if ((i % 1000000) == 0) logger.Log ("Checked {0} lines...", i);
               if (i == 12667)
                  i += 0;
               int len = threadCtx.ReadPartialLineBytesInBuffer (lines[i], lines[i + 1]);
               if (hasNewLine (i, threadCtx.ByteBuffer, len)) ++detected;
            }
         } else {
            logger.Log ("Check new lines via partials (no partials encountered).");
            for (int i = 0; i < partialLines.Count - 1; i++) {
               if ((i % 1000000) == 0) logger.Log ("Checked {0} lines...", i);
               int len = threadCtx.ReadPartialLineBytesInBuffer (i, i+1);
               if (hasNewLine (i, threadCtx.ByteBuffer, len)) ++detected;
            }
         }
         logger.Log ("Total detected newlines: {0}", detected);
      }

      private bool hasNewLine (int line, byte[] buf, int len) {
         int i;
         for (i = 0; i < len - 1; i++) if (buf[i] == (byte)10) break;
         if (i >= len - 1) return false;

         logger.Log ("-- newline detected at line={0} at offset {1}, len={2}", line, i, len);
         return true;
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

      public long GetPartialLineOffset (int index) {
         //logger.Log("PartialLineOffset[{0}]: raw={1:X}, shift={2:X}", line, partialLines[line], (int)(partialLines[line] >> FLAGS_SHIFT));
         return (partialLines[index] >> LineFlags.FLAGS_SHIFT);
      }
      public long GetPartialLineOffsetAndFlags (int line) {
         return partialLines[line];
      }
      public long GetLineOffset (int line) {
         if (lines != null) line = lines[line];
         return partialLines[line] >> LineFlags.FLAGS_SHIFT;
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

      /// <summary>
      /// Get all bytes of a complete line
      /// </summary>
      public byte[] GetLineBytes (int index, out bool truncated) {
         truncated = false;
         if (index < 0) return EMPTY_BYTES;
         if (lines != null) {
            if (index >= lines.Count - 1) return EMPTY_BYTES;
            return threadCtx.GetLineBytes (lines[index], lines[index + 1], settings.MaxLineLength, out truncated);
         }
         if (index >= partialLines.Count - 1) return EMPTY_BYTES;
         return threadCtx.GetLineBytes (index, index + 1, settings.MaxLineLength, out truncated);
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

      /// <summary>
      /// For a given partial line number, return the line that contains the partial (if the partial was the first for a line)
      /// Or return -1 if the partial wasn't the start of a line
      /// </summary>
      public int OptPartialToLineNumber (int partial) {
         if (lines == null) return partial;
         return ((int)partialLines[partial] & LineFlags.CONTINUATION) == 0 ? PartialToLineNumber (partial) : -1;
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

      public PartialLineStats GetLongestLineInBytes () {
         return this.largestPartialLine;
      }
      public PartialLineStats GetLongestLineInChars () {
         if (lines == null) return largestPartialLine;
         PartialLineStats ret= PartialLineStats.ZERO;

         for (int i =0; i< largestPartialLines.Length; i++) {
            var ll = largestPartialLines[i];
            int tmp = threadCtx.GetPartialLineLengthInChars (ll.Index);
            logger.Log ("Longest partial line: at {0}, bytes={1}, chars={2} ({3})", ll.Index, GetPartialLineLengthInBytes (ll.Index), tmp, GetPartialLineLengthInChars (ll.Index));
            if (ret==null || tmp > ret.Length) {
               ret = new PartialLineStats (ll.Index, tmp);
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

   /// <summary>
   /// Constants for the flags and masks in the line offset field (long)
   /// This long is build like [offset][search_mask][flags]
   /// - The flags part is 4 bits
   /// - There are 20 mask bits. Each mask-bit correspond to a search term hit
   /// - The offset part is 64-24=40 bits
   /// </summary>
   public static class LineFlags {
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

}