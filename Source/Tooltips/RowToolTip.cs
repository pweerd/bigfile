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

namespace Bitmanager.BigFile {
   /// <summary>
   /// Tooltip for the row column
   /// </summary>
   public class RowToolTip : TooltipHelperBase {
      private readonly GridLines _grid;
      private int row;
      public RowToolTip (GridLines grid, Logger logger) : base (grid, 2000, 10000, logger) {
         _grid = grid;
         row = int.MinValue;
      }
      protected override string GetText () {
         return _grid.GetTooltipForRow (row);
      }

      public void Start (int x, int y, int row) {
         logger?.Log ("RowToolTip::START ({0}, {1}, {2}, row={3} h={4}", x, y, row, this.row, tooltip.Height);

         if (this.row == row) return;

         var rect = _grid.GetCellBounds (row, 0);
         this.row = row;
         base.Start (rect.Right, rect.Y);
      }
      public override void Stop () {
         row = int.MinValue;
         base.Stop ();
      }

      protected override Rectangle GetParentRect () {
         return _grid.InnerClientRectangle;
      }

   }
}
