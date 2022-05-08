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
using System.Collections.Generic;
using System.Threading;
using Bitmanager.IO;
using Bitmanager.BigFile;
using Bitmanager.BigFile.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bitmanager.Core;

namespace Bitmanager.BigFile {
   [TestClass]
   public class LogFileTests : TestBase {
      private readonly SettingsSource settingsSource;
      public LogFileTests () {
         settingsSource = new SettingsSource ();
      }

      [TestMethod]
      public void TestLoad () {
         _load (dataDir + "allcountries.txt.gz");  //gzip
         _load (dataDir + "allcountries.txt"); //unpacked
      }

      [TestMethod]
      public void TestZipError () {
         String result = "ok";
         try {
            _load (dataDir + "test.txt.gz");  //gzip
         } catch (Exception e) {
            result = e.Message;
         }
         if (result.IndexOf ("Error GZIP header") < 0)
            throw new BMException ("error should contain ': not in gzip format', but was {0}", result);
      }

      [TestMethod]
      public void TestSearch () {
         var cb = new CB ();
         var logFile = new LogFile (cb, settingsSource.ActualizeDefaults (), null, -1);
         logFile.Load (dataDir + "test.txt", CancellationToken.None).Wait ();

         Assert.AreEqual (5, search (logFile, "aap"));
         Assert.AreEqual (7, search (logFile, "noot"));
         Assert.AreEqual (8, search (logFile, "mies"));
         Assert.AreEqual (2, search (logFile, "mies AND aap"));
         Assert.AreEqual (11, search (logFile, "mies OR aap"));
         Assert.AreEqual (6, search (logFile, "mies NOT aap"));


         int flagsMask = 0;
         for (int i = 0; i < LogFile.MAX_NUM_MASKS; i++)
            flagsMask |= ((int)LineFlags.Mask0) << i;
         Console.WriteLine ("Flags={0:X}, const={1:X}", flagsMask, LogFile.FLAGS_MASK);

         List<long> offsets = new List<long> ();
         for (int i = 0; i < logFile.PartialLineCount; i++) offsets.Add (logFile.GetPartialLineOffset (i));
         logFile.ResetMatchesAndFlags ();
         for (int i = 0; i < logFile.PartialLineCount; i++)
            Assert.AreEqual (offsets[i], logFile.GetPartialLineOffset (i));

         var searchNodes = new SearchNodes ();
         dumpOffsets (logFile, "before search");
         dumpSearchNodes (searchNodes, "");
         Assert.AreEqual (5, search (logFile, "aap", searchNodes));
         dumpOffsets (logFile, "after aap");
         dumpSearchNodes (searchNodes, "");
         Assert.AreEqual (7, search (logFile, "noot", searchNodes));
         dumpOffsets (logFile, "after noot");
         dumpSearchNodes (searchNodes, "");
         Assert.AreEqual (8, search (logFile, "mies", searchNodes));
         dumpOffsets (logFile, "after mies");
         dumpSearchNodes (searchNodes, "");
         search (logFile, "mies AND aap", searchNodes);
         dumpOffsets (logFile, "after mies AND aap");
         dumpSearchNodes (searchNodes, "");
         Assert.AreEqual (2, search (logFile, "mies AND aap", searchNodes));
         //Assert.AreEqual(11, search(logFile, "mies OR aap", searchNodes));
         //Assert.AreEqual(6, search(logFile, "mies NOT aap", searchNodes));
         Assert.AreEqual (3, searchNodes.Count);
      }

      [TestMethod]
      public void TestCompressErrors () {
         var settingsSource = new SettingsSource ();
         settingsSource.CompressMemoryIfBigger.Set ("0");
         settingsSource.LoadMemoryIfBigger.Set ("0");
         var cb = new CB ();
         var logFile = new LogFile (cb, settingsSource.ActualizeDefaults (), null, -1);
         logFile.Load (dataDir + "compress-errors.txt", CancellationToken.None).Wait ();

         var mem = checkAndCast<CompressedChunkedMemoryStream> (logFile.DirectStream);
         Assert.AreEqual (false, mem.IsCompressionEnabled);
         logFile.Dispose ();
      }



      [TestMethod]
      public void TestSelectedLines () {
         var cb = new CB ();
         var settingsSource = new SettingsSource ();
         var fn = this.dataDir + "allcountries.txt";

         var list = new List<int> ();
         for (int i = 0; i < 18; i++) {
            if (i == 0 || i % 2 == 1) list.Add (i);
         }

         LogFile logFile;

         //Full lines
         logFile = new LogFile (cb, settingsSource.ActualizeDefaults (), null, -1);
         logFile.Load (fn, CancellationToken.None).Wait ();

         Assert.AreEqual (18, logFile.LineCount);
         Assert.AreEqual (18, logFile.PartialLineCount);
         var lineList = logFile.ConvertToLines (list);
         Assert.AreSame (list, lineList);

         Assert.AreEqual (0, lineList[0]);
         Assert.AreEqual (1, lineList[1]);
         Assert.AreEqual (3, lineList[2]);
         Assert.AreEqual (5, lineList[3]);
         Assert.AreEqual (7, lineList[4]);
         Assert.AreEqual (9, lineList[5]);
         Assert.AreEqual (11, lineList[6]);
         Assert.AreEqual (13, lineList[7]);
         Assert.AreEqual (15, lineList[8]);
         Assert.AreEqual (17, lineList[9]);
         Assert.AreEqual (10, lineList.Count);
         logFile.Dispose ();



         //Partial lines
         logFile = new LogFile (cb, settingsSource.ActualizeDefaults (), null, 1024);
         logFile.Load (fn, CancellationToken.None).Wait ();

         Assert.AreEqual (18, logFile.LineCount);
         Assert.AreEqual (90, logFile.PartialLineCount);
         list.Add (89);
         lineList = logFile.ConvertToLines (list);

         Assert.AreEqual (4, lineList.Count);
         Assert.AreEqual (0, lineList[0]);
         Assert.AreEqual (1, lineList[1]);
         Assert.AreEqual (2, lineList[2]);
         Assert.AreEqual (17, lineList[3]);
         logFile.Dispose ();
      }

      private T checkAndCast<T> (IDirectStream strm) where T : IDirectStream {
         Assert.IsNotNull (strm);
         Assert.IsInstanceOfType (strm, typeof (T));
         return (T)strm;
      }



      private void dumpOffsets (LogFile lf, String why) {
         logger.Log ();
         logger.Log ("Dumping offsets and flags for {0} lines. Reason={1}", lf.PartialLineCount, why);
         for (int i = 0; i < lf.PartialLineCount; i++) {
            logger.Log ("-- line[{0}]: 0x{1:X}", i, lf.GetPartialLineOffsetAndFlags (i));
         }
      }
      private void dumpSearchNodes (SearchNodes nodes, String why) {
         logger.Log ();
         logger.Log ("Dumping {0} searchNodes. Reason={1}", nodes.Count, why);
         int i = 0;
         foreach (var node in nodes) {
            logger.Log ("-- node[{0}]: Bit={1}, mask={2:X}, arg={3}", i, node.BitIndex, node.BitMask, node);
            i++;
         }
      }
      private int countMatched (LogFile lf) {
         return lf.GetMatchedList (0).Count;
      }

      private int search (LogFile lf, String x, SearchNodes searchNodes = null) {
         if (searchNodes == null) searchNodes = new SearchNodes ();
         lf.Search (searchNodes.Parse (x), CancellationToken.None).Wait ();
         return lf.GetMatchedList (0).Count;
      }


      public void _load (String fn) {
         var cb = new CB ();

         //Non partial
         var logFile = new LogFile (cb, settingsSource.ActualizeDefaults (), null, -1);
         logFile.Load (fn, CancellationToken.None).Wait ();
         cb.Result.ThrowIfError ();

         Assert.AreEqual (18, logFile.LineCount);
         Assert.AreEqual (0, logFile.GetLineOffset (0));
         Assert.AreEqual (6914, logFile.GetLineOffset (1));
         Assert.AreEqual (13114, logFile.GetLineOffset (2));

         Assert.AreEqual (6007, logFile.GetLine (0).Length);
         Assert.AreEqual (5688, logFile.GetLine (1).Length);
         Assert.AreEqual (4757, logFile.GetLine (2).Length);

         //Checking longest line
         Assert.AreEqual (5, logFile.LongestPartialIndex);
         Assert.AreEqual (7464, logFile.GetPartialLine (logFile.LongestPartialIndex).Length);
         Assert.AreEqual (5, logFile.LongestLineIndex);
         Assert.AreEqual (7464, logFile.GetLine (logFile.LongestLineIndex).Length);

         //Partials
         logFile = new LogFile (cb, settingsSource.ActualizeDefaults (), null, 1024);
         logFile.Load (fn, CancellationToken.None).Wait ();
         Assert.AreEqual (18, logFile.LineCount);
         Assert.AreEqual (0, logFile.GetLineOffset (0));
         Assert.AreEqual (6914, logFile.GetLineOffset (1));
         Assert.AreEqual (13114, logFile.GetLineOffset (2));

         Assert.AreEqual (6007, logFile.GetLine (0).Length);
         Assert.AreEqual (5688, logFile.GetLine (1).Length);
         Assert.AreEqual (4757, logFile.GetLine (2).Length);

         Assert.AreEqual (90, logFile.PartialLineCount);
         Assert.AreEqual (0, logFile.GetPartialLineOffset (0));
         Assert.AreEqual (1018, logFile.GetPartialLineOffset (1));
         Assert.AreEqual (2042, logFile.GetPartialLineOffset (2));

         Assert.AreEqual (940, logFile.GetPartialLine (0).Length);
         Assert.AreEqual (965, logFile.GetPartialLine (1).Length);
         Assert.AreEqual (882, logFile.GetPartialLine (2).Length);

         //Checking longest partial line
         Assert.AreEqual (56, logFile.LongestPartialIndex);
         Assert.AreEqual (859, logFile.GetPartialLine (logFile.LongestPartialIndex).Length);

         //Checking longest  line
         Assert.AreEqual (5, logFile.LongestLineIndex);
         //Assert.AreEqual(7464, logFile.GetLine(logFile.LongestLineIndex).Length);
         Assert.AreEqual (7464, logFile.GetLine (5).Length);

         testNextLine (logFile);
         testPrevLine (logFile);

         //Match all lines
         Assert.AreEqual (logFile.PartialLineCount, search (logFile, "r:."));
         testNextPartial (logFile);
         testPrevPartial (logFile);

         //Test the next/prev by using a line filter
         var matches = new List<int> ();
         matches.Add (logFile.PartialFromLineNumber (3));
         matches.Add (logFile.PartialFromLineNumber (5));
         logger.Log ("Matching lines: {0} for line 3 and {1} for line 5", matches[0], matches[1]);
         //dumpOffsets(logFile, "test");
         Assert.AreEqual (3, logFile.NextLineNumber (-123, matches));
         Assert.AreEqual (5, logFile.NextLineNumber (3, matches));
         Assert.AreEqual (18, logFile.NextLineNumber (5, matches));

         Assert.AreEqual (5, logFile.PrevLineNumber (99999, matches));
         Assert.AreEqual (3, logFile.PrevLineNumber (5, matches));
         Assert.AreEqual (-1, logFile.PrevLineNumber (3, matches));
      }

      private void testNextLine (LogFile lf) {
         Assert.AreEqual (0, lf.NextLineNumber (-123, null));
         int line = -1;
         while (true) {
            int next = lf.NextLineNumber (line, null);
            logger.Log ("Line={0}, next={1}", line, next);
            if (next >= lf.LineCount) break;

            Assert.AreEqual (line + 1, next);
            line = next;
         }
      }
      private void testPrevLine (LogFile lf) {
         Assert.AreEqual (lf.LineCount - 1, lf.PrevLineNumber (int.MaxValue, null));
         int line = lf.LineCount;
         while (true) {
            int prev = lf.PrevLineNumber (line, null);
            logger.Log ("Line={0}, prev={1}", line, prev);
            if (prev < 0) break;

            Assert.AreEqual (line - 1, prev);
            line = prev;
         }
      }

      private void testNextPartial (LogFile lf) {
         Assert.AreEqual (0, lf.NextPartialHit (-123));
         int line = -1;
         while (true) {
            int next = lf.NextLineNumber (line, null);
            logger.Log ("Line={0}, next={1}", line, next);
            if (next >= lf.LineCount) break;

            Assert.AreEqual (line + 1, next);
            line = next;
         }
      }
      private void testPrevPartial (LogFile lf) {
         Assert.AreEqual (lf.PartialLineCount - 1, lf.PrevPartialHit (int.MaxValue));
         int line = lf.LineCount;
         while (true) {
            int prev = lf.PrevLineNumber (line, null);
            logger.Log ("Line={0}, prev={1}", line, prev);
            if (prev < 0) break;

            Assert.AreEqual (line - 1, prev);
            line = prev;
         }
      }
   }

   public class CB : ILogFileCallback {
      public Result Result;

      public void OnExportComplete (ExportResult result) {
      }

      public void OnLoadComplete (Result result) {
         Result = result;
      }

      public void OnLoadCompletePartial (LogFile cloned) {
      }

      public void OnProgress (LogFile lf, int percent) {
      }

      public void OnSearchComplete (SearchResult result) {
      }

      public void OnSearchPartial (LogFile lf, int firstMatch) {
      }
   }
}
