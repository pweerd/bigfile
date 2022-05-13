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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bitmanager.BigFile {

   /// <summary>
   /// Basically a VirtualObjectListView where the standard right mouse behavior is disabled.
   /// We need the right mouse click to be able to show a context menu in the main form
   /// </summary>
   public class SubclassedVirtualListView : BrightIdeasSoftware.VirtualObjectListView {
      [DllImport ("user32.dll")]
      static extern IntPtr DefWindowProc (IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);
      protected const int WM_RBUTTONDOWN = 0x0204;
      protected const int WM_RBUTTONUP = 0x0205;
      protected const int WM_CONTEXTMENU = 0x7B;

      protected override void WndProc (ref Message m) {
         switch (m.Msg) {
            case WM_RBUTTONDOWN: return;
            case WM_RBUTTONUP:
               DefWindowProc (m.HWnd, m.Msg, m.WParam, m.LParam);
               return;
         }
         base.WndProc (ref m);
      }
   }
}
