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

using Bitmanager.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bitmanager.BigFile {
   public partial class FormAbout : Form {
      static readonly string year;
      static FormAbout () {
         var attributes = Assembly.GetExecutingAssembly ().GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
         if (attributes.Length > 0) {
            var copyRight = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            var match = Regex.Match (copyRight, @" (\d\d\d\d)");
            if (match.Success) year = match.Groups[1].ToString ();
         }
      }
      public FormAbout () {
         InitializeComponent ();

         lblApp.Text = Application.ProductName + " v" + Application.ProductVersion;
      }

      private void btnClose_Click (object sender, System.EventArgs e) {
         this.DialogResult = DialogResult.OK;
      }

      private void FormAbout_Load (object sender, System.EventArgs e) {
         if (Globals.IsDebug) {
            var fn = IOUtils.FindFileToRoot (Globals.LoadDir + @"\about.txt", FindToTootFlags.Except);
            richTextBox1.Text = IOUtils.LoadFromFile (fn);
         }
         if (year != null) richTextBox1.Rtf = richTextBox1.Rtf.Replace ("2019", year);
         richTextBox1.ShowSelectionMargin = true;
         var m = richTextBox1.Margin;
         m.All = 10;
      }

      private void richTextBox1_LinkClicked (object sender, LinkClickedEventArgs e) {
         System.Diagnostics.Process.Start (e.LinkText);
      }
   }
}
