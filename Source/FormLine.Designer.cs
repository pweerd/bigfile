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
   partial class FormLine
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLine));
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.textLine = new System.Windows.Forms.RichTextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.cbViewAs = new System.Windows.Forms.ComboBox();
         this.buttonPrev = new System.Windows.Forms.Button();
         this.buttonNext = new System.Windows.Forms.Button();
         this.buttonClose = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.SuspendLayout();
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
         this.splitContainer1.Location = new System.Drawing.Point(10, 10);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.textLine);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.label1);
         this.splitContainer1.Panel2.Controls.Add(this.cbViewAs);
         this.splitContainer1.Panel2.Controls.Add(this.buttonPrev);
         this.splitContainer1.Panel2.Controls.Add(this.buttonNext);
         this.splitContainer1.Panel2.Controls.Add(this.buttonClose);
         this.splitContainer1.Size = new System.Drawing.Size(994, 511);
         this.splitContainer1.SplitterDistance = 459;
         this.splitContainer1.TabIndex = 4;
         // 
         // textLine
         // 
         this.textLine.Dock = System.Windows.Forms.DockStyle.Fill;
         this.textLine.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textLine.HideSelection = false;
         this.textLine.Location = new System.Drawing.Point(0, 0);
         this.textLine.Margin = new System.Windows.Forms.Padding(2);
         this.textLine.Name = "textLine";
         this.textLine.ReadOnly = true;
         this.textLine.Size = new System.Drawing.Size(994, 459);
         this.textLine.TabIndex = 1;
         this.textLine.Text = "";
         this.textLine.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textLine_KeyPress);
         this.textLine.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textLine_KeyUp);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(21, 16);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(44, 13);
         this.label1.TabIndex = 8;
         this.label1.Text = "View as";
         // 
         // cbViewAs
         // 
         this.cbViewAs.FormattingEnabled = true;
         this.cbViewAs.Items.AddRange(new object[] {
            "Text",
            "Json",
            "Xml",
            "Csv"});
         this.cbViewAs.Location = new System.Drawing.Point(71, 13);
         this.cbViewAs.Name = "cbViewAs";
         this.cbViewAs.Size = new System.Drawing.Size(140, 21);
         this.cbViewAs.TabIndex = 7;
         this.cbViewAs.SelectedIndexChanged += new System.EventHandler(this.cbViewAs_SelectedIndexChanged);
         // 
         // buttonPrev
         // 
         this.buttonPrev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonPrev.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonPrev.Location = new System.Drawing.Point(705, 9);
         this.buttonPrev.Margin = new System.Windows.Forms.Padding(2);
         this.buttonPrev.Name = "buttonPrev";
         this.buttonPrev.Size = new System.Drawing.Size(80, 26);
         this.buttonPrev.TabIndex = 6;
         this.buttonPrev.Text = "Prev";
         this.buttonPrev.UseVisualStyleBackColor = true;
         this.buttonPrev.Click += new System.EventHandler(this.buttonPrev_Click);
         // 
         // buttonNext
         // 
         this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonNext.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonNext.Location = new System.Drawing.Point(800, 9);
         this.buttonNext.Margin = new System.Windows.Forms.Padding(2);
         this.buttonNext.Name = "buttonNext";
         this.buttonNext.Size = new System.Drawing.Size(80, 26);
         this.buttonNext.TabIndex = 5;
         this.buttonNext.Text = "Next";
         this.buttonNext.UseVisualStyleBackColor = true;
         this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
         // 
         // buttonClose
         // 
         this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonClose.Location = new System.Drawing.Point(898, 9);
         this.buttonClose.Margin = new System.Windows.Forms.Padding(2);
         this.buttonClose.Name = "buttonClose";
         this.buttonClose.Size = new System.Drawing.Size(80, 26);
         this.buttonClose.TabIndex = 4;
         this.buttonClose.Text = "Close";
         this.buttonClose.UseVisualStyleBackColor = true;
         this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
         // 
         // FormLine
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1014, 531);
         this.Controls.Add(this.splitContainer1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Margin = new System.Windows.Forms.Padding(2);
         this.MinimumSize = new System.Drawing.Size(405, 306);
         this.Name = "FormLine";
         this.Padding = new System.Windows.Forms.Padding(10);
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Line";
         this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLine_FormClosed);
         this.Load += new System.EventHandler(this.FormLine_Load);
         this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textLine_KeyPress);
         this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textLine_KeyUp);
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         this.splitContainer1.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.RichTextBox textLine;
      private System.Windows.Forms.Button buttonPrev;
      private System.Windows.Forms.Button buttonNext;
      private System.Windows.Forms.Button buttonClose;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.ComboBox cbViewAs;
   }
}