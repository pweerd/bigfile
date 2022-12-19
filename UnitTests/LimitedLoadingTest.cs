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

using Bitmanager.Core;
using Bitmanager.IO;
using Bitmanager.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Bitmanager.BigFile.Tests {
   [TestClass]
   public class LimitedLoadingTest : TestBaseSimple { //FileRepo
      const String FN = "z:\\test.txt";
      const String FNGZ = "z:\\test.txt.gz";

      public LimitedLoadingTest () {
         generateTestFiles ();
      }

      [TestMethod]
      public void TestLimitedLines () {
         var settings = new SettingsSource ();
         settings.CompressMemoryIfBigger.Set ("1");
         testSkipLines (settings, FN);
         testSkipLines (settings, FNGZ);
         settings.LoadMemoryIfBigger.Set ("1");
         testSkipLines (settings, FN);
         testSkipLines (settings, FNGZ);
      }


      [TestMethod]
      public void TestLimitedOffsets () {
         var settings = new SettingsSource ();
         settings.CompressMemoryIfBigger.Set ("1");
         testSkipOffset (settings, FN);
         testSkipOffset (settings, FNGZ);
         settings.LoadMemoryIfBigger.Set ("1");
         testSkipOffset (settings, FN);
         testSkipOffset (settings, FNGZ);
      }
      private void testSkipLines (SettingsSource settings, String fn) {
         var sb = new StringBuilder ();
         for (int i = 0; i < 20; i++) {
            var skip = i * 501;
            var lf = load (settings, fn, 0, skip);

            String partial = lf.GetPartialLine (0);
            String line = lf.GetLine (0);

            String pfxExp = generatePrefix (sb, skip, lf.GetPartialLineOffset (0)).ToString ();
            String pfxAct = partial.Substring (0, pfxExp.Length);
            if (pfxAct != pfxExp) throw new BMException ("Skip {0} in {1} failed(partial): exp={2}, act={3}", skip, lf.FileName, pfxExp, pfxAct);
            pfxAct = line.Substring (0, pfxExp.Length);
            if (pfxAct != pfxExp) throw new BMException ("Skip {0} in {1} failed(line): exp={2}, act={3}", skip, lf.FileName, pfxExp, pfxAct);
         }
      }
      private void testSkipOffset (SettingsSource settings, String fn) {
         var sb = new StringBuilder ();
         int[] lineNos = { 0, 16, 31, 46, 61, 76, 91, 106, 122, 137, 152, 167, 182, 197, 213, 229, 244, 259, 274, 289 };
         for (int i = 0; i < lineNos.Length; i++) {
            var skip = i * 77001L;
            var lf = load (settings, fn, 0, -skip);

            String partial = lf.GetPartialLine (0);
            String line = lf.GetLine (0);

            String pfxExp = generatePrefix (sb, lineNos[i], lf.GetPartialLineOffset (0)).ToString ();
            String pfxAct = partial.Substring (0, pfxExp.Length);
            logger.Log ("skip[{0}]: {1}, act={2}, exp={3} skipped={4}, {5}", i, skip, pfxAct, pfxExp, lf.SkippedLines, lf.SkippedSize);
            //continue;
            if (pfxAct != pfxExp) throw new BMException ("Skip {0} in {1} failed(partial): exp={2}, act={3}", skip, lf.FileName, pfxExp, pfxAct);
            pfxAct = line.Substring (0, pfxExp.Length);
            if (pfxAct != pfxExp) throw new BMException ("Skip {0} in {1} failed(line): exp={2}, act={3}", skip, lf.FileName, pfxExp, pfxAct);
         }
      }

      private LogFile load (SettingsSource settings, String fn, long maxLoad, long toSkip) {
         var cb = new CB ();
         LogFile lf = new LogFile (cb, settings.ActualizeDefaults (), Encoding.UTF8, 2048, maxLoad, toSkip);
         lf.Load (fn, CancellationToken.None, null).Wait ();
         cb.Result.ThrowIfError ();
         return lf;
      }

      private StringBuilder generatePrefix (StringBuilder sb, int line, long offset) {
         sb.Clear ();
         return sb.AppendFormat ("{0};{1};", line, offset);
      }

      private void generateTestFiles () {
         var str = "0123456789 ";
         var mem = new MemoryStream ();
         var wtr = mem.CreateTextWriter ();
         var sb = new StringBuilder ();

         int[] len = { 751, 501, 123 };
         for (int i = 0; i < 1000; i++) {
            wtr.Flush ();
            generatePrefix (sb, i, mem.Length);
            for (int j = len[i % 3]; j >= 0; j--) {
               sb.Append (str);
            }
            sb.Append ('\n');
            wtr.Write (sb);
         }
         wtr.Flush ();

         using (var fs = IOUtils.CreateOutputStream (FN)) {
            mem.Position = 0;
            mem.WriteTo (fs);
         }
         using (var fs = IOUtils.CreateOutputStream (FNGZ)) {
            using (var gz = new GZipCompressStream (fs, false, ZLibCompressionLevel.Compress_8, 4096)) {
               mem.Position = 0;
               mem.WriteTo (gz);
            }
         }
      }
   }
}
