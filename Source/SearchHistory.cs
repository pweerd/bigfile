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

using Bitmanager.BigFile.Query;
using Bitmanager.Core;
using Bitmanager.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bitmanager.BigFile {
   public class SearchHistory {
      private readonly ToolStripComboBox cb;
      private readonly List<String> history;
      private AutoCompleteStringCollection autoComplete;
      private readonly SearchNodes searchNodes;
      private readonly Logger logger;


      public SearchHistory (ToolStripComboBox cb) {
         logger = Globals.MainLogger.Clone ("sb-driver");
         this.cb = cb;
         history = new List<string> ();
         autoComplete = new AutoCompleteStringCollection ();
         autoComplete.Add ("piet");
         searchNodes = new SearchNodes ();

         cb.AutoCompleteCustomSource = autoComplete;
         cb.AutoCompleteSource = AutoCompleteSource.CustomSource;
         cb.AutoCompleteMode = AutoCompleteMode.None;

         //cb.TextUpdate += Cb_TextUpdate;
      }

      public void Clear () {
         searchNodes.Clear ();
      }

      private void Cb_TextUpdate (object sender, EventArgs e) {
         var cb = (ToolStripComboBox)sender;
         var txt = cb.Text;
         logger.Log ("Selected index: {0} len={1}", cb.SelectionStart, cb.SelectionLength);
         //cb.AutoCompleteMode = AutoCompleteMode.Suggest;
         //cb.Select(cb.Text.Length, 0);
         if (!txt.EndsWith (" ")) return;
         return;
         autoComplete = new AutoCompleteStringCollection ();
         autoComplete.Add ("piet");
         //foreach (String x in history) autoComplete.Add(x);
         foreach (String x in cb.Items) autoComplete.Add (x);

         if (txt.EndsWith ("AND ") || txt.EndsWith ("OR ") || txt.EndsWith ("NOT ")) {
            foreach (var n in searchNodes) autoComplete.Add (txt + n.ToString ());
            cb.AutoCompleteMode = AutoCompleteMode.Suggest;
            dumpAutocomplete ();
            return;
         }
         foreach (var n in searchNodes) {
            String x = n.ToString ();
            autoComplete.Add (txt + "AND " + x);
            autoComplete.Add (txt + "NOT " + x);
            autoComplete.Add (txt + "OR " + x);
         }
         cb.AutoCompleteCustomSource = autoComplete;
         cb.AutoCompleteMode = AutoCompleteMode.Suggest;
         dumpAutocomplete ();
      }

      private void dumpAutocomplete () {
         logger.Log ("Dumping AC:");
         foreach (var x in autoComplete) logger.Log ("-- [{0}]", x);
      }

      public ParserNode<SearchContext> GetParsedQuery () {
         String s = cb.Text;
         if (String.IsNullOrEmpty (s)) return null;

         int idx = history.IndexOf (s);
         if (idx >= 0) history.RemoveAt (idx);
         history.Insert (0, s);
         if (history.Count > 20)
            history.RemoveRange (20, history.Count - 20);

         cb.Items.Clear ();
         foreach (var x in history) cb.Items.Add (x);

         cb.Text = s;
         return searchNodes.Parse (s);
      }
   }

   public static class ComboboxExtensions {
      public static void AddHistory (this ToolStripComboBox cb, Object item, int max=int.MaxValue) {
         if (item == null) return;
         String key = item.ToString ();
         var dict = new Dictionary<String, Object> ();
         dict[key] = item;
         foreach (var x in cb.Items) dict[x.ToString ()] = x;
         cb.Items.Clear ();
         foreach (var kvp in dict) {
            cb.Items.Add (kvp.Value);
            if (--max <= 0) break;
         }
      }
   }
}
