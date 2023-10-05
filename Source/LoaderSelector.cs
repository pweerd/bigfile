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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bitmanager.Core;
using Bitmanager.IO;
using Bitmanager.Storage;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Helper class to determine how a file (archive) should be loaded
   /// </summary>
   public class LoaderSelector {
      public enum Loader { Other, NativeGZip, SharpGZip, NativeZip, SharpZip, SevenZip, FileStorage};
      private readonly string[] sevenZipExts;

      public LoaderSelector(string extBySevenZip) {
         var list = new List<string>();
         foreach (var s in extBySevenZip.ToLowerInvariant ().SplitStandard ()) {
            if (string.IsNullOrEmpty (s)) continue;
            list.Add (s[0] == '.' ? s : "." + s);
         }
         list.Sort ((x, y) => y.Length - x.Length);
         sevenZipExts = list.ToArray ();
      }

      public Loader GetLoaderFor (string fn) {
         fn = fn.ToLowerInvariant ();
         for (int i=0; i<sevenZipExts.Length; i++) {
            if (!fn.EndsWith (sevenZipExts[i])) continue;
            if (SevenZipInputStream.FindSevenZip (false) != null) return Loader.SevenZip;
            break;
         }
         if (fn.EndsWith (".gz")) return Loader.NativeGZip;
         if (fn.EndsWith (".zip")) return Loader.NativeZip;
         if (FileStorage.IsPossibleAndExistingStorageFile (fn)) return Loader.FileStorage;

         return Loader.Other;
      }
   }
}
