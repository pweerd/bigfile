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

namespace Bitmanager.BigFile
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent () {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (FormSettings));
         comboNumLines = new ComboBox ();
         label1 = new Label ();
         buttonCancel = new Button ();
         buttonOK = new Button ();
         colorDialog1 = new ColorDialog ();
         label2 = new Label ();
         txtHilight = new TextBox ();
         label3 = new Label ();
         txtContext = new TextBox ();
         cbSearchThreads = new ComboBox ();
         label5 = new Label ();
         label6 = new Label ();
         label7 = new Label ();
         label8 = new Label ();
         cbCompress = new ComboBox ();
         label9 = new Label ();
         cbLineLength = new ComboBox ();
         label10 = new Label ();
         label11 = new Label ();
         txtMaxCopySize = new TextBox ();
         txtMaxCopyLines = new TextBox ();
         label4 = new Label ();
         label12 = new Label ();
         label13 = new Label ();
         label14 = new Label ();
         txtHandledBySevenZip = new TextBox ();
         chkAllowInMemory = new CheckBox ();
         SuspendLayout ();
         // 
         // comboNumLines
         // 
         comboNumLines.DropDownStyle = ComboBoxStyle.DropDownList;
         comboNumLines.FormattingEnabled = true;
         comboNumLines.Items.AddRange (new object[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
         comboNumLines.Location = new Point (135, 20);
         comboNumLines.Margin = new Padding (2);
         comboNumLines.Name = "comboNumLines";
         comboNumLines.Size = new Size (72, 23);
         comboNumLines.TabIndex = 1;
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point (22, 23);
         label1.Margin = new Padding (2, 0, 2, 0);
         label1.Name = "label1";
         label1.Size = new Size (83, 15);
         label1.TabIndex = 2;
         label1.Text = "#Context lines";
         // 
         // buttonCancel
         // 
         buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         buttonCancel.DialogResult = DialogResult.Cancel;
         buttonCancel.Location = new Point (365, 420);
         buttonCancel.Margin = new Padding (2);
         buttonCancel.Name = "buttonCancel";
         buttonCancel.Size = new Size (93, 30);
         buttonCancel.TabIndex = 4;
         buttonCancel.Text = "Cancel";
         buttonCancel.UseVisualStyleBackColor = true;
         buttonCancel.Click += buttonCancel_Click;
         // 
         // buttonOK
         // 
         buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
         buttonOK.Location = new Point (267, 420);
         buttonOK.Margin = new Padding (2);
         buttonOK.Name = "buttonOK";
         buttonOK.Size = new Size (93, 30);
         buttonOK.TabIndex = 3;
         buttonOK.Text = "OK";
         buttonOK.UseVisualStyleBackColor = true;
         buttonOK.Click += buttonOK_Click;
         // 
         // label2
         // 
         label2.AutoSize = true;
         label2.Location = new Point (22, 51);
         label2.Margin = new Padding (4, 0, 4, 0);
         label2.Name = "label2";
         label2.Size = new Size (86, 15);
         label2.TabIndex = 5;
         label2.Text = "HighlightColor";
         // 
         // txtHilight
         // 
         txtHilight.Location = new Point (135, 48);
         txtHilight.Margin = new Padding (4, 3, 4, 3);
         txtHilight.Name = "txtHilight";
         txtHilight.ReadOnly = true;
         txtHilight.Size = new Size (116, 23);
         txtHilight.TabIndex = 6;
         txtHilight.Click += colorBox_Click;
         // 
         // label3
         // 
         label3.AutoSize = true;
         label3.Location = new Point (22, 80);
         label3.Margin = new Padding (4, 0, 4, 0);
         label3.Name = "label3";
         label3.Size = new Size (78, 15);
         label3.TabIndex = 7;
         label3.Text = "ContextColor";
         // 
         // txtContext
         // 
         txtContext.Location = new Point (135, 77);
         txtContext.Margin = new Padding (4, 3, 4, 3);
         txtContext.Name = "txtContext";
         txtContext.ReadOnly = true;
         txtContext.Size = new Size (116, 23);
         txtContext.TabIndex = 8;
         txtContext.Click += colorBox_Click;
         // 
         // cbSearchThreads
         // 
         cbSearchThreads.FormattingEnabled = true;
         cbSearchThreads.Items.AddRange (new object[] { "auto", "1", "2", "3", "4" });
         cbSearchThreads.Location = new Point (134, 128);
         cbSearchThreads.Margin = new Padding (4, 3, 4, 3);
         cbSearchThreads.Name = "cbSearchThreads";
         cbSearchThreads.Size = new Size (116, 23);
         cbSearchThreads.TabIndex = 14;
         // 
         // label5
         // 
         label5.AutoSize = true;
         label5.Location = new Point (22, 131);
         label5.Margin = new Padding (4, 0, 4, 0);
         label5.Name = "label5";
         label5.Size = new Size (91, 15);
         label5.TabIndex = 15;
         label5.Text = "#Search threads";
         // 
         // label6
         // 
         label6.AutoSize = true;
         label6.Location = new Point (267, 131);
         label6.Margin = new Padding (4, 0, 4, 0);
         label6.Name = "label6";
         label6.Size = new Size (38, 15);
         label6.TabIndex = 16;
         label6.Text = "label6";
         // 
         // label7
         // 
         label7.AutoSize = true;
         label7.Location = new Point (22, 160);
         label7.Margin = new Padding (4, 0, 4, 0);
         label7.Name = "label7";
         label7.Size = new Size (98, 15);
         label7.TabIndex = 18;
         label7.Text = "Allow in memory";
         // 
         // label8
         // 
         label8.AutoSize = true;
         label8.Location = new Point (22, 189);
         label8.Margin = new Padding (4, 0, 4, 0);
         label8.Name = "label8";
         label8.Size = new Size (81, 15);
         label8.TabIndex = 20;
         label8.Text = "Compress if >";
         // 
         // cbCompress
         // 
         cbCompress.FormattingEnabled = true;
         cbCompress.Items.AddRange (new object[] { "Auto", "Off", "1GB", "2GB", "3GB", "4GB" });
         cbCompress.Location = new Point (134, 186);
         cbCompress.Margin = new Padding (4, 3, 4, 3);
         cbCompress.Name = "cbCompress";
         cbCompress.Size = new Size (116, 23);
         cbCompress.TabIndex = 21;
         // 
         // label9
         // 
         label9.AutoSize = true;
         label9.Location = new Point (267, 189);
         label9.Margin = new Padding (4, 0, 4, 0);
         label9.Name = "label9";
         label9.Size = new Size (38, 15);
         label9.TabIndex = 22;
         label9.Text = "label9";
         // 
         // cbLineLength
         // 
         cbLineLength.FormattingEnabled = true;
         cbLineLength.Items.AddRange (new object[] { "auto", "1MB", "2MB", "10MB", "20MB", "32MB", "off" });
         cbLineLength.Location = new Point (134, 215);
         cbLineLength.Margin = new Padding (4, 3, 4, 3);
         cbLineLength.Name = "cbLineLength";
         cbLineLength.Size = new Size (116, 23);
         cbLineLength.TabIndex = 23;
         // 
         // label10
         // 
         label10.AutoSize = true;
         label10.Location = new Point (22, 218);
         label10.Margin = new Padding (4, 0, 4, 0);
         label10.Name = "label10";
         label10.Size = new Size (74, 15);
         label10.TabIndex = 24;
         label10.Text = "Max line size";
         // 
         // label11
         // 
         label11.AutoSize = true;
         label11.Location = new Point (264, 267);
         label11.Margin = new Padding (4, 0, 4, 0);
         label11.Name = "label11";
         label11.Size = new Size (180, 15);
         label11.TabIndex = 25;
         label11.Text = "(when the line details are shown)";
         // 
         // txtMaxCopySize
         // 
         txtMaxCopySize.Location = new Point (134, 264);
         txtMaxCopySize.Margin = new Padding (4, 3, 4, 3);
         txtMaxCopySize.Name = "txtMaxCopySize";
         txtMaxCopySize.Size = new Size (116, 23);
         txtMaxCopySize.TabIndex = 26;
         // 
         // txtMaxCopyLines
         // 
         txtMaxCopyLines.Location = new Point (134, 293);
         txtMaxCopyLines.Margin = new Padding (4, 3, 4, 3);
         txtMaxCopyLines.Name = "txtMaxCopyLines";
         txtMaxCopyLines.Size = new Size (116, 23);
         txtMaxCopyLines.TabIndex = 27;
         // 
         // label4
         // 
         label4.AutoSize = true;
         label4.Location = new Point (22, 296);
         label4.Margin = new Padding (4, 0, 4, 0);
         label4.Name = "label4";
         label4.Size = new Size (107, 15);
         label4.TabIndex = 28;
         label4.Text = "Max #lines to copy";
         // 
         // label12
         // 
         label12.AutoSize = true;
         label12.Location = new Point (22, 267);
         label12.Margin = new Padding (4, 0, 4, 0);
         label12.Name = "label12";
         label12.Size = new Size (81, 15);
         label12.TabIndex = 29;
         label12.Text = "Max copy size";
         // 
         // label13
         // 
         label13.AutoSize = true;
         label13.Location = new Point (264, 296);
         label13.Margin = new Padding (4, 0, 4, 0);
         label13.Name = "label13";
         label13.Size = new Size (99, 15);
         label13.TabIndex = 30;
         label13.Text = "(Clipboard limits)";
         // 
         // label14
         // 
         label14.AutoSize = true;
         label14.Location = new Point (22, 346);
         label14.Name = "label14";
         label14.Size = new Size (103, 15);
         label14.TabIndex = 32;
         label14.Text = "Handled by 7z.exe";
         // 
         // txtHandledBySevenZip
         // 
         txtHandledBySevenZip.Location = new Point (135, 343);
         txtHandledBySevenZip.Name = "txtHandledBySevenZip";
         txtHandledBySevenZip.Size = new Size (309, 23);
         txtHandledBySevenZip.TabIndex = 33;
         // 
         // chkAllowInMemory
         // 
         chkAllowInMemory.AutoSize = true;
         chkAllowInMemory.Location = new Point (135, 160);
         chkAllowInMemory.Name = "chkAllowInMemory";
         chkAllowInMemory.Size = new Size (15, 14);
         chkAllowInMemory.TabIndex = 34;
         chkAllowInMemory.UseVisualStyleBackColor = true;
         // 
         // FormSettings
         // 
         AcceptButton = buttonOK;
         AutoScaleDimensions = new SizeF (7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = buttonCancel;
         ClientSize = new Size (468, 459);
         Controls.Add (chkAllowInMemory);
         Controls.Add (txtHandledBySevenZip);
         Controls.Add (label14);
         Controls.Add (label13);
         Controls.Add (label12);
         Controls.Add (label4);
         Controls.Add (txtMaxCopyLines);
         Controls.Add (txtMaxCopySize);
         Controls.Add (label11);
         Controls.Add (label10);
         Controls.Add (cbLineLength);
         Controls.Add (label9);
         Controls.Add (cbCompress);
         Controls.Add (label8);
         Controls.Add (label7);
         Controls.Add (label6);
         Controls.Add (label5);
         Controls.Add (cbSearchThreads);
         Controls.Add (txtContext);
         Controls.Add (label3);
         Controls.Add (txtHilight);
         Controls.Add (label2);
         Controls.Add (buttonCancel);
         Controls.Add (buttonOK);
         Controls.Add (label1);
         Controls.Add (comboNumLines);
         FormBorderStyle = FormBorderStyle.FixedSingle;
         Icon = (Icon)resources.GetObject ("$this.Icon");
         Margin = new Padding (2);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormSettings";
         ShowInTaskbar = false;
         StartPosition = FormStartPosition.CenterParent;
         Text = "Configuration";
         Load += FormSettings_Load;
         ResumeLayout (false);
         PerformLayout ();

      }

      #endregion
      private System.Windows.Forms.ComboBox comboNumLines;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtHilight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtContext;
      private System.Windows.Forms.ComboBox cbSearchThreads;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.ComboBox cbCompress;
      private System.Windows.Forms.Label label9;
      private System.Windows.Forms.ComboBox cbLineLength;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.Label label11;
      private System.Windows.Forms.TextBox txtMaxCopySize;
      private System.Windows.Forms.TextBox txtMaxCopyLines;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Label label12;
      private System.Windows.Forms.Label label13;
      private Label label14;
      private TextBox txtHandledBySevenZip;
      private CheckBox chkAllowInMemory;
   }
}