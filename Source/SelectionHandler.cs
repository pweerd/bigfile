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
using Bitmanager.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bitmanager.BigFile {
   public delegate void SelectionEventHandler (int from, int to);

   /// <summary>
   /// Module to handle selection logic for the Grid component.
   /// Original reason was that the Listview handles selections very slow in case of lots of selected elements,
   /// But today we  implemented our own Grid. This Grid doesn't do selection at all...
   /// 
   /// This class hooks mouse and key events and handles them.
   /// The caller needs to handle selection ranges via the supplied events.
   /// 
   /// NB: the rows in this class are grid-indexes. They must be translated into partial lines by our caller!
   /// </summary>
   public class SelectionHandler {
      private enum InducedBy { Other, Mouse, Keypress, Self };
      public event SelectionEventHandler OnToggleSelection;
      public event SelectionEventHandler OnAddSelection;
      public event SelectionEventHandler OnRemoveSelection;
      public readonly RawGrid Grid;
      private Logger logger;

      private InducedBy inducedBy;
      private bool complex;
      private int low, high;
      private int prevRow;

      public SelectionHandler (FormMain form, RawGrid grid) {
         this.Grid = grid;
         this.logger = Globals.MainLogger.Clone ("select");
         prevRow = -1;
         grid.MouseDown += Grid_MouseDown;
         grid.SelectedIndexChanged += Grid_SelectedIndexChanged;
         grid.KeyDown += Grid_KeyDown;
      }

      public void NotifyExternalChange () {
         complex = true;
         low = -1;
         high = -1;
         prevRow = -1;
      }

      public bool SingleLineSelected {
         get {
            return low >=0 && !complex && (low + 1 == high);
         }
      }
      public int SelectedIndex {
         get {
            return SingleLineSelected ? low : -1;
         }
      }

      public void Clear () {
         inducedBy = InducedBy.Other;
         complex = false;
         low = -1;
         high = -1;
         prevRow = -1;
      }

      #region EventHandlers for Grid
      private void Grid_KeyDown (object sender, System.Windows.Forms.KeyEventArgs e) {
         switch (e.KeyCode) {
            default: return;
            case Keys.A:
               logger.Log ();
               logger.Log ("KeyDown: key={0}, prev={1} complex={2}", e.KeyCode, prevRow, complex);
               if (e.Control) {
                  complex = false;
                  low = 0;
                  high = Grid.RowCount;
                  prevRow = -1;
                  addSelection (0, high);
               }
               return;

            case Keys.Home:
            case Keys.End:
               if (!e.Control) return;
               break;
            case Keys.Up:
            case Keys.Down:
            case Keys.PageDown:
            case Keys.PageUp:
               break;
         }

         logger.Log ();
         logger.Log ("KeyDown: key={0}, prev={1} complex={2}", e.KeyCode, prevRow, complex);
         if (e.Shift) {
            if (prevRow >= 0) inducedBy = InducedBy.Keypress;
            return;
         }

         prevRow = -1;
         inducedBy = InducedBy.Keypress;
      }

      private void Grid_MouseDown (object sender, System.Windows.Forms.MouseEventArgs e) {
         if (e.Button != MouseButtons.Left) return;
         logger.Log ();
         logger.Log ("LMouseDown: prev={0} complex={1}, mods={2}", prevRow, complex, Control.ModifierKeys);
         inducedBy = InducedBy.Mouse;
         int row = Grid.GetRowFromLocation (e.Y);
         if (row < 0) return;

         Keys mods = Control.ModifierKeys;
         if ((mods & Keys.Control) != 0) {
            toggle (row);
            return;
         }

         if ((mods & Keys.Shift) != 0) {
            if (complex) {
               select (prevRow, row, row);
               return;
            }
            if (low < 0 || (row == low && row + 1 == high)) {
               select (row);
               return;
            }

            if (row < prevRow)
               select (row, prevRow + 1, prevRow);
            else
               select (prevRow, row + 1, prevRow);
            return;
         }

         select (row);
      }

      private void Grid_SelectedIndexChanged (object sender, EventArgs e) {
         int row = Grid.SelectedIndex;
         logger.Log ("SelectedIndexChanged: row={0}, inducedBy={1}", row, inducedBy);
         if (row < 0) return;

         //PW
         ////Force the selection not to be happen in the listview itself by deselecting immediately
         ////Reason: it is impossible to reliable select colors for selected items in the listview
         //Grid.SelectedIndex = -1;

         switch (inducedBy) {
            case InducedBy.Self: goto RESET; //Selection already handled 
            case InducedBy.Mouse: goto RESET; //Selection already handled in mouse-procedure 
            case InducedBy.Keypress:
               if (prevRow < 0)
                  select (row);
               else if (row < prevRow)
                  select (row, prevRow + 1, prevRow);
               else
                  select (prevRow, row + 1, prevRow);
               break;

            default:
               prevRow = -1;
               select (row);
               break;
         }

      RESET:
         inducedBy = InducedBy.Other;
      }

      #endregion

      #region event-handlers
      protected virtual void toggleSelection (int from, int to) {
         dump ("-- ToggleSelection ({0}, {1})", from, to);
         if (OnToggleSelection != null) OnToggleSelection (from, to);
      }
      protected virtual void addSelection (int from, int to) {
         dump ("-- AddSelection ({0}, {1})", from, to);
         if (OnAddSelection != null) OnAddSelection (from, to);
         if (SingleLineSelected) {
            inducedBy = InducedBy.Self;
            Grid.SelectedIndex = from;
         }
      }
      protected virtual void removeSelection (int from, int to) {
         dump ("-- RemoveSelection ({0}, {1})", from, to);
         if (OnRemoveSelection != null) OnRemoveSelection (from, to);
      }
      #endregion

      private void dump (String fmt, params Object[] args) {
         logger.Log (fmt, args);
         String[] stack = Environment.StackTrace.Split ('\n');
         int N = stack.Length;
         if (N > 7) N = 7;

         for (int i = 3; i < N; i++)
            logger.Log ("-- -- {0}", stack[i]);
      }


      private void select (int row) {
         if (complex)
            deselectAll ();
         else {
            if (low >= 0 && (low != row || high != row + 1))
               removeSelection (low, high);
         }
         complex = false;
         low = row;
         high = row + 1;
         prevRow = row;
         addSelection (row, row + 1);
      }

      private void select (int from, int to, int row) {
         if (complex)
            deselectAll ();
         else {
            if (low >= 0 && (low != from || high != to))
               removeSelection (low, high);
         }
         complex = false;
         low = from;
         high = to;
         prevRow = row;
         addSelection (from, to);
      }

      private void toggle (int row) {
         complex = true;
         low = row;
         high = row + 1;
         prevRow = row;
         toggleSelection (row, row + 1);
      }

      private void deselectAll () {
         removeSelection (0, Grid.RowCount);
         Clear ();
      }

   }
}
