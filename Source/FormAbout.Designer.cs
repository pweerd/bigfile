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
    partial class FormAbout
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (FormAbout));
         pictureBox2 = new PictureBox ();
         btnClose = new Button ();
         lblApp = new Label ();
         pictureBox1 = new PictureBox ();
         richTextBox1 = new RichTextBox ();
         ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit ();
         ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit ();
         SuspendLayout ();
         // 
         // pictureBox2
         // 
         pictureBox2.BackColor = Color.White;
         pictureBox2.Dock = DockStyle.Top;
         pictureBox2.Location = new Point (0, 0);
         pictureBox2.Margin = new Padding (2, 1, 2, 1);
         pictureBox2.Name = "pictureBox2";
         pictureBox2.Size = new Size (642, 103);
         pictureBox2.TabIndex = 26;
         pictureBox2.TabStop = false;
         // 
         // btnClose
         // 
         btnClose.DialogResult = DialogResult.Cancel;
         btnClose.Location = new Point (270, 301);
         btnClose.Margin = new Padding (2, 1, 2, 1);
         btnClose.Name = "btnClose";
         btnClose.Size = new Size (84, 25);
         btnClose.TabIndex = 23;
         btnClose.Text = "Close";
         btnClose.UseVisualStyleBackColor = true;
         btnClose.Click += btnClose_Click;
         // 
         // lblApp
         // 
         lblApp.AutoSize = true;
         lblApp.BackColor = Color.White;
         lblApp.Font = new Font ("Verdana", 18F, FontStyle.Bold, GraphicsUnit.Point);
         lblApp.Location = new Point (90, 42);
         lblApp.Margin = new Padding (2, 0, 2, 0);
         lblApp.Name = "lblApp";
         lblApp.Size = new Size (63, 29);
         lblApp.TabIndex = 28;
         lblApp.Text = "app";
         // 
         // pictureBox1
         // 
         pictureBox1.BackColor = Color.White;
         pictureBox1.Image = (Image)resources.GetObject ("pictureBox1.Image");
         pictureBox1.Location = new Point (0, 9);
         pictureBox1.Margin = new Padding (2, 1, 2, 1);
         pictureBox1.Name = "pictureBox1";
         pictureBox1.Padding = new Padding (5);
         pictureBox1.Size = new Size (86, 94);
         pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
         pictureBox1.TabIndex = 27;
         pictureBox1.TabStop = false;
         // 
         // richTextBox1
         // 
         richTextBox1.Dock = DockStyle.Top;
         richTextBox1.Location = new Point (0, 103);
         richTextBox1.Margin = new Padding (12);
         richTextBox1.Name = "richTextBox1";
         richTextBox1.Size = new Size (642, 171);
         richTextBox1.TabIndex = 32;
         richTextBox1.Text = resources.GetString ("richTextBox1.Text");
         richTextBox1.LinkClicked += richTextBox1_LinkClicked;
         // 
         // FormAbout
         // 
         AcceptButton = btnClose;
         AutoScaleDimensions = new SizeF (7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = btnClose;
         ClientSize = new Size (642, 347);
         Controls.Add (richTextBox1);
         Controls.Add (lblApp);
         Controls.Add (pictureBox1);
         Controls.Add (pictureBox2);
         Controls.Add (btnClose);
         Font = new Font ("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point);
         FormBorderStyle = FormBorderStyle.FixedSingle;
         Icon = (Icon)resources.GetObject ("$this.Icon");
         Margin = new Padding (2);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FormAbout";
         ShowInTaskbar = false;
         StartPosition = FormStartPosition.CenterParent;
         Text = "About";
         Load += FormAbout_Load;
         ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit ();
         ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit ();
         ResumeLayout (false);
         PerformLayout ();
      }

      #endregion

      private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblApp;
        private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.RichTextBox richTextBox1;
   }
}