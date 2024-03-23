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
using System.Threading.Tasks;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Handles the file history for the open-menu
   /// </summary>
   public class FileHistory {
      private readonly string[] history;
      private readonly string regPrefix;

      public string[] Items {
         get {
            lock (history) {
               return (string[])history.Clone ();
            }
         }
      }
      public string Top {
         get {
            lock (history) {
               return history[0];
            }
         }
      }


      public FileHistory (string prefix) {
         this.regPrefix = prefix;
         history = SettingsSource.LoadFileHistory (prefix);

         //dedup entries (caused by a previous bug
         int j = 0;
         for (int i = 0; i < history.Length; i++) {
            string tmp = history[i];
            if (tmp == null) continue;
            int k;
            for (k = j - 1; k >= 0; k--) {
               if (string.Equals (tmp, history[k], StringComparison.OrdinalIgnoreCase))
                  break;
            }
            if (k >= 0) continue;
            history[j++] = tmp;
         }
         for (; j < history.Length; j++) history[j] = null;
      }

      public void Save () {
         lock (history) {
            SettingsSource.SaveFileHistory (history, regPrefix);
         }
      }

      public void Add (string fn) {
         if (string.IsNullOrEmpty (fn)) return;
         lock (history) {
            int j;
            for (j = 0; j < history.Length-1; j++) {
               if (!string.Equals (history[j], fn, StringComparison.OrdinalIgnoreCase)) continue;
               fn = history[j]; //reuse oldest string to prevent garbage collected
               break;
            }
            if (j > 0) {
               for (int i = j; i > 0; i--) {
                  history[i] = history[i - 1];
               }
               history[0] = fn;
            }
         }
      }

      public void RemoveListed (List<string> list) {
         lock (history) {
            int j = 0;
            for (int i = 0; i < history.Length; i++) {
               string fn = history[i];
               if (fn != null && list.Contains (fn)) continue;
               history[j++] = fn;
            }
            for (; j < history.Length; j++) {
               history[j] = null;
            }
         }
      }
      public void RemoveInvalid (Func<string, bool> checker) {
         string[] h = Items;
         var toRemove = new List<string> ();
         foreach (var fn in h) {
            if (fn != null && !checker (fn)) toRemove.Add (fn);
         }
         if (toRemove.Count > 0) RemoveListed (toRemove);
      }
   }
}
