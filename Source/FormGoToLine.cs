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
using System.Windows.Forms;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Let the user input a line number
   /// </summary>
   public partial class FormGoToLine : Form {
      static bool prevPartial = false;
      static String prevGoto = String.Empty;

      public int LineNumber { get { return Invariant.ToInt32 (prevGoto); } }
      public bool IsPartial { get { return prevPartial; } }

      public FormGoToLine () {
         InitializeComponent ();
         Bitmanager.Core.GlobalExceptionHandler.Hook ();
         textLineNum.Text = prevGoto;
         chkPartial.Checked = prevPartial;
      }

      public static void ResetGoto () {
         prevGoto = String.Empty;
      }

      private void buttonOK_Click (object sender, EventArgs e) {
         if (textLineNum.Text.Trim ().Length == 0)
            throw new Exception ("The line number must be entered");

         Invariant.ToInt32 (textLineNum.Text);
         prevGoto = textLineNum.Text;
         prevPartial = chkPartial.Checked;
         this.DialogResult = DialogResult.OK;
      }

      private void buttonCancel_Click (object sender, EventArgs e) {
         this.DialogResult = DialogResult.Cancel;
      }

   }
}
