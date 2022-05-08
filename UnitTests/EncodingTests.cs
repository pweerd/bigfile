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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitmanager.BigFile.Tests {
   [TestClass]
   public class EncodingTests {
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
   }
}
