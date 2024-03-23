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

using Bitmanager.Core;
using Bitmanager.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   public class TestBase {
      protected readonly string dataDir;
      protected readonly string oldDir;
      protected readonly string newDir;
      protected readonly Logger logger;

      public TestBase () {
         logger = Logs.CreateLogger ("Test", "test");
         string root = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
         root = IOUtils.AddSlash (IOUtils.FindDirectoryToRoot (root, "data", FindToTootFlags.Except));
         dataDir = root;
         oldDir = root + @"old\";
         newDir = root + @"new\";
      }
   }
}
