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
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLine));
         this.statusStrip1 = new System.Windows.Forms.StatusStrip();
         this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
         this.timer1 = new System.Windows.Forms.Timer(this.components);
         this.toolStrip1 = new System.Windows.Forms.ToolStrip();
         this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
         this.cbViewAs = new System.Windows.Forms.ToolStripComboBox();
         this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
         this.menuNormalized = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.toolLabelSearch = new System.Windows.Forms.ToolStripLabel();
         this.cbSearch = new System.Windows.Forms.ToolStripComboBox();
         this.btnSearch = new System.Windows.Forms.ToolStripButton();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.btnPrev = new System.Windows.Forms.ToolStripButton();
         this.btnNext = new System.Windows.Forms.ToolStripButton();
         this.textLine = new System.Windows.Forms.RichTextBox();
         this.statusStrip1.SuspendLayout();
         this.toolStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // statusStrip1
         // 
         this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
         this.statusStrip1.Location = new System.Drawing.Point(0, 509);
         this.statusStrip1.Name = "statusStrip1";
         this.statusStrip1.Size = new System.Drawing.Size(1014, 22);
         this.statusStrip1.TabIndex = 2;
         this.statusStrip1.Text = "statusStrip1";
         // 
         // toolStripStatusLabel1
         // 
         this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
         this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
         this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
         // 
         // timer1
         // 
         this.timer1.Enabled = true;
         this.timer1.Interval = 1000;
         this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
         // 
         // toolStrip1
         // 
         this.toolStrip1.BackColor = System.Drawing.SystemColors.ButtonFace;
         this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
         this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.cbViewAs,
            this.toolStripDropDownButton1,
            this.toolStripSeparator2,
            this.toolLabelSearch,
            this.cbSearch,
            this.btnSearch,
            this.toolStripSeparator1,
            this.btnPrev,
            this.btnNext});
         this.toolStrip1.Location = new System.Drawing.Point(0, 0);
         this.toolStrip1.Name = "toolStrip1";
         this.toolStrip1.Size = new System.Drawing.Size(1014, 31);
         this.toolStrip1.TabIndex = 5;
         this.toolStrip1.Text = "toolStrip1";
         // 
         // toolStripLabel1
         // 
         this.toolStripLabel1.Name = "toolStripLabel1";
         this.toolStripLabel1.Size = new System.Drawing.Size(46, 28);
         this.toolStripLabel1.Text = "View as";
         // 
         // cbViewAs
         // 
         this.cbViewAs.Items.AddRange(new object[] {
            "Auto",
            "Text",
            "Json",
            "Xml",
            "Csv"});
         this.cbViewAs.Name = "cbViewAs";
         this.cbViewAs.Size = new System.Drawing.Size(121, 31);
         this.cbViewAs.SelectedIndexChanged += new System.EventHandler(this.cbViewAs_SelectedIndexChanged);
         // 
         // toolStripDropDownButton1
         // 
         this.toolStripDropDownButton1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
         this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNormalized});
         this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
         this.toolStripDropDownButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
         this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
         this.toolStripDropDownButton1.Size = new System.Drawing.Size(37, 28);
         this.toolStripDropDownButton1.Text = "Extra ettings";
         // 
         // menuNormalized
         // 
         this.menuNormalized.CheckOnClick = true;
         this.menuNormalized.Name = "menuNormalized";
         this.menuNormalized.Size = new System.Drawing.Size(180, 22);
         this.menuNormalized.Text = "Normalized";
         this.menuNormalized.ToolTipText = "Try normalize (sort) json-keys";
         this.menuNormalized.CheckStateChanged += new System.EventHandler(this.menuNormalized_CheckStateChanged);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
         // 
         // toolLabelSearch
         // 
         this.toolLabelSearch.Name = "toolLabelSearch";
         this.toolLabelSearch.Size = new System.Drawing.Size(42, 28);
         this.toolLabelSearch.Text = "Search";
         // 
         // cbSearch
         // 
         this.cbSearch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
         this.cbSearch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
         this.cbSearch.AutoToolTip = true;
         this.cbSearch.Name = "cbSearch";
         this.cbSearch.Size = new System.Drawing.Size(350, 31);
         this.cbSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
         // 
         // btnSearch
         // 
         this.btnSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.btnSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.Image")));
         this.btnSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.btnSearch.Name = "btnSearch";
         this.btnSearch.Size = new System.Drawing.Size(28, 28);
         this.btnSearch.ToolTipText = "Search";
         this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
         // 
         // btnPrev
         // 
         this.btnPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.btnPrev.Image = ((System.Drawing.Image)(resources.GetObject("btnPrev.Image")));
         this.btnPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.btnPrev.Name = "btnPrev";
         this.btnPrev.Size = new System.Drawing.Size(28, 28);
         this.btnPrev.Text = "Previous line (Ctrl-Up)";
         this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
         // 
         // btnNext
         // 
         this.btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.btnNext.Image = ((System.Drawing.Image)(resources.GetObject("btnNext.Image")));
         this.btnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.btnNext.Name = "btnNext";
         this.btnNext.Size = new System.Drawing.Size(28, 28);
         this.btnNext.Text = "Next line (Ctrl-Down)";
         this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
         // 
         // textLine
         // 
         this.textLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.textLine.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textLine.HideSelection = false;
         this.textLine.Location = new System.Drawing.Point(170, 177);
         this.textLine.Margin = new System.Windows.Forms.Padding(20);
         this.textLine.Name = "textLine";
         this.textLine.ReadOnly = true;
         this.textLine.Size = new System.Drawing.Size(490, 90);
         this.textLine.TabIndex = 6;
         this.textLine.Text = "boe";
         // 
         // FormLine
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1014, 531);
         this.Controls.Add(this.textLine);
         this.Controls.Add(this.toolStrip1);
         this.Controls.Add(this.statusStrip1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.KeyPreview = true;
         this.Margin = new System.Windows.Forms.Padding(2);
         this.MinimumSize = new System.Drawing.Size(405, 306);
         this.Name = "FormLine";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Line";
         this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLine_FormClosed);
         this.Load += new System.EventHandler(this.FormLine_Load);
         this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_KeyPress);
         this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.form_KeyUp);
         this.statusStrip1.ResumeLayout(false);
         this.statusStrip1.PerformLayout();
         this.toolStrip1.ResumeLayout(false);
         this.toolStrip1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion
      private System.Windows.Forms.StatusStrip statusStrip1;
      private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
      private System.Windows.Forms.Timer timer1;
      private System.Windows.Forms.ToolStrip toolStrip1;
      private System.Windows.Forms.ToolStripLabel toolLabelSearch;
      private System.Windows.Forms.ToolStripComboBox cbSearch;
      private System.Windows.Forms.ToolStripButton btnSearch;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripLabel toolStripLabel1;
      private System.Windows.Forms.ToolStripComboBox cbViewAs;
      private System.Windows.Forms.ToolStripButton btnPrev;
      private System.Windows.Forms.ToolStripButton btnNext;
      private System.Windows.Forms.RichTextBox textLine;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
      private System.Windows.Forms.ToolStripMenuItem menuNormalized;
   }
}