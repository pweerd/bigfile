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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitmanager.Core;
using Bitmanager.IO;
using Bitmanager.Storage;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Holds a list of ZipEntries and the index of the currently selected item
   /// ZipEntries that have a FullName ending with / or \ (directories) are skipped
   /// </summary>
   public class ZipEntries : List<ZipEntry> {
      public int SelectedItemIndex = -1;

      public new void Add(ZipEntry e) {
         if (e.FullName.EndsWith ('/') || e.FullName.EndsWith ('\\')) return; //Directory
         base.Add(e);
      }

      public void SortAndSelect (string archiveName, string entryName) {
         Sort (ZipEntry.SortSize);
         Select (archiveName, entryName);
      }

      public void Select (string archiveName, string entryName) {
         long max = -1;
         int ixMax = -1;
         if (string.IsNullOrEmpty (entryName)) {
            for (int i = 0; i < Count; i++) {
               if (this[i].Length <= max) continue;
               max = this[i].Length;
               ixMax = i;
            }
            SelectedItemIndex = ixMax;
         } else {
            SelectedItemIndex = FindIndex (x => x.FullName == entryName);
            if (SelectedItemIndex < 0) throw new BMException ("Requested entry '{0}' not found in archive '{1}'.", entryName, archiveName);
         }
      }

      public int IndexOf (string entryName) {
         return FindIndex (x => x.FullName == entryName);
      }

      public ZipEntry SelectedItem {
         get {
            return SelectedItemIndex < 0 ? null : this[SelectedItemIndex];
         }
      }
   }

   /// <summary>
   /// Standalone version of a ZipArchiveEntry, that holds relevant information, 
   /// without holding a reference to the ZipArchive
   /// </summary>
   public class ZipEntry {
      public readonly string Name;
      public readonly string FullName;
      public readonly long Length;
      public readonly string ArchiveName;
      private readonly string _tos;

      public ZipEntry (string archiveName, ZipArchiveEntry e) {
         Name = e.Name;
         FullName = e.FullName;
         Length = e.Length;
         ArchiveName = archiveName;
         _tos = Invariant.Format ("{0} ({1})", e.FullName, Pretty.PrintSize (e.Length));
      }

      public ZipEntry (string archiveName, SevenZipEntry e) {
         Name = Path.GetFileName(e.Name);
         FullName = e.Name;
         Length = e.Size;
         ArchiveName = archiveName;
         _tos = Invariant.Format ("{0} ({1})", e.Name, Pretty.PrintSize (e.Size));
      }

      public ZipEntry (string archiveName, FileEntry e) {
         Name = e.Name;
         FullName = e.Name;
         Length = e.Size;
         ArchiveName = archiveName;
         _tos = Invariant.Format ("{0} ({1})", e.Name, Pretty.PrintSize (e.Size));
      }

      public ZipEntry (string archiveName, string txt) {
         ArchiveName = archiveName;
         Name = txt;
         FullName = txt;
         Length = 0;
         _tos = txt;
      }

      public ZipEntry (string archiveName, ICSharpCode.SharpZipLib.Zip.ZipEntry e) {
         Name = e.Name;
         FullName = e.Name;
         Length = e.Size;
         ArchiveName = archiveName;
         _tos = Invariant.Format ("{0} ({1})", e.Name, Pretty.PrintSize (e.Size));
      }

      public static int SortName (ZipEntry x, ZipEntry y) {
         return string.Compare (x._tos, y._tos, StringComparison.OrdinalIgnoreCase);
      }
      public static int SortSize (ZipEntry x, ZipEntry y) {
         if (x.Length > y.Length) return -1;
         if (x.Length < y.Length) return 1;
         return string.CompareOrdinal (x.Name, y.Name);
      }

      public override string ToString () {
         return _tos;
      }
   }
}
