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
        private void InitializeComponent()
        {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
         this.pictureBox2 = new System.Windows.Forms.PictureBox();
         this.btnClose = new System.Windows.Forms.Button();
         this.lblApp = new System.Windows.Forms.Label();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.richTextBox1 = new System.Windows.Forms.RichTextBox();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.SuspendLayout();
         // 
         // pictureBox2
         // 
         this.pictureBox2.BackColor = System.Drawing.Color.White;
         this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Top;
         this.pictureBox2.Location = new System.Drawing.Point(0, 0);
         this.pictureBox2.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
         this.pictureBox2.Name = "pictureBox2";
         this.pictureBox2.Size = new System.Drawing.Size(642, 103);
         this.pictureBox2.TabIndex = 26;
         this.pictureBox2.TabStop = false;
         // 
         // btnClose
         // 
         this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnClose.Location = new System.Drawing.Point(270, 301);
         this.btnClose.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
         this.btnClose.Name = "btnClose";
         this.btnClose.Size = new System.Drawing.Size(84, 25);
         this.btnClose.TabIndex = 23;
         this.btnClose.Text = "Close";
         this.btnClose.UseVisualStyleBackColor = true;
         this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
         // 
         // lblApp
         // 
         this.lblApp.AutoSize = true;
         this.lblApp.BackColor = System.Drawing.Color.White;
         this.lblApp.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
         this.lblApp.Location = new System.Drawing.Point(90, 42);
         this.lblApp.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
         this.lblApp.Name = "lblApp";
         this.lblApp.Size = new System.Drawing.Size(63, 29);
         this.lblApp.TabIndex = 28;
         this.lblApp.Text = "app";
         // 
         // pictureBox1
         // 
         this.pictureBox1.BackColor = System.Drawing.Color.White;
         this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
         this.pictureBox1.Location = new System.Drawing.Point(0, 9);
         this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Padding = new System.Windows.Forms.Padding(5);
         this.pictureBox1.Size = new System.Drawing.Size(86, 94);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
         this.pictureBox1.TabIndex = 27;
         this.pictureBox1.TabStop = false;
         // 
         // richTextBox1
         // 
         this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Top;
         this.richTextBox1.Location = new System.Drawing.Point(0, 103);
         this.richTextBox1.Margin = new System.Windows.Forms.Padding(12);
         this.richTextBox1.Name = "richTextBox1";
         this.richTextBox1.Size = new System.Drawing.Size(642, 171);
         this.richTextBox1.TabIndex = 32;
         this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
         this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
         // 
         // FormAbout
         // 
         this.AcceptButton = this.btnClose;
         this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnClose;
         this.ClientSize = new System.Drawing.Size(642, 347);
         this.Controls.Add(this.richTextBox1);
         this.Controls.Add(this.lblApp);
         this.Controls.Add(this.pictureBox1);
         this.Controls.Add(this.pictureBox2);
         this.Controls.Add(this.btnClose);
         this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Margin = new System.Windows.Forms.Padding(2);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormAbout";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "About";
         this.Load += new System.EventHandler(this.FormAbout_Load);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblApp;
        private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.RichTextBox richTextBox1;
   }
}