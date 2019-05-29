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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitmanager.Core;

namespace Bitmanager.BigFile
{
   /// <summary>
   /// Holds a list of ZipEntries and the index of the currently selected item
   /// </summary>
   public class ZipEntries : List<ZipEntry>
   {
      public int SelectedEntry = -1;
   }

   /// <summary>
   /// Standalone version of a ZipArchiveEntry, that holds relevant information, 
   /// without holding a reference to the ZipArchive
   /// </summary>
   public class ZipEntry
   {
      public readonly String Name;
      public readonly String FullName;
      public readonly long Length;
      public readonly String ArchiveName;
      private readonly String _tos;

      public ZipEntry(String archiveName, ZipArchiveEntry e)
      {
         Name = e.Name;
         FullName = e.FullName;
         Length = e.Length;
         ArchiveName = archiveName;
         _tos = Invariant.Format("{0} ({1})", e.FullName, Pretty.PrintSize(e.Length));
      }

      public override String ToString()
      {
         return _tos;
      }
   }
}
