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

using Bitmanager.Core;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Let the user input a line number
   /// </summary>
   public partial class FormGoToLine : Form {
      static GotoType prevGotoType = GotoType.Line;
      static string prevGoto = string.Empty;

      public int LineNumber { get { return Invariant.ToInt32 (prevGoto); } }
      public GotoType GotoType { get { return prevGotoType; } }

      public FormGoToLine () {
         InitializeComponent ();
         Bitmanager.Core.GlobalExceptionHandler.Hook ();
         textLineNum.Text = prevGoto;
         gotoTypeToForm (prevGotoType);
      }

      public static void ResetGoto () {
         prevGoto = string.Empty;
      }

      private void buttonOK_Click (object sender, EventArgs e) {
         if (textLineNum.Text.Trim ().Length == 0)
            throw new Exception ("The line number must be entered");

         Invariant.ToInt32 (textLineNum.Text);
         prevGoto = textLineNum.Text;
         prevGotoType = formToGotoType();
         this.DialogResult = DialogResult.OK;
      }

      private void buttonCancel_Click (object sender, EventArgs e) {
         this.DialogResult = DialogResult.Cancel;
      }

      private GotoType formToGotoType () {
         if (rbLineIndex.Checked) return GotoType.Line;
         if (rbPartialLine.Checked) return GotoType.PartialLine;
         if (rbRowIndex.Checked) return GotoType.Row;
         return GotoType.Line;
      }

      private void gotoTypeToForm (GotoType type) {
         switch(type) {
            case GotoType.Line:
               rbLineIndex.Checked = true;
               break;
            case GotoType.PartialLine:
               rbPartialLine.Checked = true;
               break;
            case GotoType.Row:
               rbRowIndex.Checked = true;
               break;
         }
      }
   }
}
