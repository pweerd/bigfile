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
        private void InitializeComponent()
        {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
         this.comboNumLines = new System.Windows.Forms.ComboBox();
         this.label1 = new System.Windows.Forms.Label();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonOK = new System.Windows.Forms.Button();
         this.colorDialog1 = new System.Windows.Forms.ColorDialog();
         this.label2 = new System.Windows.Forms.Label();
         this.txtHilight = new System.Windows.Forms.TextBox();
         this.label3 = new System.Windows.Forms.Label();
         this.txtContext = new System.Windows.Forms.TextBox();
         this.cbSearchThreads = new System.Windows.Forms.ComboBox();
         this.label5 = new System.Windows.Forms.Label();
         this.label6 = new System.Windows.Forms.Label();
         this.cbInMemory = new System.Windows.Forms.ComboBox();
         this.label7 = new System.Windows.Forms.Label();
         this.label8 = new System.Windows.Forms.Label();
         this.cbCompress = new System.Windows.Forms.ComboBox();
         this.label9 = new System.Windows.Forms.Label();
         this.cbLineLength = new System.Windows.Forms.ComboBox();
         this.label10 = new System.Windows.Forms.Label();
         this.label11 = new System.Windows.Forms.Label();
         this.txtMaxCopySize = new System.Windows.Forms.TextBox();
         this.txtMaxCopyLines = new System.Windows.Forms.TextBox();
         this.label4 = new System.Windows.Forms.Label();
         this.label12 = new System.Windows.Forms.Label();
         this.label13 = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // comboNumLines
         // 
         this.comboNumLines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboNumLines.FormattingEnabled = true;
         this.comboNumLines.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
         this.comboNumLines.Location = new System.Drawing.Point(116, 17);
         this.comboNumLines.Margin = new System.Windows.Forms.Padding(2);
         this.comboNumLines.Name = "comboNumLines";
         this.comboNumLines.Size = new System.Drawing.Size(62, 21);
         this.comboNumLines.TabIndex = 1;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(19, 20);
         this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(74, 13);
         this.label1.TabIndex = 2;
         this.label1.Text = "#Context lines";
         // 
         // buttonCancel
         // 
         this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(313, 364);
         this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(80, 26);
         this.buttonCancel.TabIndex = 4;
         this.buttonCancel.Text = "Cancel";
         this.buttonCancel.UseVisualStyleBackColor = true;
         this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
         // 
         // buttonOK
         // 
         this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonOK.Location = new System.Drawing.Point(229, 364);
         this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
         this.buttonOK.Name = "buttonOK";
         this.buttonOK.Size = new System.Drawing.Size(80, 26);
         this.buttonOK.TabIndex = 3;
         this.buttonOK.Text = "OK";
         this.buttonOK.UseVisualStyleBackColor = true;
         this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(19, 52);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(72, 13);
         this.label2.TabIndex = 5;
         this.label2.Text = "HighlightColor";
         // 
         // txtHilight
         // 
         this.txtHilight.Location = new System.Drawing.Point(116, 49);
         this.txtHilight.Name = "txtHilight";
         this.txtHilight.ReadOnly = true;
         this.txtHilight.Size = new System.Drawing.Size(100, 20);
         this.txtHilight.TabIndex = 6;
         this.txtHilight.Click += new System.EventHandler(this.colorBox_Click);
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(19, 83);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(67, 13);
         this.label3.TabIndex = 7;
         this.label3.Text = "ContextColor";
         // 
         // txtContext
         // 
         this.txtContext.Location = new System.Drawing.Point(116, 80);
         this.txtContext.Name = "txtContext";
         this.txtContext.ReadOnly = true;
         this.txtContext.Size = new System.Drawing.Size(100, 20);
         this.txtContext.TabIndex = 8;
         this.txtContext.Click += new System.EventHandler(this.colorBox_Click);
         // 
         // cbSearchThreads
         // 
         this.cbSearchThreads.FormattingEnabled = true;
         this.cbSearchThreads.Items.AddRange(new object[] {
            "auto",
            "1",
            "2",
            "3",
            "4"});
         this.cbSearchThreads.Location = new System.Drawing.Point(116, 131);
         this.cbSearchThreads.Name = "cbSearchThreads";
         this.cbSearchThreads.Size = new System.Drawing.Size(100, 21);
         this.cbSearchThreads.TabIndex = 14;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(19, 134);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(86, 13);
         this.label5.TabIndex = 15;
         this.label5.Text = "#Search threads";
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(226, 134);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(35, 13);
         this.label6.TabIndex = 16;
         this.label6.Text = "label6";
         // 
         // cbInMemory
         // 
         this.cbInMemory.FormattingEnabled = true;
         this.cbInMemory.Items.AddRange(new object[] {
            "Auto",
            "Off",
            "1GB",
            "2GB"});
         this.cbInMemory.Location = new System.Drawing.Point(116, 164);
         this.cbInMemory.Name = "cbInMemory";
         this.cbInMemory.Size = new System.Drawing.Size(101, 21);
         this.cbInMemory.TabIndex = 17;
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(19, 167);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(72, 13);
         this.label7.TabIndex = 18;
         this.label7.Text = "In memory if >";
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(19, 198);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(70, 13);
         this.label8.TabIndex = 20;
         this.label8.Text = "Compress if >";
         // 
         // cbCompress
         // 
         this.cbCompress.FormattingEnabled = true;
         this.cbCompress.Items.AddRange(new object[] {
            "Auto",
            "Off",
            "1GB",
            "2GB",
            "3GB",
            "4GB"});
         this.cbCompress.Location = new System.Drawing.Point(117, 195);
         this.cbCompress.Name = "cbCompress";
         this.cbCompress.Size = new System.Drawing.Size(100, 21);
         this.cbCompress.TabIndex = 21;
         // 
         // label9
         // 
         this.label9.AutoSize = true;
         this.label9.Location = new System.Drawing.Point(226, 167);
         this.label9.Name = "label9";
         this.label9.Size = new System.Drawing.Size(35, 13);
         this.label9.TabIndex = 22;
         this.label9.Text = "label9";
         // 
         // cbLineLength
         // 
         this.cbLineLength.FormattingEnabled = true;
         this.cbLineLength.Items.AddRange(new object[] {
            "auto",
            "1MB",
            "2MB",
            "10MB",
            "20MB",
            "32MB",
            "off"});
         this.cbLineLength.Location = new System.Drawing.Point(117, 226);
         this.cbLineLength.Name = "cbLineLength";
         this.cbLineLength.Size = new System.Drawing.Size(100, 21);
         this.cbLineLength.TabIndex = 23;
         // 
         // label10
         // 
         this.label10.AutoSize = true;
         this.label10.Location = new System.Drawing.Point(19, 229);
         this.label10.Name = "label10";
         this.label10.Size = new System.Drawing.Size(67, 13);
         this.label10.TabIndex = 24;
         this.label10.Text = "Max line size";
         // 
         // label11
         // 
         this.label11.AutoSize = true;
         this.label11.Location = new System.Drawing.Point(226, 229);
         this.label11.Name = "label11";
         this.label11.Size = new System.Drawing.Size(161, 13);
         this.label11.TabIndex = 25;
         this.label11.Text = "(when the line details are shown)";
         // 
         // txtMaxCopySize
         // 
         this.txtMaxCopySize.Location = new System.Drawing.Point(117, 277);
         this.txtMaxCopySize.Name = "txtMaxCopySize";
         this.txtMaxCopySize.Size = new System.Drawing.Size(100, 20);
         this.txtMaxCopySize.TabIndex = 26;
         // 
         // txtMaxCopyLines
         // 
         this.txtMaxCopyLines.Location = new System.Drawing.Point(117, 308);
         this.txtMaxCopyLines.Name = "txtMaxCopyLines";
         this.txtMaxCopyLines.Size = new System.Drawing.Size(100, 20);
         this.txtMaxCopyLines.TabIndex = 27;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(19, 311);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(96, 13);
         this.label4.TabIndex = 28;
         this.label4.Text = "Max #lines to copy";
         // 
         // label12
         // 
         this.label12.AutoSize = true;
         this.label12.Location = new System.Drawing.Point(19, 280);
         this.label12.Name = "label12";
         this.label12.Size = new System.Drawing.Size(74, 13);
         this.label12.TabIndex = 29;
         this.label12.Text = "Max copy size";
         // 
         // label13
         // 
         this.label13.AutoSize = true;
         this.label13.Location = new System.Drawing.Point(226, 280);
         this.label13.Name = "label13";
         this.label13.Size = new System.Drawing.Size(82, 13);
         this.label13.TabIndex = 30;
         this.label13.Text = "(Clipboard limits)";
         // 
         // FormSettings
         // 
         this.AcceptButton = this.buttonOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.buttonCancel;
         this.ClientSize = new System.Drawing.Size(401, 398);
         this.Controls.Add(this.label13);
         this.Controls.Add(this.label12);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.txtMaxCopyLines);
         this.Controls.Add(this.txtMaxCopySize);
         this.Controls.Add(this.label11);
         this.Controls.Add(this.label10);
         this.Controls.Add(this.cbLineLength);
         this.Controls.Add(this.label9);
         this.Controls.Add(this.cbCompress);
         this.Controls.Add(this.label8);
         this.Controls.Add(this.label7);
         this.Controls.Add(this.cbInMemory);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.cbSearchThreads);
         this.Controls.Add(this.txtContext);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.txtHilight);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.buttonOK);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.comboNumLines);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Margin = new System.Windows.Forms.Padding(2);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormSettings";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Configuration";
         this.Load += new System.EventHandler(this.FormSettings_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

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
      private System.Windows.Forms.ComboBox cbInMemory;
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
   }
}