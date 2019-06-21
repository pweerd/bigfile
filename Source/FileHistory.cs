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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile
{
   /// <summary>
   /// Handles the file history for the open-menu
   /// </summary>
   public class FileHistory
   {
      private readonly String[] history;
      private readonly String regPrefix;

      public string[] Items { get { return history; } }

      public FileHistory (String prefix="fh_")
      {
         this.regPrefix = prefix;
         history = SettingsSource.LoadFileHistory(prefix);
      }

      public void Save()
      {
         SettingsSource.SaveFileHistory(history, regPrefix);
      }

      public void Add (String fn)
      {
         int j;
         for (j=0; j<history.Length; j++)
         {
            if (history[j] == null) goto SHIFT_DOWN;
            if (String.Equals(history[j], fn, StringComparison.OrdinalIgnoreCase)) goto SHIFT_DOWN;
         }
         j--;

         SHIFT_DOWN:
         for (; j>=1; j--)
         {
            history[j] = history[j - 1];
         }
         history[0] = fn;
      }
   }
}
