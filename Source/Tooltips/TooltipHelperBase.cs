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
using static System.Net.Mime.MediaTypeNames;
using Timer = System.Windows.Forms.Timer;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Simple tooltip, base on a label
   /// </summary>
   public abstract class TooltipHelperBase : IDisposable {
      protected readonly Logger logger;
      protected readonly Timer timer;
      protected readonly Label tooltip;
      protected bool Visible { 
         get { return tooltip.Visible; }
         set { tooltip.Visible = value; }
      }
      public int delay;
      public int duration;

      public readonly Control Parent;
      public int X { get; private set; }
      public int Y { get; private set; }

      public TooltipHelperBase (Control parent, int delay, int duration, Logger logger = null) {
         this.logger = logger;
         this.Parent = parent;
         this.duration = duration;
         this.delay = delay;
         timer = new Timer ();
         timer.Tick += Timer_Tick;
         timer.Enabled = false;

         tooltip = new Label ();
         tooltip.BackColor = Color.LightYellow;
         tooltip.BorderStyle = BorderStyle.FixedSingle;
         tooltip.Visible = false;
         tooltip.AutoSize = true;
         tooltip.Padding = new Padding (3);
         Control controlForFont = parent.Parent;
         if (controlForFont == null) controlForFont = parent;
         tooltip.Font = controlForFont.Font;
         Parent.Controls.Add (tooltip);
      }

      protected void Timer_Tick (object sender, EventArgs e) {
         logger?.Log ("Timer CB, visible={0}", tooltip.Visible);
         try {
            timer.Enabled = false;
            if (tooltip.Visible) _hide (); else _show (GetText (), X, Y, duration);
         } catch (Exception ee) {
            Logs.ErrorLog.Log (ee);
         }
      }

      protected virtual Rectangle GetParentRect () {
         return Parent.ClientRectangle;
      }

      //The interval needs to be different, otherwise the timer is not restarted
      private void restartTimer (int duration) {
         if (duration == -1) return;
         timer.Interval = (timer.Interval == duration) ? duration + 1 : duration;
         timer.Enabled = true;
      }
      private void _show (String txt, int x, int y, int duration) {
         timer.Enabled = false;
         if (String.IsNullOrEmpty (txt)) return;

         restartTimer (duration);
         tooltip.Text = txt;
         tooltip.Top = y;
         tooltip.Left = x;
         tooltip.Visible = true;

         Rectangle rect = GetParentRect ();
         int w = tooltip.Width;
         int h = tooltip.Height;
         bool needReposX = true, needReposY = true;
         if (y < 0) y = 0;
         else if (y + h > rect.Height) y = rect.Height - h;
         else needReposY = false;
         if (x < 0) x = 0;
         else if (x + w > rect.Width) x = rect.Width - w;
         else needReposX = false;

         if (needReposY) tooltip.Top = y;
         if (needReposX) tooltip.Left = x;
         tooltip.BringToFront ();
      }
      private void _hide () {
         timer.Enabled = false;
         tooltip.Visible = false;
      }

      protected abstract String GetText ();

      protected virtual void Hide () {
         _hide ();
      }

      public void Start (int x, int y) {
         timer.Enabled = false;
         bool visible = tooltip.Visible;
         logger?.Log ("TooltipHelperBase::START ({0}, {1}), visible={2}", x, y, visible);
         this.X = x;
         this.Y = y;
         if (visible || delay==0) _show (GetText (), x, y, duration);
         else restartTimer (delay);
      }

      public void Show (int x, int y) {
         logger?.Log ("TooltipHelperBase::show({0}, {1})", x, y);
         _show (GetText (), X, Y, duration);
      }
      public void Show (int x, int y, String txt) {
         logger?.Log ("TooltipHelperBase::show({0}, {1}, {2})", x, y, txt);
         _show (txt, X, Y, duration);
      }

      public virtual void Stop () {
         logger?.Log ("TooltipHelperBase::Stop()");
         _hide ();
      }

      public void Dispose () {
         Stop ();
         timer.Dispose ();
         tooltip.Dispose ();
      }
   }
}
