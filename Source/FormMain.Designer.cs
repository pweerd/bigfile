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

using System.ComponentModel;

namespace Bitmanager.BigFile {
   partial class FormMain {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose (bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose ();
         }
         base.Dispose (disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent () {
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
         this.menuFindMultis = new System.Windows.Forms.ToolStripMenuItem();
         this.menuFindSingles = new System.Windows.Forms.ToolStripMenuItem();
         this.menuResetMatched = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
         this.gotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.exportMatchedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.clearAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toggleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.matchedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.nonMatchedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.clearByMatchedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.clearByNonMatchedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripCopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
         this.menuToolsConfiguration = new System.Windows.Forms.ToolStripMenuItem();
         this.registerShellextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
         this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.cbEncoding = new System.Windows.Forms.ToolStripComboBox();
         this.cbFontSize = new System.Windows.Forms.ToolStripComboBox();
         this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
         this.cbZipEntries = new System.Windows.Forms.ToolStripComboBox();
         this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
         this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
         this.cbSplit = new System.Windows.Forms.ToolStripComboBox();
         this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
         this.cbLoadLimits = new System.Windows.Forms.ToolStripComboBox();
         this.btnResplit = new System.Windows.Forms.ToolStripButton();
         this.cbZipEngine = new System.Windows.Forms.ToolStripComboBox();
         this.panelMain = new System.Windows.Forms.Panel();
         this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.contextMenuCopy = new System.Windows.Forms.ToolStripMenuItem();
         this.menuStrip.SuspendLayout();
         this.statusStrip.SuspendLayout();
         this.toolStrip.SuspendLayout();
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
         this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 1, 0, 1);
         this.menuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
         this.menuStrip.Size = new System.Drawing.Size(1300, 24);
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
            this.menuFindMultis,
            this.menuFindSingles,
            this.menuResetMatched,
            this.toolStripSeparator5,
            this.gotoToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.selectToolStripMenuItem,
            this.toolStripCopyMenuItem,
            this.toolStripMenuItem2,
            this.menuToolsConfiguration,
            this.registerShellextToolStripMenuItem});
         this.menuTools.Name = "menuTools";
         this.menuTools.Size = new System.Drawing.Size(46, 22);
         this.menuTools.Text = "Tools";
         // 
         // menuFindMultis
         // 
         this.menuFindMultis.Name = "menuFindMultis";
         this.menuFindMultis.Size = new System.Drawing.Size(159, 22);
         this.menuFindMultis.Text = "Find multi lines";
         this.menuFindMultis.Click += new System.EventHandler(this.menuFindMultis_Click);
         // 
         // menuFindSingles
         // 
         this.menuFindSingles.Name = "menuFindSingles";
         this.menuFindSingles.Size = new System.Drawing.Size(159, 22);
         this.menuFindSingles.Text = "Find single lines";
         this.menuFindSingles.Click += new System.EventHandler(this.menuFindSingles_Click);
         // 
         // menuResetMatched
         // 
         this.menuResetMatched.Name = "menuResetMatched";
         this.menuResetMatched.Size = new System.Drawing.Size(159, 22);
         this.menuResetMatched.Text = "Reset matched";
         this.menuResetMatched.Click += new System.EventHandler(this.menuResetMatched_Click);
         // 
         // toolStripSeparator5
         // 
         this.toolStripSeparator5.Name = "toolStripSeparator5";
         this.toolStripSeparator5.Size = new System.Drawing.Size(156, 6);
         // 
         // gotoToolStripMenuItem
         // 
         this.gotoToolStripMenuItem.Name = "gotoToolStripMenuItem";
         this.gotoToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
         this.gotoToolStripMenuItem.Text = "Goto (Ctrl-G)";
         this.gotoToolStripMenuItem.Click += new System.EventHandler(this.gotoToolStripMenuItem_Click);
         // 
         // exportToolStripMenuItem
         // 
         this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAllToolStripMenuItem,
            this.exportSelectedToolStripMenuItem,
            this.exportMatchedToolStripMenuItem});
         this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
         this.exportToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
         this.exportToolStripMenuItem.Text = "Export";
         // 
         // exportAllToolStripMenuItem
         // 
         this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
         this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
         this.exportAllToolStripMenuItem.Text = "All";
         this.exportAllToolStripMenuItem.Click += new System.EventHandler(this.exportAllToolStripMenuItem_Click);
         // 
         // exportSelectedToolStripMenuItem
         // 
         this.exportSelectedToolStripMenuItem.Name = "exportSelectedToolStripMenuItem";
         this.exportSelectedToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
         this.exportSelectedToolStripMenuItem.Text = "Selected";
         this.exportSelectedToolStripMenuItem.Click += new System.EventHandler(this.exportSelectedToolStripMenuItem_Click);
         // 
         // exportMatchedToolStripMenuItem
         // 
         this.exportMatchedToolStripMenuItem.Name = "exportMatchedToolStripMenuItem";
         this.exportMatchedToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
         this.exportMatchedToolStripMenuItem.Text = "Matched";
         this.exportMatchedToolStripMenuItem.Click += new System.EventHandler(this.exportMatchedToolStripMenuItem_Click);
         // 
         // selectToolStripMenuItem
         // 
         this.selectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem,
            this.clearAllToolStripMenuItem,
            this.toggleToolStripMenuItem,
            this.toolStripSeparator1,
            this.matchedToolStripMenuItem,
            this.nonMatchedToolStripMenuItem,
            this.clearByMatchedToolStripMenuItem,
            this.clearByNonMatchedToolStripMenuItem});
         this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
         this.selectToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
         this.selectToolStripMenuItem.Text = "Select";
         // 
         // allToolStripMenuItem
         // 
         this.allToolStripMenuItem.Name = "allToolStripMenuItem";
         this.allToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
         this.allToolStripMenuItem.Text = "All";
         this.allToolStripMenuItem.Click += new System.EventHandler(this.allToolStripMenuItem_Click);
         // 
         // clearAllToolStripMenuItem
         // 
         this.clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
         this.clearAllToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
         this.clearAllToolStripMenuItem.Text = "Clear all";
         this.clearAllToolStripMenuItem.Click += new System.EventHandler(this.clearAllToolStripMenuItem_Click);
         // 
         // toggleToolStripMenuItem
         // 
         this.toggleToolStripMenuItem.Name = "toggleToolStripMenuItem";
         this.toggleToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
         this.toggleToolStripMenuItem.Text = "Toggle";
         this.toggleToolStripMenuItem.Click += new System.EventHandler(this.toggleToolStripMenuItem_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
         // 
         // matchedToolStripMenuItem
         // 
         this.matchedToolStripMenuItem.Name = "matchedToolStripMenuItem";
         this.matchedToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
         this.matchedToolStripMenuItem.Text = "By matched";
         this.matchedToolStripMenuItem.Click += new System.EventHandler(this.matchedToolStripMenuItem_Click);
         // 
         // nonMatchedToolStripMenuItem
         // 
         this.nonMatchedToolStripMenuItem.Name = "nonMatchedToolStripMenuItem";
         this.nonMatchedToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
         this.nonMatchedToolStripMenuItem.Text = "By non matched";
         this.nonMatchedToolStripMenuItem.Click += new System.EventHandler(this.nonMatchedToolStripMenuItem_Click);
         // 
         // clearByMatchedToolStripMenuItem
         // 
         this.clearByMatchedToolStripMenuItem.Name = "clearByMatchedToolStripMenuItem";
         this.clearByMatchedToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
         this.clearByMatchedToolStripMenuItem.Text = "Clear by matched";
         this.clearByMatchedToolStripMenuItem.Click += new System.EventHandler(this.clearByMatchedToolStripMenuItem_Click);
         // 
         // clearByNonMatchedToolStripMenuItem
         // 
         this.clearByNonMatchedToolStripMenuItem.Name = "clearByNonMatchedToolStripMenuItem";
         this.clearByNonMatchedToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
         this.clearByNonMatchedToolStripMenuItem.Text = "Clear by non matched";
         this.clearByNonMatchedToolStripMenuItem.Click += new System.EventHandler(this.clearByNonMatchedToolStripMenuItem_Click);
         // 
         // toolStripCopyMenuItem
         // 
         this.toolStripCopyMenuItem.Name = "toolStripCopyMenuItem";
         this.toolStripCopyMenuItem.Size = new System.Drawing.Size(159, 22);
         this.toolStripCopyMenuItem.Text = "Copy (Ctrl-C)";
         this.toolStripCopyMenuItem.Click += new System.EventHandler(this.toolStripCopyMenuItem_Click);
         // 
         // toolStripMenuItem2
         // 
         this.toolStripMenuItem2.Name = "toolStripMenuItem2";
         this.toolStripMenuItem2.Size = new System.Drawing.Size(156, 6);
         // 
         // menuToolsConfiguration
         // 
         this.menuToolsConfiguration.Name = "menuToolsConfiguration";
         this.menuToolsConfiguration.Size = new System.Drawing.Size(159, 22);
         this.menuToolsConfiguration.Text = "Options";
         this.menuToolsConfiguration.Click += new System.EventHandler(this.menuToolsConfiguration_Click);
         // 
         // registerShellextToolStripMenuItem
         // 
         this.registerShellextToolStripMenuItem.Name = "registerShellextToolStripMenuItem";
         this.registerShellextToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
         this.registerShellextToolStripMenuItem.Text = "Register shellext";
         this.registerShellextToolStripMenuItem.Click += new System.EventHandler(this.registerShellextToolStripMenuItem_Click);
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
         this.statusStrip.Location = new System.Drawing.Point(0, 623);
         this.statusStrip.Name = "statusStrip";
         this.statusStrip.Size = new System.Drawing.Size(1300, 24);
         this.statusStrip.TabIndex = 2;
         this.statusStrip.Text = "statusStrip1";
         // 
         // statusProgress
         // 
         this.statusProgress.Name = "statusProgress";
         this.statusProgress.Size = new System.Drawing.Size(78, 18);
         this.statusProgress.Click += new System.EventHandler(this.statusProgress_Click);
         // 
         // statusLabelMain
         // 
         this.statusLabelMain.Name = "statusLabelMain";
         this.statusLabelMain.Size = new System.Drawing.Size(0, 19);
         // 
         // statusSep1
         // 
         this.statusSep1.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
         this.statusSep1.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
         this.statusSep1.Name = "statusSep1";
         this.statusSep1.Size = new System.Drawing.Size(4, 19);
         // 
         // statusLabelSearch
         // 
         this.statusLabelSearch.Name = "statusLabelSearch";
         this.statusLabelSearch.Size = new System.Drawing.Size(0, 19);
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
            this.toolStripSeparator2,
            this.cbEncoding,
            this.cbFontSize,
            this.toolStripSeparator3,
            this.cbZipEntries,
            this.toolStripSeparator4,
            this.toolStripLabel1,
            this.cbSplit,
            this.toolStripLabel2,
            this.cbLoadLimits,
            this.btnResplit,
            this.cbZipEngine});
         this.toolStrip.Location = new System.Drawing.Point(0, 24);
         this.toolStrip.Name = "toolStrip";
         this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
         this.toolStrip.Size = new System.Drawing.Size(1300, 31);
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
         this.btnWarning.ToolTipText = "Important warnings detected. Click to view.";
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
         this.cbSearch.AutoToolTip = true;
         this.cbSearch.Name = "cbSearch";
         this.cbSearch.Size = new System.Drawing.Size(408, 31);
         this.cbSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbSearch_KeyPress);
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
         this.btnResetSearch.ToolTipText = "Reset all searches";
         this.btnResetSearch.Click += new System.EventHandler(this.btnResetSearch_Click);
         // 
         // toolStripSeparator2
         // 
         this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
         this.toolStripSeparator2.Name = "toolStripSeparator2";
         this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
         // 
         // cbEncoding
         // 
         this.cbEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbEncoding.Items.AddRange(new object[] {
            "Utf8",
            "Utf16LE",
            "Utf16BE",
            "Windows"});
         this.cbEncoding.Name = "cbEncoding";
         this.cbEncoding.Size = new System.Drawing.Size(80, 31);
         this.cbEncoding.ToolTipText = "Codepage selection";
         this.cbEncoding.SelectedIndexChanged += new System.EventHandler(this.dropdownEncoding_SelectedIndexChanged);
         // 
         // cbFontSize
         // 
         this.cbFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbFontSize.Name = "cbFontSize";
         this.cbFontSize.Size = new System.Drawing.Size(75, 31);
         this.cbFontSize.ToolTipText = "Modify fontsize";
         // 
         // toolStripSeparator3
         // 
         this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
         this.toolStripSeparator3.Name = "toolStripSeparator3";
         this.toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
         // 
         // cbZipEntries
         // 
         this.cbZipEntries.Name = "cbZipEntries";
         this.cbZipEntries.Size = new System.Drawing.Size(184, 31);
         this.cbZipEntries.ToolTipText = "Select entry in the zip-file";
         this.cbZipEntries.SelectedIndexChanged += new System.EventHandler(this.cbZipEntries_SelectedIndexChanged);
         // 
         // toolStripSeparator4
         // 
         this.toolStripSeparator4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
         this.toolStripSeparator4.Name = "toolStripSeparator4";
         this.toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
         // 
         // toolStripLabel1
         // 
         this.toolStripLabel1.Name = "toolStripLabel1";
         this.toolStripLabel1.Size = new System.Drawing.Size(65, 28);
         this.toolStripLabel1.Text = "Linesplitter";
         // 
         // cbSplit
         // 
         this.cbSplit.Name = "cbSplit";
         this.cbSplit.Size = new System.Drawing.Size(75, 31);
         this.cbSplit.Text = "2048";
         this.cbSplit.ToolTipText = "Automatic split lines larger than this value";
         // 
         // toolStripLabel2
         // 
         this.toolStripLabel2.Name = "toolStripLabel2";
         this.toolStripLabel2.Size = new System.Drawing.Size(65, 28);
         this.toolStripLabel2.Text = "Load limits";
         // 
         // cbLoadLimits
         // 
         this.cbLoadLimits.Name = "cbLoadLimits";
         this.cbLoadLimits.Size = new System.Drawing.Size(100, 31);
         // 
         // btnResplit
         // 
         this.btnResplit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.btnResplit.Image = ((System.Drawing.Image)(resources.GetObject("btnResplit.Image")));
         this.btnResplit.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.btnResplit.Name = "btnResplit";
         this.btnResplit.Size = new System.Drawing.Size(28, 28);
         this.btnResplit.Text = "Re-split";
         // 
         // cbZipEngine
         // 
         this.cbZipEngine.Items.AddRange(new object[] {
            "intern",
            "zlib",
            "sharp"});
         this.cbZipEngine.Name = "cbZipEngine";
         this.cbZipEngine.Size = new System.Drawing.Size(87, 23);
         this.cbZipEngine.ToolTipText = "Which compression engine to use";
         this.cbZipEngine.SelectedIndexChanged += new System.EventHandler(this.cbZipEngine_SelectedIndexChanged);
         // 
         // panelMain
         // 
         this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panelMain.Location = new System.Drawing.Point(0, 55);
         this.panelMain.Margin = new System.Windows.Forms.Padding(2);
         this.panelMain.Name = "panelMain";
         this.panelMain.Size = new System.Drawing.Size(1300, 568);
         this.panelMain.TabIndex = 5;
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
         this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1300, 647);
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
         this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
         this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormMain_KeyPress);
         this.menuStrip.ResumeLayout(false);
         this.menuStrip.PerformLayout();
         this.statusStrip.ResumeLayout(false);
         this.statusStrip.PerformLayout();
         this.toolStrip.ResumeLayout(false);
         this.toolStrip.PerformLayout();
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
      private System.Windows.Forms.ToolStripComboBox cbEncoding;
      private System.Windows.Forms.ToolStripStatusLabel statusLabelSearch;
      private System.Windows.Forms.ToolStripMenuItem contextMenuCopy;
      private System.Windows.Forms.ToolStripMenuItem menuTools;
      private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
      private System.Windows.Forms.ToolStripMenuItem menuToolsConfiguration;
      private System.Windows.Forms.ToolStripMenuItem menuFileClose;
      private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
      private System.Windows.Forms.ToolStripMenuItem menuView;
      private System.Windows.Forms.ToolStripMenuItem menuViewAll;
      private System.Windows.Forms.ToolStripMenuItem menuViewMatched;
      private System.Windows.Forms.ToolStripMenuItem menuViewUnmatched;
      private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportSelectedToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem exportMatchedToolStripMenuItem;
      private System.Windows.Forms.ToolStripButton btnWarning;
      private System.Windows.Forms.ToolStripButton btnResetSearch;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
      private System.Windows.Forms.ToolStripComboBox cbZipEntries;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
      private System.Windows.Forms.ToolStripLabel toolStripLabel1;
      private System.Windows.Forms.ToolStripComboBox cbSplit;
      private System.Windows.Forms.ToolStripButton btnResplit;
      private System.Windows.Forms.ToolStripMenuItem registerShellextToolStripMenuItem;
      private System.Windows.Forms.ToolStripComboBox cbZipEngine;
      private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem clearAllToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem toggleToolStripMenuItem;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripMenuItem matchedToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem nonMatchedToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem clearByMatchedToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem clearByNonMatchedToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem toolStripCopyMenuItem;
      private ToolStripComboBox cbFontSize;
      private ToolStripLabel toolStripLabel2;
      private ToolStripComboBox cbLoadLimits;
      private ToolStripSeparator toolStripSeparator4;
      private ToolStripMenuItem menuFindMultis;
      private ToolStripMenuItem menuFindSingles;
      private ToolStripSeparator toolStripSeparator5;
      private ToolStripMenuItem menuResetMatched;
   }
}

