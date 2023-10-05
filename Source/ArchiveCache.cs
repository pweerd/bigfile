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

using Bitmanager.Cache;

namespace Bitmanager.BigFile {

   /// <summary>
   /// Cache for caching 1 archive.
   /// Reason for choosing 1 archive is that we want to keep at most 1 archive open.
   /// If we keep more archives open we need an option to clear the cache from the UI!
   /// </summary>
   public class ArchiveCache : LRUCache {
      public static readonly ArchiveCache Instance = new ArchiveCache ();
      public ArchiveCache () : base (1, LRUCache.DisposeEntry) {
      }

      public void Add (CachedArchive value) {
         base.Add (value.FileName, value);
      }

      public new CachedArchive Get (string key) {
         return (CachedArchive)base.Get (key);
      }
   }

   /// <summary>
   /// An entry in the ArchiveCache
   /// </summary>
   public class CachedArchive: IDisposable {
      public readonly string FileName;
      public readonly object Archive;
      public readonly ZipEntries Entries;

      public CachedArchive (string fn, object archive, ZipEntries entries) {
         FileName = fn;
         Archive = archive;
         Entries = entries;
      }

      public bool IsSame (CachedArchive other) {
         return this==other || this.FileName == other.FileName;
      }

      public void Dispose () {
         (Archive as IDisposable)?.Dispose ();
      }
   }
}
