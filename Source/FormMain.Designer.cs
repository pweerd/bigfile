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
   partial class FormMain
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
         this.menuStrip = new System.Windows.Forms.MenuStrip();
         this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
         this.menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
         this.menuRecentFiles = new System.Windows.Forms.ToolStripMenuItem();
         this.menuRecentFolders = new System.Windows.Forms.ToolStripMenuItem();
         this.menuFileSep1 = new System.Windows.Forms.ToolStripSeparator();
         this.menuFileClose = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
         this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
         this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
         this.gotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exprtAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportMatchedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
         this.menuToolsConfiguration = new System.Windows.Forms.ToolStripMenuItem();
         this.menuView = new System.Windows.Forms.ToolStripMenuItem();
         this.menuViewAll = new System.Windows.Forms.ToolStripMenuItem();
         this.menuViewMatched = new System.Windows.Forms.ToolStripMenuItem();
         this.menuViewUnmatched = new System.Windows.Forms.ToolStripMenuItem();
         this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
         this.menuHelpHelp = new System.Windows.Forms.ToolStripMenuItem();
         this.menuHelpSep1 = new System.Windows.Forms.ToolStripSeparator();
         this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
         this.statusStrip = new System.Windows.Forms.StatusStrip();
         this.statusProgress = new System.Windows.Forms.ToolStripProgressBar();
         this.statusLabelMain = new System.Windows.Forms.ToolStripStatusLabel();
         this.statusSep1 = new System.Windows.Forms.ToolStripStatusLabel();
         this.statusLabelSearch = new System.Windows.Forms.ToolStripStatusLabel();
         this.toolStrip = new System.Windows.Forms.ToolStrip();
         this.btnWarning = new System.Windows.Forms.ToolStripButton();
         this.toolLabelSearch = new System.Windows.Forms.ToolStripLabel();
         this.cbSearch = new System.Windows.Forms.ToolStripComboBox();
         this.btnSearch = new System.Windows.Forms.ToolStripButton();
         this.btnResetSearch = new System.Windows.Forms.ToolStripButton();
         this.dropdownEncoding = new System.Windows.Forms.ToolStripComboBox();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
         this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.cbZipEntries = new System.Windows.Forms.ToolStripComboBox();
         this.panelMain = new System.Windows.Forms.Panel();
         this.listLines = new BrightIdeasSoftware.VirtualObjectListView();
         this.olvcLineNumber = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
         this.olvcText = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
         this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.contextMenuCopy = new System.Windows.Forms.ToolStripMenuItem();
         this.menuStrip.SuspendLayout();
         this.statusStrip.SuspendLayout();
         this.toolStrip.SuspendLayout();
         this.panelMain.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.listLines)).BeginInit();
         this.contextMenu.SuspendLayout();
         this.SuspendLayout();
         // 
         // menuStrip
         // 
         this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
         this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuTools,
            this.menuView,
            this.menuHelp});
         this.menuStrip.Location = new System.Drawing.Point(0, 0);
         this.menuStrip.Name = "menuStrip";
         this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
         this.menuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
         this.menuStrip.Size = new System.Drawing.Size(978, 24);
         this.menuStrip.TabIndex = 1;
         // 
         // menuFile
         // 
         this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileOpen,
            this.menuRecentFiles,
            this.menuRecentFolders,
            this.menuFileSep1,
            this.menuFileClose,
            this.toolStripMenuItem3,
            this.menuFileExit});
         this.menuFile.Name = "menuFile";
         this.menuFile.Size = new System.Drawing.Size(37, 22);
         this.menuFile.Text = "&File";
         // 
         // menuFileOpen
         // 
         this.menuFileOpen.Name = "menuFileOpen";
         this.menuFileOpen.Size = new System.Drawing.Size(149, 22);
         this.menuFileOpen.Text = "&Open";
         this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
         // 
         // menuRecentFiles
         // 
         this.menuRecentFiles.Name = "menuRecentFiles";
         this.menuRecentFiles.Size = new System.Drawing.Size(149, 22);
         this.menuRecentFiles.Text = "Recent files";
         // 
         // menuRecentFolders
         // 
         this.menuRecentFolders.Name = "menuRecentFolders";
         this.menuRecentFolders.Size = new System.Drawing.Size(149, 22);
         this.menuRecentFolders.Text = "Recent folders";
         // 
         // menuFileSep1
         // 
         this.menuFileSep1.Name = "menuFileSep1";
         this.menuFileSep1.Size = new System.Drawing.Size(146, 6);
         // 
         // menuFileClose
         // 
         this.menuFileClose.Name = "menuFileClose";
         this.menuFileClose.Size = new System.Drawing.Size(149, 22);
         this.menuFileClose.Text = "Close";
         this.menuFileClose.Click += new System.EventHandler(this.menuFileClose_Click);
         // 
         // toolStripMenuItem3
         // 
         this.toolStripMenuItem3.Name = "toolStripMenuItem3";
         this.toolStripMenuItem3.Size = new System.Drawing.Size(146, 6);
         // 
         // menuFileExit
         // 
         this.menuFileExit.Name = "menuFileExit";
         this.menuFileExit.Size = new System.Drawing.Size(149, 22);
         this.menuFileExit.Text = "&Exit";
         this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
         // 
         // menuTools
         // 
         this.menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gotoToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripMenuItem2,
            this.menuToolsConfiguration});
         this.menuTools.Name = "menuTools";
         this.menuTools.Size = new System.Drawing.Size(47, 22);
         this.menuTools.Text = "Tools";
         // 
         // gotoToolStripMenuItem
         // 
         this.gotoToolStripMenuItem.Name = "gotoToolStripMenuItem";
         this.gotoToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
         this.gotoToolStripMenuItem.Text = "Goto";
         this.gotoToolStripMenuItem.Click += new System.EventHandler(this.gotoToolStripMenuItem_Click);
         // 
         // exportToolStripMenuItem
         // 
         this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exprtAllToolStripMenuItem,
            this.exportSelectedToolStripMenuItem,
            this.exportMatchedToolStripMenuItem});
         this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
         this.exportToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
         this.exportToolStripMenuItem.Text = "Export";
         // 
         // exprtAllToolStripMenuItem
         // 
         this.exprtAllToolStripMenuItem.Name = "exprtAllToolStripMenuItem";
         this.exprtAllToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
         this.exprtAllToolStripMenuItem.Text = "All";
         // 
         // exportSelectedToolStripMenuItem
         // 
         this.exportSelectedToolStripMenuItem.Name = "exportSelectedToolStripMenuItem";
         this.exportSelectedToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
         this.exportSelectedToolStripMenuItem.Text = "Selected";
         // 
         // exportMatchedToolStripMenuItem
         // 
         this.exportMatchedToolStripMenuItem.Name = "exportMatchedToolStripMenuItem";
         this.exportMatchedToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
         this.exportMatchedToolStripMenuItem.Text = "Matched";
         // 
         // toolStripMenuItem2
         // 
         this.toolStripMenuItem2.Name = "toolStripMenuItem2";
         this.toolStripMenuItem2.Size = new System.Drawing.Size(113, 6);
         // 
         // menuToolsConfiguration
         // 
         this.menuToolsConfiguration.Name = "menuToolsConfiguration";
         this.menuToolsConfiguration.Size = new System.Drawing.Size(116, 22);
         this.menuToolsConfiguration.Text = "Settings";
         this.menuToolsConfiguration.Click += new System.EventHandler(this.menuToolsConfiguration_Click);
         // 
         // menuView
         // 
         this.menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuViewAll,
            this.menuViewMatched,
            this.menuViewUnmatched});
         this.menuView.Name = "menuView";
         this.menuView.Size = new System.Drawing.Size(44, 22);
         this.menuView.Text = "View";
         // 
         // menuViewAll
         // 
         this.menuViewAll.Checked = true;
         this.menuViewAll.CheckState = System.Windows.Forms.CheckState.Checked;
         this.menuViewAll.Name = "menuViewAll";
         this.menuViewAll.Size = new System.Drawing.Size(136, 22);
         this.menuViewAll.Text = "View all";
         this.menuViewAll.Click += new System.EventHandler(this.menuView_Click);
         // 
         // menuViewMatched
         // 
         this.menuViewMatched.Name = "menuViewMatched";
         this.menuViewMatched.Size = new System.Drawing.Size(136, 22);
         this.menuViewMatched.Text = "Matched";
         this.menuViewMatched.Click += new System.EventHandler(this.menuView_Click);
         // 
         // menuViewUnmatched
         // 
         this.menuViewUnmatched.Name = "menuViewUnmatched";
         this.menuViewUnmatched.Size = new System.Drawing.Size(136, 22);
         this.menuViewUnmatched.Text = "Unmatched";
         this.menuViewUnmatched.Click += new System.EventHandler(this.menuView_Click);
         // 
         // menuHelp
         // 
         this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHelpHelp,
            this.menuHelpSep1,
            this.menuHelpAbout});
         this.menuHelp.Name = "menuHelp";
         this.menuHelp.Size = new System.Drawing.Size(44, 22);
         this.menuHelp.Text = "&Help";
         // 
         // menuHelpHelp
         // 
         this.menuHelpHelp.Name = "menuHelpHelp";
         this.menuHelpHelp.Size = new System.Drawing.Size(107, 22);
         this.menuHelpHelp.Text = "&Help";
         this.menuHelpHelp.Click += new System.EventHandler(this.menuHelpHelp_Click);
         // 
         // menuHelpSep1
         // 
         this.menuHelpSep1.Name = "menuHelpSep1";
         this.menuHelpSep1.Size = new System.Drawing.Size(104, 6);
         // 
         // menuHelpAbout
         // 
         this.menuHelpAbout.Name = "menuHelpAbout";
         this.menuHelpAbout.Size = new System.Drawing.Size(107, 22);
         this.menuHelpAbout.Text = "&About";
         this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
         // 
         // statusStrip
         // 
         this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
         this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusProgress,
            this.statusLabelMain,
            this.statusSep1,
            this.statusLabelSearch});
         this.statusStrip.Location = new System.Drawing.Point(0, 539);
         this.statusStrip.Name = "statusStrip";
         this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
         this.statusStrip.Size = new System.Drawing.Size(978, 22);
         this.statusStrip.TabIndex = 2;
         this.statusStrip.Text = "statusStrip1";
         // 
         // statusProgress
         // 
         this.statusProgress.Name = "statusProgress";
         this.statusProgress.Size = new System.Drawing.Size(67, 16);
         this.statusProgress.Click += new System.EventHandler(this.statusProgress_Click);
         // 
         // statusLabelMain
         // 
         this.statusLabelMain.Name = "statusLabelMain";
         this.statusLabelMain.Size = new System.Drawing.Size(0, 17);
         // 
         // statusSep1
         // 
         this.statusSep1.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
         this.statusSep1.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
         this.statusSep1.Name = "statusSep1";
         this.statusSep1.Size = new System.Drawing.Size(4, 17);
         // 
         // statusLabelSearch
         // 
         this.statusLabelSearch.Name = "statusLabelSearch";
         this.statusLabelSearch.Size = new System.Drawing.Size(0, 17);
         // 
         // toolStrip
         // 
         this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
         this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnWarning,
            this.toolLabelSearch,
            this.cbSearch,
            this.btnSearch,
            this.btnResetSearch,
            this.dropdownEncoding,
            this.toolStripSeparator1,
            this.toolStripButton2,
            this.toolStripSeparator2,
            this.cbZipEntries});
         this.toolStrip.Location = new System.Drawing.Point(0, 24);
         this.toolStrip.Name = "toolStrip";
         this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
         this.toolStrip.Size = new System.Drawing.Size(978, 31);
         this.toolStrip.TabIndex = 4;
         this.toolStrip.Text = "toolStrip1";
         // 
         // btnWarning
         // 
         this.btnWarning.AutoToolTip = false;
         this.btnWarning.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.btnWarning.Image = ((System.Drawing.Image)(resources.GetObject("btnWarning.Image")));
         this.btnWarning.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.btnWarning.Name = "btnWarning";
         this.btnWarning.Size = new System.Drawing.Size(28, 28);
         this.btnWarning.Text = "toolStripButton1";
         this.btnWarning.Click += new System.EventHandler(this.btnWarning_Click);
         // 
         // toolLabelSearch
         // 
         this.toolLabelSearch.Name = "toolLabelSearch";
         this.toolLabelSearch.Size = new System.Drawing.Size(42, 28);
         this.toolLabelSearch.Text = "Search";
         // 
         // cbSearch
         // 
         this.cbSearch.AutoCompleteCustomSource.AddRange(new string[] {
            "aap",
            "noot",
            "mies"});
         this.cbSearch.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
         this.cbSearch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
         this.cbSearch.AutoToolTip = true;
         this.cbSearch.Name = "cbSearch";
         this.cbSearch.Size = new System.Drawing.Size(350, 31);
         this.cbSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbSearch_KeyPress);
         this.cbSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cbSearch_KeyUp);
         // 
         // btnSearch
         // 
         this.btnSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.btnSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.Image")));
         this.btnSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.btnSearch.Name = "btnSearch";
         this.btnSearch.Size = new System.Drawing.Size(28, 28);
         this.btnSearch.ToolTipText = "Search";
         this.btnSearch.Click += new System.EventHandler(this.toolButtonSearch_Click);
         // 
         // btnResetSearch
         // 
         this.btnResetSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.btnResetSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnResetSearch.Image")));
         this.btnResetSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.btnResetSearch.Name = "btnResetSearch";
         this.btnResetSearch.Size = new System.Drawing.Size(28, 28);
         this.btnResetSearch.Text = "toolStripButton3";
         this.btnResetSearch.Click += new System.EventHandler(this.btnResetSearch_Click);
         // 
         // dropdownEncoding
         // 
         this.dropdownEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.dropdownEncoding.Items.AddRange(new object[] {
            "Utf8",
            "Utf16LE",
            "Utf16BE",
            "Windows"});
         this.dropdownEncoding.Name = "dropdownEncoding";
         this.dropdownEncoding.Size = new System.Drawing.Size(158, 31);
         this.dropdownEncoding.SelectedIndexChanged += new System.EventHandler(this.dropdownEncoding_SelectedIndexChanged);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
         // 
         // toolStripButton2
         // 
         this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
         this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.toolStripButton2.Name = "toolStripButton2";
         this.toolStripButton2.Size = new System.Drawing.Size(28, 28);
         this.toolStripButton2.Text = "toolStripButton2";
         this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
         // 
         // cbZipEntries
         // 
         this.cbZipEntries.DropDownWidth = 300;
         this.cbZipEntries.Name = "cbZipEntries";
         this.cbZipEntries.Size = new System.Drawing.Size(158, 31);
         this.cbZipEntries.SelectedIndexChanged += new System.EventHandler(this.cbZipEntries_SelectedIndexChanged);
         // 
         // panelMain
         // 
         this.panelMain.Controls.Add(this.listLines);
         this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panelMain.Location = new System.Drawing.Point(0, 55);
         this.panelMain.Margin = new System.Windows.Forms.Padding(2);
         this.panelMain.Name = "panelMain";
         this.panelMain.Size = new System.Drawing.Size(978, 484);
         this.panelMain.TabIndex = 5;
         // 
         // listLines
         // 
         this.listLines.AllColumns.Add(this.olvcLineNumber);
         this.listLines.AllColumns.Add(this.olvcText);
         this.listLines.AllowDrop = true;
         this.listLines.AutoArrange = false;
         this.listLines.CausesValidation = false;
         this.listLines.CellEditUseWholeCell = false;
         this.listLines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvcLineNumber,
            this.olvcText});
         this.listLines.ContextMenuStrip = this.contextMenu;
         this.listLines.Cursor = System.Windows.Forms.Cursors.Default;
         this.listLines.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.listLines.FullRowSelect = true;
         this.listLines.GridLines = true;
         this.listLines.HasCollapsibleGroups = false;
         this.listLines.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
         this.listLines.HideSelection = false;
         this.listLines.IsSearchOnSortColumn = false;
         this.listLines.Location = new System.Drawing.Point(0, 0);
         this.listLines.Margin = new System.Windows.Forms.Padding(2);
         this.listLines.Name = "listLines";
         this.listLines.SelectColumnsMenuStaysOpen = false;
         this.listLines.SelectColumnsOnRightClick = false;
         this.listLines.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.None;
         this.listLines.ShowFilterMenuOnRightClick = false;
         this.listLines.ShowGroups = false;
         this.listLines.ShowSortIndicators = false;
         this.listLines.Size = new System.Drawing.Size(897, 67);
         this.listLines.TabIndex = 0;
         this.listLines.TriggerCellOverEventsWhenOverHeader = false;
         this.listLines.UseCompatibleStateImageBehavior = false;
         this.listLines.View = System.Windows.Forms.View.Details;
         this.listLines.VirtualMode = true;
         this.listLines.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.listLines_FormatRow);
         this.listLines.Scroll += new System.EventHandler<System.Windows.Forms.ScrollEventArgs>(this.listLines_Scroll);
         this.listLines.ItemActivate += new System.EventHandler(this.listLines_ItemActivate);
         this.listLines.DragDrop += new System.Windows.Forms.DragEventHandler(this.listLines_DragDrop);
         this.listLines.DragEnter += new System.Windows.Forms.DragEventHandler(this.listLines_DragEnter);
         this.listLines.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormMain_KeyPress);
         this.listLines.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyUp);
         this.listLines.Resize += new System.EventHandler(this.listLines_Resize);
         // 
         // olvcLineNumber
         // 
         this.olvcLineNumber.Searchable = false;
         this.olvcLineNumber.Sortable = false;
         this.olvcLineNumber.Text = "Line No.";
         this.olvcLineNumber.UseFiltering = false;
         this.olvcLineNumber.Width = 95;
         // 
         // olvcText
         // 
         this.olvcText.Searchable = false;
         this.olvcText.Sortable = false;
         this.olvcText.Text = "Data";
         this.olvcText.UseFiltering = false;
         // 
         // contextMenu
         // 
         this.contextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
         this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuCopy});
         this.contextMenu.Name = "contextMenu";
         this.contextMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
         this.contextMenu.Size = new System.Drawing.Size(103, 26);
         this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
         // 
         // contextMenuCopy
         // 
         this.contextMenuCopy.Name = "contextMenuCopy";
         this.contextMenuCopy.Size = new System.Drawing.Size(102, 22);
         this.contextMenuCopy.Text = "Copy";
         this.contextMenuCopy.Click += new System.EventHandler(this.contextMenuCopy_Click);
         // 
         // FormMain
         // 
         this.AllowDrop = true;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(978, 561);
         this.Controls.Add(this.panelMain);
         this.Controls.Add(this.toolStrip);
         this.Controls.Add(this.statusStrip);
         this.Controls.Add(this.menuStrip);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MainMenuStrip = this.menuStrip;
         this.Margin = new System.Windows.Forms.Padding(2);
         this.Name = "FormMain";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
         this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
         this.Load += new System.EventHandler(this.FormMain_Load);
         this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormMain_DragDrop);
         this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormMain_DragEnter);
         this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormMain_KeyPress);
         this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyUp);
         this.menuStrip.ResumeLayout(false);
         this.menuStrip.PerformLayout();
         this.statusStrip.ResumeLayout(false);
         this.statusStrip.PerformLayout();
         this.toolStrip.ResumeLayout(false);
         this.toolStrip.PerformLayout();
         this.panelMain.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.listLines)).EndInit();
         this.contextMenu.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion
      private System.Windows.Forms.MenuStrip menuStrip;
      private System.Windows.Forms.ToolStripMenuItem menuFile;
      private System.Windows.Forms.StatusStrip statusStrip;
      private System.Windows.Forms.ToolStripStatusLabel statusLabelMain;
      private System.Windows.Forms.ToolStrip toolStrip;
      private System.Windows.Forms.ToolStripLabel toolLabelSearch;
      private System.Windows.Forms.ToolStripComboBox cbSearch;
      private System.Windows.Forms.ToolStripButton btnSearch;
      private System.Windows.Forms.Panel panelMain;
      private System.Windows.Forms.ToolStripMenuItem menuHelp;
      private System.Windows.Forms.ToolStripMenuItem menuHelpHelp;
      private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;
      private System.Windows.Forms.ToolStripMenuItem menuFileOpen;
      private System.Windows.Forms.ToolStripMenuItem menuRecentFiles;
      private System.Windows.Forms.ToolStripMenuItem menuRecentFolders;
      private System.Windows.Forms.ToolStripSeparator menuFileSep1;
      private System.Windows.Forms.ToolStripMenuItem menuFileExit;
      private System.Windows.Forms.ToolStripProgressBar statusProgress;
      private System.Windows.Forms.ToolStripStatusLabel statusSep1;
      private System.Windows.Forms.ToolStripSeparator menuHelpSep1;
      private System.Windows.Forms.ContextMenuStrip contextMenu;
      private System.Windows.Forms.ToolStripComboBox dropdownEncoding;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripStatusLabel statusLabelSearch;
      private System.Windows.Forms.ToolStripMenuItem contextMenuCopy;
      private System.Windows.Forms.ToolStripMenuItem menuTools;
      private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
      private System.Windows.Forms.ToolStripMenuItem menuToolsConfiguration;
      private BrightIdeasSoftware.VirtualObjectListView listLines;
      private BrightIdeasSoftware.OLVColumn olvcLineNumber;
      private BrightIdeasSoftware.OLVColumn olvcText;
      private System.Windows.Forms.ToolStripMenuItem menuFileClose;
      private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
      private System.Windows.Forms.ToolStripMenuItem menuView;
      private System.Windows.Forms.ToolStripMenuItem menuViewAll;
      private System.Windows.Forms.ToolStripMenuItem menuViewMatched;
      private System.Windows.Forms.ToolStripMenuItem menuViewUnmatched;
      private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exprtAllToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportSelectedToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportMatchedToolStripMenuItem;
      private System.Windows.Forms.ToolStripButton toolStripButton2;
      private System.Windows.Forms.ToolStripButton btnWarning;
      private System.Windows.Forms.ToolStripButton btnResetSearch;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripComboBox cbZipEntries;
   }
}

