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

namespace Bitmanager.BigFile
{
   partial class FormGoToLine
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGoToLine));
         this.label1 = new System.Windows.Forms.Label();
         this.textLineNum = new System.Windows.Forms.TextBox();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonOK = new System.Windows.Forms.Button();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.rbLineIndex = new System.Windows.Forms.RadioButton();
         this.rbPartialLine = new System.Windows.Forms.RadioButton();
         this.rbRowIndex = new System.Windows.Forms.RadioButton();
         this.groupBox1.SuspendLayout();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(11, 15);
         this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(51, 15);
         this.label1.TabIndex = 0;
         this.label1.Text = "Line No.";
         // 
         // textLineNum
         // 
         this.textLineNum.Location = new System.Drawing.Point(92, 7);
         this.textLineNum.Margin = new System.Windows.Forms.Padding(2);
         this.textLineNum.Name = "textLineNum";
         this.textLineNum.Size = new System.Drawing.Size(142, 23);
         this.textLineNum.TabIndex = 1;
         // 
         // buttonCancel
         // 
         this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(190, 182);
         this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(93, 30);
         this.buttonCancel.TabIndex = 3;
         this.buttonCancel.Text = "Cancel";
         this.buttonCancel.UseVisualStyleBackColor = true;
         this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
         // 
         // buttonOK
         // 
         this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonOK.Location = new System.Drawing.Point(92, 182);
         this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
         this.buttonOK.Name = "buttonOK";
         this.buttonOK.Size = new System.Drawing.Size(93, 30);
         this.buttonOK.TabIndex = 2;
         this.buttonOK.Text = "OK";
         this.buttonOK.UseVisualStyleBackColor = true;
         this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
         // 
         // groupBox1
         // 
         this.groupBox1.Controls.Add(this.rbRowIndex);
         this.groupBox1.Controls.Add(this.rbPartialLine);
         this.groupBox1.Controls.Add(this.rbLineIndex);
         this.groupBox1.Location = new System.Drawing.Point(92, 48);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(180, 105);
         this.groupBox1.TabIndex = 5;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "Type";
         // 
         // rbLineIndex
         // 
         this.rbLineIndex.AutoSize = true;
         this.rbLineIndex.Location = new System.Drawing.Point(21, 22);
         this.rbLineIndex.Name = "rbLineIndex";
         this.rbLineIndex.Size = new System.Drawing.Size(79, 19);
         this.rbLineIndex.TabIndex = 0;
         this.rbLineIndex.Text = "Line index";
         this.rbLineIndex.UseVisualStyleBackColor = true;
         // 
         // rbPartiaLine
         // 
         this.rbPartialLine.AutoSize = true;
         this.rbPartialLine.Location = new System.Drawing.Point(21, 47);
         this.rbPartialLine.Name = "rbPartiaLine";
         this.rbPartialLine.Size = new System.Drawing.Size(90, 19);
         this.rbPartialLine.TabIndex = 1;
         this.rbPartialLine.Text = "Partial index";
         this.rbPartialLine.UseVisualStyleBackColor = true;
         // 
         // rbRowIndex
         // 
         this.rbRowIndex.AutoSize = true;
         this.rbRowIndex.Location = new System.Drawing.Point(21, 72);
         this.rbRowIndex.Name = "rbRowIndex";
         this.rbRowIndex.Size = new System.Drawing.Size(80, 19);
         this.rbRowIndex.TabIndex = 2;
         this.rbRowIndex.Text = "Row index";
         this.rbRowIndex.UseVisualStyleBackColor = true;
         // 
         // FormGoToLine
         // 
         this.AcceptButton = this.buttonOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.buttonCancel;
         this.ClientSize = new System.Drawing.Size(293, 220);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.buttonOK);
         this.Controls.Add(this.textLineNum);
         this.Controls.Add(this.label1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Margin = new System.Windows.Forms.Padding(2);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormGoToLine";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Go To Line";
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox textLineNum;
      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.Button buttonOK;
        private GroupBox groupBox1;
        private RadioButton rbRowIndex;
        private RadioButton rbPartialLine;
        private RadioButton rbLineIndex;
    }
}