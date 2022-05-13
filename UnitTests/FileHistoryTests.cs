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

namespace Bitmanager.BigFile.Tests {

   [TestClass]

   public class FileHistoryTests : TestBaseSimple { 

      [TestMethod]
      public void TestInsert () {
         var fh = new FileHistory ("xx");
         Assert.AreEqual (null, fh.Top);

         fh.Add ("test1.txt");
         fh.Add ("test2.txt");
         fh.Add ("test3.txt");
         Assert.AreEqual ("test3.txt", fh.Items[0]);
         dump(fh, "1");

         fh.Add ("test1.txt");
         dump (fh, "2");
         Assert.AreEqual ("test1.txt", fh.Items[0]);
         Assert.AreEqual ("test3.txt", fh.Items[1]);
         Assert.AreEqual ("test2.txt", fh.Items[2]);
         Assert.AreEqual (null, fh.Items[3]);

         fh.Add (null);
         Assert.AreEqual ("test1.txt", fh.Items[0]);
         Assert.AreEqual ("test1.txt", fh.Top);
      }

      private void dump(FileHistory fh, String why) {
         Console.WriteLine ("Dumping items ({0})", why);
         foreach (var x in fh.Items) Console.WriteLine (x ?? "null");
      }
   }
}