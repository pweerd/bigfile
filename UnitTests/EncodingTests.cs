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
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitmanager.BigFile.Tests {
   [TestClass]
   public class EncodingTests: TestBase {
      private readonly String text;

      public EncodingTests() {
         var sb = new StringBuilder();
         sb.AppendLine ("Line 1");
         sb.Append ("Long line: ");
         for (int i = 0; i < 100; i++) sb.Append ("0123456789 abcdefghijklmnopqrstuvwxyz ").Append ((char)0x0102).Append ((char)0x0153).Append ((char)0x0154);
         sb.Append ('\n');
         for (int i = 1; i < 100; i++) sb.AppendFormat ("Line {0}\n", i);
         text = sb.ToString ();
      }

      [TestMethod]
      public void TestLength () {
         var enc = new UTF8Encoding (); // Encoding.UTF8;

         byte[] b = enc.GetBytes ("aÂÆ");
         Assert.AreEqual (5, b.Length);

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
      public void CreateEncodingSamples () {
         createTextFile ("utf8.txt", Encoding.UTF8);
         createTextFile ("utf16LE.txt", Encoding.Unicode);
         createTextFile ("utf16BE.txt", Encoding.BigEndianUnicode);
         createTextFile ("utf8-no-bom.txt", new UTF8Encoding (false));
         createTextFile ("utf16LE-no-bom.txt", new UnicodeEncoding(false, false));
         createTextFile ("utf16BE-no-bom.txt", new UnicodeEncoding (true, false));
      }


      private void createTextFile (String fn, Encoding enc) {
         File.AppendAllText (Path.Combine(this.dataDir, fn), text, enc);
      }
   }
}
