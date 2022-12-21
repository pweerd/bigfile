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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitmanager.BigFile.Tests {
   [TestClass]
   public class LargestLinesTests: TestBase {

      [TestMethod]
      public void TestEmpty () {
         var x = new LargestLines ();
         var arr = x.ToArray ();
         Assert.AreEqual (1, arr.Length);
         Assert.AreEqual (-1, arr[0].Index);
         Assert.AreEqual (0, arr[0].Length);
      }


      [TestMethod]
      public void TestMulti () {
         var x = new LargestLines ();
         for (int i=0; i<100; i++) x.Add(new PartialLineStats(i, i));
         Assert.AreEqual (68, x.Min.Length);

         var arr = x.ToArray ();
         Assert.AreEqual (32, arr.Length);
         Assert.AreEqual (68, arr[0].Index);
         Assert.AreEqual (68, arr[0].Length);

      }
   }
}
