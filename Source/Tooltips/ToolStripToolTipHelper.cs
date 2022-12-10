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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Bitmanager.BigFile {
   public class ToolStripToolTipHelper : TooltipHelperBase {
      protected readonly ToolStrip target;
      protected readonly int defDelay, defDuration;
      protected ToolStripItem mouseOverItem = null;

      public ToolStripToolTipHelper (ToolStrip target, Logger logger = null): base(target.Parent, 1000, 4000, logger) {
         defDelay = delay;
         defDuration = duration; 
         this.target = target;
         target.ShowItemToolTips = false;

         foreach (ToolStripItem x in target.Items) {
            x.MouseEnter += Item_MouseEnter;
            x.MouseDown += Item_MouseDown;
            x.MouseLeave += Item_MouseLeave;
         }
         //tooltip.Font = new Font (tooltip.Font.FontFamily, tooltip.Font.Size + 1);
      }

      protected override string GetText () {
         return mouseOverItem.ToolTipText.TrimToNull();
      }


      private void checkAndStart (ToolStripItem item) {
         logger?.Log ("checkAndStart: new={0}, existing={1}", mouseOverItem?.Name, item?.Name);
         if (item == null || String.IsNullOrEmpty(item.ToolTipText)) Stop ();
         else {
            if (item == mouseOverItem) return;
            stop ();
            mouseOverItem = item;
            var bounds = item.Bounds;
            logger?.Log ("Bounds={0}", bounds);
            var times = item.Tag as TooltipTimes;
            if (times==null) {
               duration = defDuration;
               delay = defDelay;
            } else {
               duration = times.Duration;
               delay = times.Delay;
            }
            Start (bounds.Left, target.Bottom);
         }
      }
      private void stop() {
         if (mouseOverItem != null) {
            Stop ();
            logger?.Log ("ToolStripItem:stop");
         }
         mouseOverItem = null;
      }

      #region events
      protected virtual void Item_MouseLeave (Object sender, EventArgs e) {
         stop ();
      }
      protected virtual void Item_MouseEnter (Object sender, EventArgs e) {
         logger?.Log ("Item_MouseEnter over {0}", ((ToolStripItem)sender).Name);
         checkAndStart (sender as ToolStripItem);
      }
      protected virtual void Item_MouseDown (Object sender, MouseEventArgs mea) {
         logger?.Log ("Item_MouseDown over {0}", ((ToolStripItem)sender).Name);
         stop ();
      }
#endregion events


   }
}
