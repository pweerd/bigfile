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
      private void InitializeComponent () {
         components = new System.ComponentModel.Container ();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (FormLine));
         statusStrip1 = new StatusStrip ();
         toolStripStatusLabel1 = new ToolStripStatusLabel ();
         timer1 = new System.Windows.Forms.Timer (components);
         toolStrip1 = new ToolStrip ();
         toolStripLabel1 = new ToolStripLabel ();
         cbViewAs = new ToolStripComboBox ();
         toolStripSeparator3 = new ToolStripSeparator ();
         toolStripDropDownButton1 = new ToolStripDropDownButton ();
         menuNormalized = new ToolStripMenuItem ();
         menuExpandJson = new ToolStripMenuItem ();
         toolStripSeparator2 = new ToolStripSeparator ();
         toolLabelSearch = new ToolStripLabel ();
         cbSearch = new ToolStripComboBox ();
         btnSearch = new ToolStripButton ();
         toolStripSeparator1 = new ToolStripSeparator ();
         btnPrev = new ToolStripButton ();
         btnNext = new ToolStripButton ();
         textLine = new RichTextBox ();
         statusStrip1.SuspendLayout ();
         toolStrip1.SuspendLayout ();
         SuspendLayout ();
         // 
         // statusStrip1
         // 
         statusStrip1.Items.AddRange (new ToolStripItem[] { toolStripStatusLabel1 });
         statusStrip1.Location = new Point (0, 591);
         statusStrip1.Name = "statusStrip1";
         statusStrip1.Size = new Size (1183, 22);
         statusStrip1.TabIndex = 2;
         statusStrip1.Text = "statusStrip1";
         // 
         // toolStripStatusLabel1
         // 
         toolStripStatusLabel1.Name = "toolStripStatusLabel1";
         toolStripStatusLabel1.Size = new Size (118, 17);
         toolStripStatusLabel1.Text = "toolStripStatusLabel1";
         // 
         // timer1
         // 
         timer1.Enabled = true;
         timer1.Interval = 1000;
         timer1.Tick += timer1_Tick;
         // 
         // toolStrip1
         // 
         toolStrip1.BackColor = SystemColors.ButtonFace;
         toolStrip1.ImageScalingSize = new Size (24, 24);
         toolStrip1.Items.AddRange (new ToolStripItem[] { toolStripLabel1, cbViewAs, toolStripSeparator3, toolStripDropDownButton1, toolStripSeparator2, toolLabelSearch, cbSearch, btnSearch, toolStripSeparator1, btnPrev, btnNext });
         toolStrip1.Location = new Point (0, 0);
         toolStrip1.Name = "toolStrip1";
         toolStrip1.Size = new Size (1183, 31);
         toolStrip1.TabIndex = 5;
         toolStrip1.Text = "toolStrip1";
         // 
         // toolStripLabel1
         // 
         toolStripLabel1.Name = "toolStripLabel1";
         toolStripLabel1.Size = new Size (46, 28);
         toolStripLabel1.Text = "View as";
         // 
         // cbViewAs
         // 
         cbViewAs.Name = "cbViewAs";
         cbViewAs.Size = new Size (140, 31);
         cbViewAs.SelectedIndexChanged += cbViewAs_SelectedIndexChanged;
         // 
         // toolStripSeparator3
         // 
         toolStripSeparator3.Name = "toolStripSeparator3";
         toolStripSeparator3.Size = new Size (6, 31);
         // 
         // toolStripDropDownButton1
         // 
         toolStripDropDownButton1.BackColor = SystemColors.ButtonFace;
         toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
         toolStripDropDownButton1.DropDownItems.AddRange (new ToolStripItem[] { menuNormalized, menuExpandJson });
         toolStripDropDownButton1.ImageScaling = ToolStripItemImageScaling.None;
         toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
         toolStripDropDownButton1.Margin = new Padding (2, 1, 0, 2);
         toolStripDropDownButton1.Name = "toolStripDropDownButton1";
         toolStripDropDownButton1.Size = new Size (62, 28);
         toolStripDropDownButton1.Text = "Options";
         toolStripDropDownButton1.ToolTipText = "Extra options";
         // 
         // menuNormalized
         // 
         menuNormalized.CheckOnClick = true;
         menuNormalized.Name = "menuNormalized";
         menuNormalized.Size = new Size (187, 22);
         menuNormalized.Text = "Normalized";
         menuNormalized.ToolTipText = "Try normalize (sort) json-keys";
         menuNormalized.CheckStateChanged += menuNormalized_CheckStateChanged;
         // 
         // menuExpandJson
         // 
         menuExpandJson.CheckOnClick = true;
         menuExpandJson.Name = "menuExpandJson";
         menuExpandJson.Size = new Size (187, 22);
         menuExpandJson.Text = "Expand encoded json";
         menuExpandJson.ToolTipText = "Try to expand encoded json in a string";
         menuExpandJson.CheckStateChanged += menuExpandJson_CheckStateChanged;
         // 
         // toolStripSeparator2
         // 
         toolStripSeparator2.Name = "toolStripSeparator2";
         toolStripSeparator2.Size = new Size (6, 31);
         // 
         // toolLabelSearch
         // 
         toolLabelSearch.Margin = new Padding (2, 1, 0, 2);
         toolLabelSearch.Name = "toolLabelSearch";
         toolLabelSearch.Size = new Size (42, 28);
         toolLabelSearch.Text = "Search";
         // 
         // cbSearch
         // 
         cbSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
         cbSearch.AutoCompleteSource = AutoCompleteSource.CustomSource;
         cbSearch.AutoToolTip = true;
         cbSearch.Name = "cbSearch";
         cbSearch.Size = new Size (408, 31);
         cbSearch.KeyUp += txtSearch_KeyUp;
         // 
         // btnSearch
         // 
         btnSearch.DisplayStyle = ToolStripItemDisplayStyle.Image;
         btnSearch.Image = (Image)resources.GetObject ("btnSearch.Image");
         btnSearch.ImageTransparentColor = Color.Magenta;
         btnSearch.Name = "btnSearch";
         btnSearch.Size = new Size (28, 28);
         btnSearch.ToolTipText = "Search";
         btnSearch.Click += btnSearch_Click;
         // 
         // toolStripSeparator1
         // 
         toolStripSeparator1.Name = "toolStripSeparator1";
         toolStripSeparator1.Size = new Size (6, 31);
         // 
         // btnPrev
         // 
         btnPrev.AutoSize = false;
         btnPrev.DisplayStyle = ToolStripItemDisplayStyle.Image;
         btnPrev.Image = (Image)resources.GetObject ("btnPrev.Image");
         btnPrev.ImageScaling = ToolStripItemImageScaling.None;
         btnPrev.ImageTransparentColor = Color.Magenta;
         btnPrev.Margin = new Padding (2, 0, 0, 0);
         btnPrev.Name = "btnPrev";
         btnPrev.Size = new Size (28, 28);
         btnPrev.Text = "Previous line (Ctrl-Up)";
         btnPrev.Click += btnPrev_Click;
         // 
         // btnNext
         // 
         btnNext.AutoSize = false;
         btnNext.DisplayStyle = ToolStripItemDisplayStyle.Image;
         btnNext.Image = (Image)resources.GetObject ("btnNext.Image");
         btnNext.ImageScaling = ToolStripItemImageScaling.None;
         btnNext.ImageTransparentColor = Color.Magenta;
         btnNext.Name = "btnNext";
         btnNext.Size = new Size (28, 28);
         btnNext.Text = "Next line (Ctrl-Down)";
         btnNext.Click += btnNext_Click;
         // 
         // textLine
         // 
         textLine.BorderStyle = BorderStyle.FixedSingle;
         textLine.Font = new Font ("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
         textLine.HideSelection = false;
         textLine.Location = new Point (198, 204);
         textLine.Margin = new Padding (23);
         textLine.Name = "textLine";
         textLine.ReadOnly = true;
         textLine.Size = new Size (571, 103);
         textLine.TabIndex = 6;
         textLine.Text = "boe";
         // 
         // FormLine
         // 
         AutoScaleDimensions = new SizeF (7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size (1183, 613);
         Controls.Add (textLine);
         Controls.Add (toolStrip1);
         Controls.Add (statusStrip1);
         Icon = (Icon)resources.GetObject ("$this.Icon");
         KeyPreview = true;
         Margin = new Padding (2);
         MinimumSize = new Size (470, 347);
         Name = "FormLine";
         ShowInTaskbar = false;
         StartPosition = FormStartPosition.CenterParent;
         Text = "Line";
         FormClosed += FormLine_FormClosed;
         Load += FormLine_Load;
         LocationChanged += FormLine_LocationChanged;
         KeyDown += form_KeyDown;
         KeyPress += form_KeyPress;
         statusStrip1.ResumeLayout (false);
         statusStrip1.PerformLayout ();
         toolStrip1.ResumeLayout (false);
         toolStrip1.PerformLayout ();
         ResumeLayout (false);
         PerformLayout ();
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
      private System.Windows.Forms.ToolStripMenuItem menuExpandJson;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
   }
}