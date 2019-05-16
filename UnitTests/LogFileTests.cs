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

namespace Bitmanager.BigFile.Tests
{
   [TestClass]
   public class LogFileTests: TestBase
   {
      private readonly Settings settings;
      public LogFileTests()
      {
         settings = new Settings();
         if (settings.GzipExe == null) throw new Exception("Cannot find Gzip.exe in path or app folders.");
      }

      [TestMethod]
      public void TestLoad()
      {
         _load(dataDir + "logfile.gz");  //gzip
         _load(dataDir + "logfile.txt"); //unpacked
      }

      [TestMethod]
      public void TestZipError()
      {
         String result = "ok";
         try
         {
            _load(dataDir + "test.txt.gz");  //gzip
         }
         catch (Exception e)
         {
            result = e.Message;
         }
         if (result.IndexOf(": not in gzip format") < 0)
            throw new BMException("error should contain ': not in gzip format', but was {0}", result);
      }

      [TestMethod]
      public void TestSearch()
      {
         var cb = new CB();
         var logFile = new LogFile(cb, settings, null);
         logFile.Load(dataDir+"test.txt", CancellationToken.None).Wait();

         Assert.AreEqual(5, search(logFile, "aap"));
         Assert.AreEqual(7, search(logFile, "noot"));
         Assert.AreEqual(8, search(logFile, "mies"));
         Assert.AreEqual(2, search(logFile, "mies AND aap"));
         Assert.AreEqual(11, search(logFile, "mies OR aap"));
         Assert.AreEqual(6, search(logFile, "mies NOT aap"));


         int flagsMask = 0;
         for (int i=0; i<LogFile.MAX_NUM_MASKS; i++)
            flagsMask |= ((int)LineFlags.Mask0) << i;
         Console.WriteLine("Flags={0:X}, const={1:X}", flagsMask, LogFile.FLAGS_MASK);

         List<long> offsets = new List<long>();
         for (int i = 0; i < logFile.PartialLineCount; i++) offsets.Add(logFile.GetPartialLineOffset(i));
         logFile.ResetMatchesAndFlags();
         for (int i = 0; i < logFile.PartialLineCount; i++)
            Assert.AreEqual (offsets[i], logFile.GetPartialLineOffset(i));

         var searchNodes = new SearchNodes();
         dumpOffsets(logFile, "before search");
         dumpSearchNodes(searchNodes, "");
         Assert.AreEqual(5, search(logFile, "aap", searchNodes));
         dumpOffsets(logFile, "after aap");
         dumpSearchNodes(searchNodes, "");
         Assert.AreEqual(7, search(logFile, "noot", searchNodes));
         dumpOffsets(logFile, "after noot");
         dumpSearchNodes(searchNodes, "");
         Assert.AreEqual(8, search(logFile, "mies", searchNodes));
         dumpOffsets(logFile, "after mies");
         dumpSearchNodes(searchNodes, "");
         search(logFile, "mies AND aap", searchNodes);
         dumpOffsets(logFile, "after mies AND aap");
         dumpSearchNodes(searchNodes, "");
         Assert.AreEqual(2, search(logFile, "mies AND aap", searchNodes));
         //Assert.AreEqual(11, search(logFile, "mies OR aap", searchNodes));
         //Assert.AreEqual(6, search(logFile, "mies NOT aap", searchNodes));
         Assert.AreEqual(3, searchNodes.Count);
      }

      private void dumpOffsets(LogFile lf, String why)
      {
         logger.Log();
         logger.Log("Dumping offsets and flags for {0} lines. Reason={1}", lf.PartialLineCount, why);
         for (int i = 0; i < lf.PartialLineCount; i++)
         {
            logger.Log("-- line[{0}]: 0x{1:X}", i, lf.GetPartialLineOffsetAndFlags(i));
         }
      }
      private void dumpSearchNodes(SearchNodes nodes, String why)
      {
         logger.Log();
         logger.Log("Dumping {0} searchNodes. Reason={1}", nodes.Count, why);
         int i = 0;
         foreach (var node in nodes)
         {
            logger.Log("-- node[{0}]: Bit={1}, mask={2:X}, arg={3}", i, node.BitIndex, node.BitMask, node);
            i++;
         }
      }
      private int countMatched (LogFile lf)
      {
         return lf.GetMatchedList(0).Count;
      }

      private int search (LogFile lf, String x, SearchNodes searchNodes=null)
      {
         if (searchNodes==null) searchNodes = new SearchNodes();
         lf.Search(searchNodes.Parse(x), CancellationToken.None).Wait();
         return lf.GetMatchedList(0).Count;
      }


      public void _load (String fn)
      {
         var cb = new CB();

         //Non partial
         var logFile = new LogFile(cb, settings);
         logFile.Load(fn, CancellationToken.None).Wait();
         cb.Result.ThrowIfError();

         Assert.AreEqual(35268, logFile.LineCount);
         Assert.AreEqual(0, logFile.GetLineOffset(0));
         Assert.AreEqual(209008, logFile.GetLineOffset(1));
         Assert.AreEqual(210358, logFile.GetLineOffset(2));

         Assert.AreEqual(186411, logFile.GetLine(0).Length);
         Assert.AreEqual(1349, logFile.GetLine(1).Length);
         Assert.AreEqual(1352, logFile.GetLine(2).Length);

         //Checking longest line
         Assert.AreEqual(0, logFile.LongestPartialIndex);
         Assert.AreEqual(186411, logFile.GetPartialLine(logFile.LongestPartialIndex).Length);


         //Partials
         settings.MaxPartialSize = 1024;
         logFile = new LogFile(cb, settings);
         settings.MaxPartialSize = -1;
         logFile.Load(fn, CancellationToken.None).Wait();
         Assert.AreEqual(35268, logFile.LineCount);
         Assert.AreEqual(0, logFile.GetLineOffset(0));
         Assert.AreEqual(209008, logFile.GetLineOffset(1));
         Assert.AreEqual(210358, logFile.GetLineOffset(2));

         Assert.AreEqual(186411, logFile.GetLine(0).Length);
         Assert.AreEqual(1349, logFile.GetLine(1).Length);
         Assert.AreEqual(1352, logFile.GetLine(2).Length);

         Assert.AreEqual(37681, logFile.PartialLineCount);
         Assert.AreEqual(0, logFile.GetPartialLineOffset(0));
         Assert.AreEqual(1014, logFile.GetPartialLineOffset(1));
         Assert.AreEqual(2038, logFile.GetPartialLineOffset(2));

         Assert.AreEqual(1014, logFile.GetPartialLine(0).Length);
         Assert.AreEqual(1024, logFile.GetPartialLine(1).Length);
         Assert.AreEqual(1013, logFile.GetPartialLine(2).Length);

         //Checking longest line
         Assert.AreEqual(12, logFile.LongestPartialIndex);
         Assert.AreEqual(782, logFile.GetPartialLine(logFile.LongestPartialIndex).Length);
      }
   }

   public class CB : ILogFileCallback
   {
      public Result Result;

      public void OnExportComplete(Result result)
      {
      }

      public void OnLoadComplete(Result result)
      {
         Result = result;
      }

      public void OnLoadCompletePartial(LogFile cloned)
      {
      }

      public void OnProgress(LogFile lf, int percent)
      {
      }

      public void OnSearchComplete(SearchResult result)
      {
      }

      public void OnSearchPartial(LogFile lf, int firstMatch)
      {
      }
   }
}
