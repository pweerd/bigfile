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
using System.IO;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using Bitmanager.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitmanager.BigFile.Tests {
   [TestClass]
   public class EncodingTests: TestBase {
      private readonly SettingsSource settingsSource;
      private readonly string text;

      public EncodingTests() {
         settingsSource = new SettingsSource ();

         var sb = new StringBuilder();
         sb.AppendLine ("Line 1");
         sb.Append ("Long line: ");
         for (int i = 0; i < 100; i++) sb.Append ("0123456789 abcdefghijklmnopqrstuvwxyz ").Append ((char)0x0102).Append ((char)0x0153).Append ((char)0x0154);
         sb.Append ('\n');
         for (int i = 1; i < 100; i++) sb.AppendFormat (Invariant.Culture, "Line {0}\n", i);
         text = sb.ToString ();
      }

      [TestMethod]
      public void TestLength () {
         var enc = new UTF8Encoding (); // Encoding.UTF8;

         byte[] b = enc.GetBytes ("aÃƒâ€šÃƒâ€ ");
         Assert.AreEqual (23, b.Length);

         byte[] b2 = new byte[4];
         Array.Copy (b, b2, 4);
         Assert.AreEqual (3, enc.GetCharCount (b, 0, 5));
         //Assert.AreEqual("", enc.GetString(b2));
         Assert.AreEqual (3, enc.GetCharCount (b2, 0, 4));
         Assert.AreEqual (2, enc.GetCharCount (b, 0, 3));
         Assert.AreEqual (2, enc.GetCharCount (b, 0, 2));
         Assert.AreEqual (1, enc.GetCharCount (b, 0, 1));
      }


      [TestMethod]
      public void TestDifferentEncodings () {
         createTextFile ("utf8.txt", Encoding.UTF8);
         createTextFile ("utf16LE.txt", Encoding.Unicode);
         createTextFile ("utf16BE.txt", Encoding.BigEndianUnicode);
         createTextFile ("utf8-no-bom.txt", new UTF8Encoding (false));
         createTextFile ("utf16LE-no-bom.txt", new UnicodeEncoding (false, false));
         createTextFile ("utf16BE-no-bom.txt", new UnicodeEncoding (true, false));

         var lfUtf8 = loadLogFile ("utf8.txt", FileEncoding.CP_UTF8, 3);
         var lfUtf8_nb = loadLogFile ("utf8-no-bom.txt", FileEncoding.CP_UTF8, 0);
         var lfUtf16LE = loadLogFile ("utf16LE.txt", FileEncoding.CP_UTF16, 2);
         var lfUtf16BE = loadLogFile ("utf16BE.txt", FileEncoding.CP_UTF16BE, 2);
         var lfUtf16LE_nb = loadLogFile ("utf16LE-no-bom.txt", FileEncoding.CP_UTF16, 0);
         var lfUtf16BE_nb = loadLogFile ("utf16BE-no-bom.txt", FileEncoding.CP_UTF16BE, 0);

         var line0 = lfUtf16LE.GetLine (0);

         int N = lfUtf8.LineCount;
         for (int i=0; i<N; i++) {
            var s = lfUtf8.GetLine (i);
            checkEqualLine (s, lfUtf8_nb, i);
            checkEqualLine (s, lfUtf16LE, i);
            checkEqualLine (s, lfUtf16BE, i);
            checkEqualLine (s, lfUtf16LE_nb, i);
            checkEqualLine (s, lfUtf16BE_nb, i);
         }
      }

      private void checkEqualLine (string exp, LogFile lf, int idx) {
         var x = lf.GetLine (idx);
         if (x != exp) {
            var msg = Invariant.Format ("Difference with {0} at line {1}", Path.GetFileName (lf.FileName), idx);
            logger.Log (msg);
            logger.Log ("-- Exp: [{0}]", exp);
            logger.Log ("-- Act: [{0}]", x);
            throw new Exception (msg);
         }
      }

      private void dumpStats (LogFile lf) {
         var stats = lf.LongestPartialLine;
         var enc = lf.DetectedEncoding.Current;
         logger.Log ("Dumping primary stats for {0}:", lf.FileName);
         logger.Log ("-- Encoding={0}, preamble={1}", enc.EncodingName, lf.DetectedEncoding.PreambleBytes);
         logger.Log ("-- LineCount={0}", lf.LineCount);
         logger.Log ("-- PartialLineCount={0}", lf.PartialLineCount);
         logger.Log ("-- LongestLineIndex={0}", lf.LongestLineIndex);
         logger.Log ("-- Starts: index={0}, length={1}", stats.Index, stats.Length);
      }

      private LogFile loadLogFile (string fn, int expCodePage, int expPreamble) {
         var cb = new CB ();

         var lf = new LogFile (cb, settingsSource.ActualizeDefaults (), null, 257);
         lf.Load (Path.Combine (dataDir, fn), CancellationToken.None).Wait ();
         cb.Result.ThrowIfError ();

         dumpStats (lf);
         Assert.AreEqual (expCodePage, lf.DetectedEncoding.Current.CodePage);
         Assert.AreEqual (expPreamble, lf.DetectedEncoding.PreambleBytes);
         Assert.AreEqual (101, lf.LineCount);
         Assert.AreEqual (1, lf.LongestLineIndex);
         return lf;
      }


      private void createTextFile (string fn, Encoding enc) {
         File.WriteAllText (Path.Combine(this.dataDir, fn), text, enc);
      }
   }
}
