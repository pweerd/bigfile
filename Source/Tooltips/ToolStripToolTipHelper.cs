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
   public class ToolStripToolTipHelpers {
      private List<ToolStripToolTipHelper> helpers;

      public ToolStripToolTipHelpers () {
         helpers = new List<ToolStripToolTipHelper> ();
      }

      public void Add (ToolStrip strip, Action<ToolStripToolTipHelper> customizer) {
         foreach (ToolStripItem item in strip.Items) {
            if (String.IsNullOrEmpty (item.ToolTipText)) continue;
            if (isPresent (item)) continue;

            var tth = new ToolStripToolTipHelper (strip, item);
            if (customizer != null) customizer(tth);
            helpers.Add (tth);
         }
      }
      public void Add (ToolStripToolTipHelper helper) {
         if (!isPresent (helper.targetItem)) helpers.Add (helper);
      }

      private bool isPresent (ToolStripItem item) {
         foreach (var h in helpers) {
            if (h.targetItem == item) return true;
         }
         return false;
      }
   }


   public class ToolStripToolTipHelper {
      internal readonly ToolStrip target;
      internal readonly ToolStripItem targetItem;

      ToolStripItem mouseOverItem = null;
      Point mouseOverPoint;
      private readonly Timer timer;
      public readonly ToolTip Tooltip;
      public int ToolTipInterval = 4000;
      public string ToolTipText;
      public bool ToolTipShowUp;

      private readonly Logger logger;

      public ToolStripToolTipHelper (ToolStrip target, ToolStripItem targetItem = null) {
         logger = Globals.TooltipLogger;
         this.target = target;
         this.targetItem = targetItem;

         if (targetItem != null) {
            targetItem.MouseMove += Item_MouseMove;
            targetItem.MouseEnter += Item_MouseEnter;
            targetItem.MouseDown += Item_MouseDown;
            targetItem.MouseLeave += Item_MouseLeave;
         } else {
            target.MouseMove += Target_MouseMove;
            target.MouseDown += Target_MouseDown;
            target.MouseLeave += Target_MouseLeave;
         }
         target.ShowItemToolTips = false;

         timer = new Timer ();
         timer.Enabled = false;
         timer.Interval = 2000;// SystemInformation.MouseHoverTime;
         timer.Tick += new EventHandler (timer_Tick);
         Tooltip = new ToolTip ();
      }
      protected virtual void Target_MouseMove (Object sender, MouseEventArgs mea) {
         ToolStripItem newMouseOverItem = target.GetItemAt (mea.Location);
         //logger.Log("MM: {0}", tos(newMouseOverItem));

         if (mouseOverItem != newMouseOverItem ||
             (Math.Abs (mouseOverPoint.X - mea.X) > SystemInformation.MouseHoverSize.Width || (Math.Abs (mouseOverPoint.Y - mea.Y) > SystemInformation.MouseHoverSize.Height))) {
            mouseOverItem = newMouseOverItem;
            mouseOverPoint = mea.Location;
            if (Tooltip != null)
               Tooltip.Hide (target);
            timer.Stop ();
            timer.Start ();
            //logger.Log("MM: {0}, start timer", tos(newMouseOverItem));
         }
      }
      protected virtual void Item_MouseMove (Object sender, MouseEventArgs mea) {
         mouseOverPoint = mea.Location;
      }
      protected virtual void Item_MouseEnter (Object sender, EventArgs e) {
         ToolStripItem newMouseOverItem = (ToolStripItem)sender;
         //logger.Log("enter: {0}", tos(newMouseOverItem));

         if (mouseOverItem != newMouseOverItem) {
            mouseOverItem = newMouseOverItem;
            Tooltip.Hide (target);
            timer.Stop ();
            timer.Start ();
            //logger.Log("MMI: {0}, start timer", tos(newMouseOverItem));
         }
      }

      private String tos (ToolStripItem x) {
         if (x == null) return "null";
         return String.Format ("{0}: {1}", x.Name, x.GetType ().Name);
      }

      protected virtual void Target_MouseDown (Object sender, MouseEventArgs mea) {
         ToolStripItem newMouseOverItem = target.GetItemAt (mea.Location);
         if (newMouseOverItem != null) {
            Tooltip.Hide (target);
         }
      }
      protected virtual void Item_MouseDown (Object sender, MouseEventArgs mea) {
         ToolStripItem newMouseOverItem = (ToolStripItem)sender;
         if (newMouseOverItem != null) {
            Tooltip.Hide (target);
         }
      }

      protected virtual void Target_MouseLeave (Object sender, EventArgs e) {
         timer.Stop ();
         Tooltip.Hide (target);
         mouseOverPoint = new Point (-50, -50);
         mouseOverItem = null;
      }
      protected virtual void Item_MouseLeave (Object sender, EventArgs e) {
         //logger.Log("Leave: {0}, stop", tos(sender as ToolStripItem));
         timer.Stop ();
         Tooltip.Hide (target);
         mouseOverPoint = new Point (-50, -50);
         mouseOverItem = null;
      }

      void timer_Tick (object sender, EventArgs e) {
         timer.Stop ();
         timer.Interval = Tooltip.ReshowDelay;
         Cursor cursor = Cursor.Current;
         if (cursor == null) return; //cursor is null if the mouse is invisible
         try {
            //logger.Log("Timer: {0}", tos(mouseOverItem));
            Point currentMouseOverPoint;
            if (ToolTipShowUp)
               currentMouseOverPoint = target.PointToClient (new Point (Control.MousePosition.X, Control.MousePosition.Y - cursor.Size.Height + cursor.HotSpot.Y));
            else
               currentMouseOverPoint = target.PointToClient (new Point (Control.MousePosition.X, Control.MousePosition.Y + cursor.Size.Height - cursor.HotSpot.Y));

            showToolTip (currentMouseOverPoint);
         } catch { }
      }

      protected virtual bool canDoToolTip (ToolStripItem x) {
         var ddi = x as ToolStripDropDownItem;
         return ddi == null || !ddi.DropDown.Visible;
      }
      protected virtual void showToolTip (Point currentMouseOverPoint) {
         //logger.Log("Show: {0}", tos(mouseOverItem));
         String txt = ToolTipText;

         if (mouseOverItem != null) {
            if (!canDoToolTip (mouseOverItem)) return;
            if (txt == null) txt = mouseOverItem.ToolTipText;
         }

         //logger.Log("Show: {0}, txt={1}", tos(mouseOverItem), txt);
         if (String.IsNullOrEmpty (txt)) return;

         Tooltip.Show (txt, target, currentMouseOverPoint, ToolTipInterval);
      }

   }
}
