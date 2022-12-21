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
using System.Drawing;
using System.Windows.Forms;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Displays the settings (stored in the registry)
   /// </summary>
   public partial class FormSettings : Form {
      private readonly SettingsSource settingsSource;

      public FormSettings (SettingsSource settings) {
         InitializeComponent ();
         this.settingsSource = settings;
         comboNumLines.SelectedIndex = this.settingsSource.NumContextLines;
         txtHilight.Text = ColorTranslator.ToHtml (settings.HighlightColor);
         txtContext.Text = ColorTranslator.ToHtml (settings.ContextColor);
         cbSearchThreads.Text = settings.SearchThreads.Value;
         cbCompress.Text = settings.CompressMemoryIfBigger.Value;
         cbInMemory.Text = settings.LoadMemoryIfBigger.Value;
         txtMaxCopyLines.Text = settingsSource.MaxCopyLines.Value;
         txtMaxCopySize.Text = settingsSource.MaxCopySize.Value;
         label6.Text = Invariant.Format ("(#cpu's: {0})", Environment.ProcessorCount);
      }

      private void buttonOK_Click (object sender, EventArgs e) {
         settingsSource.HighlightColor.Set (txtHilight.Text);
         settingsSource.ContextColor.Set (txtContext.Text);
         settingsSource.NumContextLines.Set (comboNumLines.SelectedIndex.ToString ());
         settingsSource.SearchThreads.Set (cbSearchThreads.Text);
         settingsSource.CompressMemoryIfBigger.Set (cbCompress.Text);
         settingsSource.LoadMemoryIfBigger.Set (cbInMemory.Text);
         settingsSource.MaxLineLength.Set (cbLineLength.Text);
         settingsSource.MaxCopyLines.Set (txtMaxCopyLines.Text);
         settingsSource.MaxCopySize.Set (txtMaxCopySize.Text);
         settingsSource.Save ();
         settingsSource.ActualizeDefaults ();
         settingsSource.Dump ("settings changed");

         this.DialogResult = DialogResult.OK;
      }

      private void buttonCancel_Click (object sender, EventArgs e) {
         this.DialogResult = DialogResult.Cancel;
      }

      private void colorBox_Click (object sender, EventArgs e) {
         var t = (TextBox)sender;
         colorDialog1.Color = ColorTranslator.FromHtml (t.Text);
         if (colorDialog1.ShowDialog () != DialogResult.OK) return;
         t.Text = ColorTranslator.ToHtml (colorDialog1.Color);
         t.Tag = colorDialog1.Color;
      }


      private void FormSettings_Load (object sender, EventArgs e) {
         label9.Text = "";
         if (!Globals.CanCompress) {
            label9.Text = "No or old bmucore102_64.dll";
            cbCompress.Enabled = false;
         }
      }

   }
}
