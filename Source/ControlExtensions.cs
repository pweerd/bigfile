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
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Helper routines for windows controls
   /// </summary>
   public static class ControlExtensions {
      private const int WM_USER = 0x0400;
      private const int EM_SETEVENTMASK = (WM_USER + 69);
      private const int WM_SETREDRAW = 0x0b;
      private static readonly IntPtr one = new IntPtr (1);

      [DllImport ("user32.dll", CharSet = CharSet.Auto)]
      private static extern IntPtr SendMessage (IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

      public static void BeginUpdate (this Control c) {
         SendMessage (c.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
      }
      public static void EndUpdate (this Control c) {
         SendMessage (c.Handle, WM_SETREDRAW, one, IntPtr.Zero);
         c.Refresh ();
      }
      public static IntPtr SendMessage (this Control c, int msg, int wParam, long lParam) {
         return SendMessage (c.Handle, msg, new IntPtr (wParam), new IntPtr (lParam));
      }
   }
}
