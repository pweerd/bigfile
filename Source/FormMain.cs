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

using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Threading;
using System.Text;
using Bitmanager.Core;
using Bitmanager.BigFile.Query;
using System.Runtime;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Bitmanager.IO;
using System.Diagnostics;
using Bitmanager.Query;
using System.Reflection;
using Microsoft.Win32;
using static BrightIdeasSoftware.ObjectListView;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Main form for the application
   /// </summary>
   public partial class FormMain : Form, ILogFileCallback {
      private static readonly Logger logger = Globals.MainLogger;
      private LogFile lf;
      private readonly SynchronizationContext synchronizationContext;
      private readonly SearchHistory searchboxDriver;
      private CancellationTokenSource cancellationTokenSource;
      private bool processing;
      private SettingsSource settingsSource;
      private Settings settings;
      private FixedFontMeasures fontMeasures;
      private readonly VirtualDataSource listDatasource;
      private readonly Encoding[] encodings;
      private ParserNode<SearchContext> lastQuery;
      private readonly SelectionHandler selectionHandler;

      public FormMain () {
         InitializeComponent ();
         this.Text = Globals.TITLE;

         synchronizationContext = SynchronizationContext.Current;
         encodings = new Encoding[6];
         encodings[0] = Encoding.UTF8;
         encodings[1] = Encoding.Default;
         encodings[2] = Encoding.Unicode;
         encodings[3] = Encoding.BigEndianUnicode;

         dropdownEncoding.Items.Clear ();
         dropdownEncoding.Items.Add ("Utf8");
         dropdownEncoding.Items.Add ("Windows");
         dropdownEncoding.Items.Add ("Utf16");
         dropdownEncoding.Items.Add ("Utf16BE");

         if (Globals.IsDebug) {
            encodings[4] = new Utf81 ();
            encodings[5] = new Utf82 ();
            dropdownEncoding.Items.Add ("Utf8 with arr");
            dropdownEncoding.Items.Add ("Utf8 with ptrs");
         }
         dropdownEncoding.SelectedIndex = 0;
         listLines.VirtualListDataSource = listDatasource = new VirtualDataSource (listLines);
         listLines.MultiSelect = false;
         listLines.SelectAllOnControlA = false;
         listLines.SelectColumnsOnRightClickBehaviour = ColumnSelectBehaviour.None;
         listLines.CopySelectionOnControlC = false;

         searchboxDriver = new SearchHistory (cbSearch);
         btnResetSearch.Visible = Globals.IsDebug;
         btnWarning.Visible = false;
         btnResplit.Visible = false;

         olvcLineNumber.AutoResize (ColumnHeaderAutoResizeStyle.None);
         olvcText.AutoResize (ColumnHeaderAutoResizeStyle.None);
         showZipEntries (false);
         selectionHandler = new SelectionHandler (this, listLines);
         selectionHandler.OnAddSelection += selectionHandler_Add;
         selectionHandler.OnRemoveSelection += selectionHandler_Remove;
         selectionHandler.OnToggleSelection += selectionHandler_Toggle;
      }

      private void showZipEntries (bool visible) {
         cbZipEntries.Visible = visible;
         toolStripSeparator3.Visible = visible;
         if (!visible) cbZipEntries.Items.Clear ();
      }

      private Encoding getCurrentEncoding () {
         int sel = dropdownEncoding.SelectedIndex;
         //logger.Log("Selected encoding idx={0}", sel);
         if (sel < 0) sel = 0;
         return encodings[sel];
      }

      FileHistory fileHistory, directoryHistory;

      ToolStripToolTipHelper tooltipHelper;
      private void FormMain_Load (object sender, EventArgs e) {
         Bitmanager.Core.GlobalExceptionHandler.Hook ();
         //GCSettings.LatencyMode = GCLatencyMode.Batch;

         this.settingsSource = new SettingsSource (true);
         this.settings = this.settingsSource.Settings;
         this.settingsSource.Dump ("initial load");
         this.fileHistory = new FileHistory ("fh_");
         this.directoryHistory = new FileHistory ("dh_");

         //Async checking of valid history: this takes time in case of network drives
         Task.Run (() => {
            fileHistory.RemoveInvalid (File.Exists);
            directoryHistory.RemoveInvalid (Directory.Exists);
            synchronizationContext.Post (new SendOrPostCallback (o => {
               createRecentItems ();
            }), null);
         });

         this.olvcLineNumber.AspectGetter = getLineNumber;
         this.olvcText.AspectGetter = getLimitedLine;
         this.dropdownEncoding.SelectedIndex = 0;

         contextMenu.Items.Clear ();
         contextMenu.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.toolStripCopyMenuItem.Clone(),
            this.selectToolStripMenuItem.Clone(),
            this.exportToolStripMenuItem.Clone()});

         menuFileClose.Enabled = false;
         listLines.Dock = DockStyle.Fill;
         listLines.Visible = true;
         fontMeasures = new FixedFontMeasures (listLines.Font);
         cbDbgLoad.SelectedIndex = 0;
         if (Globals.IsDebug) {
            String fn = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
            fn = IOUtils.FindFileToRoot (fn, @"UnitTests\data\test.txt", FindToTootFlags.Except);
            LoadFile (fn);
         } else {
            toolStripButton2.Visible = false;
            cbDbgLoad.Visible = false;
         }

         var sb = new StringBuilder ();
         sb.Append ("You can enter boolean expressions (AND, OR, NOT) in the search bar.");
         sb.Append ("\nAlso, you can specify search-types by using : like cs:Paris, to do a case sensitive search for Paris");
         sb.Append ("\nFollowing types are supported:");
         sb.Append ("\n- cs: for case-sensitive search (default is non case sensitive)");
         sb.Append ("\n- csr: for case-sensitive regex search");
         sb.Append ("\n- r: for case-insensitive regex search");
         sb.Append ("\n\nExample: \"with blank\" AND r:en$");
         sb.Append ("\nRedoing searches for previous parts are extremely fast.");
         sb.Append ("\nCancel by <esc> or clicking in progress bar");

         btnSearch.ToolTipText = sb.ToString ();
         cbSearch.ToolTipText = btnSearch.ToolTipText;

         tooltipHelper = new ToolStripToolTipHelper (this.toolStrip, cbSearch);
         tooltipHelper.ToolTipInterval = 20000;
         tooltipHelper.Tooltip.ReshowDelay = 4000;

         checkWarnings ();

         int left, top, width, height;
         SettingsSource.LoadFormPosition (out left, out top, out width, out height);
         if (left > 0) Left = left;
         if (top > 0) Top = top;
         if (width > 300) Width = width;
         if (height > 200) Height = height;

         //In NetCore the cmdline contains the dll as the first param
         var lexer = new Lexer (Environment.CommandLine);
         int i = 1;
         Lexer.Token t;
         while (true) {
            t = lexer.NextToken ();
            if (t == null || t.Type == Lexer.TokenType.Eof) break;
            if (t.Type != Lexer.TokenType.Value) continue;
            if (--i < 0) {
               String startFile = t.Text;
               if (File.Exists (startFile)) LoadFile (startFile);
               else if (Directory.Exists (startFile)) ShowOpenDialogAndLoad (startFile);
               break;
            };
         }
      }

      private void checkWarnings () {
         var sb = new StringBuilder ();
         if (!Globals.CanCompress) {
            sb.Append ("\n\nMemory compression is disabled because bmucore_XX.dll is not found or too old.");
            sb.AppendFormat ("\nVersion of {0} is {1}.", Globals.UCoreDll, Globals.UCoreDllVersion);
         }
         if (!Globals.CanInternalGZip) {
            sb.Append ("\n\nInternal gUnzipping is disabled because bmcore_102.dll is too old.");
            sb.AppendFormat ("\nVersion of {0} is {1}.", Globals.UCoreDll, Globals.UCoreDllVersion);
         }

         if (sb.Length == 0) {
            btnWarning.Visible = false;
            return;
         }

         String msg = sb.ToString (2, sb.Length - 2);
         btnWarning.ToolTipText = msg;
         btnWarning.Visible = true;
         btnWarning.AutoToolTip = false;
      }

      private void btnWarning_Click (object sender, EventArgs e) {
         MessageBox.Show (btnWarning.ToolTipText, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }


      private bool closeError;
      private void FormMain_FormClosing (object sender, FormClosingEventArgs e) {
         logger.Log ("Initiating close");
         if (!closeError) {
            closeError = true;
            cancel ();
            settingsSource.Save ();
            SettingsSource.SaveFormPosition (Left, Top, Width, Height);
            fileHistory.Save ();
            directoryHistory.Save ();
         }
      }

      private void FormMain_FormClosed (object sender, FormClosedEventArgs e) {
         if (lf != null) lf.Dispose ();
      }


      private void FormMain_DragDrop (object sender, DragEventArgs e) {
         if (processing) return;

         string[] files = (string[])e.Data.GetData (DataFormats.FileDrop);
         switch (files.Length) {
            case 0: return;
            case 1:
               LoadFile (files[0]);
               break;
            default:
               throw new Exception ("Only one file can be processed at one time");
         }
      }

      private void listLines_DragDrop (object sender, DragEventArgs e) {
         FormMain_DragDrop (sender, e);
      }


      private void FormMain_DragEnter (object sender, DragEventArgs e) {
         if (processing) return;
         if (e.Data.GetDataPresent (DataFormats.FileDrop)) {
            e.Effect = DragDropEffects.Copy;
         } else {
            e.Effect = DragDropEffects.None;
         }
      }

      private void listLines_DragEnter (object sender, DragEventArgs e) {
         FormMain_DragEnter (sender, e);
      }

      private void createRecentItems () {
         createRecentItems (fileHistory, menuRecentFiles);
         createRecentItems (directoryHistory, menuRecentFolders);
      }

      private void createRecentItems (FileHistory hist, ToolStripMenuItem menuItem) {
         String[] history = hist.Items;

         var subItems = new List<ToolStripMenuItem> ();
         foreach (var x in history) {
            if (x == null) continue;

            var subItem = new ToolStripMenuItem ();
            subItem.Text = x;
            subItem.AutoSize = true;
            subItem.Click += recentFile_Click;
            subItems.Add (subItem);
         }
         menuItem.DropDownItems.Clear ();
         if (subItems.Count > 0) menuItem.DropDownItems.AddRange (subItems.ToArray ());
      }

      private void recentFile_Click (object sender, EventArgs e) {
         String fn = ((ToolStripMenuItem)sender).Text;
         if (!String.IsNullOrEmpty (fn)) {
            if (Directory.Exists (fn))
               ShowOpenDialogAndLoad (fn);
            else
               LoadFile (fn);
         }
      }

      /// <summary>
      /// Handles the loading of a different entry in a zip file
      /// </summary>
      private void cbZipEntries_SelectedIndexChanged (object sender, EventArgs e) {
         var cb = (ToolStripComboBox)sender;
         int idx = cb.SelectedIndex;
         if (idx < 0) return;
         if (lf != null && lf.ZipEntries != null && lf.ZipEntries.SelectedEntry == idx) return;

         var entry = (ZipEntry)cb.Items[idx];
         LoadFile (entry.ArchiveName, entry.FullName);
      }


      /// <summary>
      /// Creates a LogFile object and let it asynchronously load the file
      /// </summary>
      private void LoadFile (string filePath, String zipEntry = null) {
         int maxPartial = Invariant.ToInt32 (cbSplit.Text);
         if (maxPartial < 0) maxPartial = 10 * 1024 * 1024;

         FormGoToLine.ResetGoto ();
         fileHistory.Add (filePath);
         directoryHistory.Add (Path.GetDirectoryName (filePath));
         createRecentItems ();
         indicateProcessing ();

         // Clear any existing filters/reset values
         clearAll ();

         this.Text = String.Format ("[{0}] - {1}", filePath, Globals.TITLE);

         statusLabelMain.Text = "Loading...";
         setSearchStatus (String.Empty);
         new LogFile (this, settings, getCurrentEncoding (), maxPartial).Load (filePath, cancellationTokenSource.Token, zipEntry);
      }

      private void setSearchStatus (String txt, int count = 1) {
         statusLabelSearch.Text = txt;
         statusLabelSearch.BackColor = count == 0 ? Color.FromArgb (0xF1, 0xD0, 0xD0) : statusLabelMain.BackColor;
      }

      private void SearchFile () {
         if (lf == null) return;

         lastQuery = searchboxDriver.GetParsedQuery ();
         if (lastQuery == null) return;

         setSearchStatus ("Searching...");

         indicateProcessing ();
         lf.SyncSettings (settings, getCurrentEncoding ());
         lf.Search (lastQuery, cancellationTokenSource.Token);
      }


      private List<int> getSelectedLineIndexes (int maxCount, out bool truncated) {
         truncated = false;
         if (lf == null) return null;
         var selected = lf.GetSelectedPartialLines (maxCount, out truncated);
         if (selected.Count == 0) return null;

         var filter = listDatasource.Filter;
         if (filter != null) {
            var list = new List<int> (selected.Count);
            foreach (int x in selected) list.Add (filter[x]);
            selected = list;
         }

         return lf.ConvertToLines (selected);
      }


      int neededTextLength;

      private void computeNeededTextLength () {
         if (fontMeasures == null) return;
         int needed = listLines.LowLevelScrollPosition.X + listLines.Width - olvcLineNumber.Width;

         neededTextLength = fontMeasures.GetTextLengthForPixels (needed);
         //int pixels = fontMeasures.GetTextPixels(neededTextLength);
         //logger.Log("(Re)size... Needed pixels is {0}, chrs={1}, pixels for these chars is: {2}", needed, neededTextLength, pixels);
      }

      private String getLimitedLine (Object model) {
         String x = model == null ? String.Empty : lf.GetPartialLine ((int)model, neededTextLength, replaceTabs);
         //logger.Log("{2}: Needed={0}, returned={1}, {3}", neededTextLength, x.Length, model, x.Length > neededTextLength + 32 ? x.Substring(0, neededTextLength) : x);
         return x.Length > neededTextLength + 32 ? x.Substring (0, neededTextLength) : x;
      }
      private static void replaceTabs (char[] arr, int len) {
         for (int i = 0; i < len; i++) {
            if (arr[i] == '\t') arr[i] = (char)0x279c; // 0x21e5;// 0xbb;
         }
      }

      private String getLine (Object model) {
         String x = model == null ? String.Empty : lf.GetPartialLine ((int)model);
         return x;
      }
      private String getLineNumber (Object model) {
         var str = String.Empty;
         if (model != null) {
            int ix = lf.GetOptRealLineNumber ((int)model);
            if (ix >= 0) str = ix.ToString ();
         }
         return str;
      }



      /// <summary>
      /// Determines how a row(item) in the listview is formatted
      /// </summary>
      private void listLines_FormatRow (object sender, BrightIdeasSoftware.FormatRowEventArgs e) {
         if (e.Model == null) return;

         LineFlags flags = lf.GetLineFlags ((int)e.Model);
         Color f = listLines.ForeColor;
         Color b = listLines.BackColor;

         if ((flags & LineFlags.Match) != 0) {
            b = settings.HighlightColor;
            if ((flags & LineFlags.Selected) != 0) {
               b = settings.SelectedHighlightColor;
               f = listLines.SelectedForeColorOrDefault;
            }
         } else if ((flags & LineFlags.Selected) != 0) {
            f = listLines.SelectedForeColorOrDefault;
            b = listLines.SelectedBackColorOrDefault;
         }

         e.Item.BackColor = b;
         e.Item.SelectedBackColor = b;
         e.Item.ForeColor = f;
         e.Item.SelectedForeColor = f;
         //PW
         //else if ((flags & LineFlags.Context) != 0)
         //{
         //   e.Item.BackColor = settings.ContextColor;
         //}
      }

      FormLine lineForm;
      private void listLines_ItemActivate (object sender, EventArgs e) {
         if (listLines.SelectedObjects.Count != 1) return;

         int m = listLines.SelectedIndex;

         FormLine fl;
         if ((Control.ModifierKeys & Keys.Alt) != 0)
            fl = new FormLine ();
         else {
            fl = lineForm;
            if (fl == null || fl.IsClosed) fl = lineForm = new FormLine ();
         }
         fl.ShowLine (settings, lf, listDatasource.Filter, m, lastQuery);
      }

      private enum WhatToExport { All, Selected, Matched };
      private void export (WhatToExport what) {
         if (lf == null) return;

         SaveFileDialog sfd = new SaveFileDialog ();
         sfd.Filter = "Gzip file|*.gz|Text file|*.txt|Any File|*.*";
         sfd.FileName = Path.GetFileName (lf.FileName).Replace ('.', '_') + "_exported";
         sfd.InitialDirectory = Path.GetDirectoryName (lf.FileName);
         sfd.Title = "Select export file";
         sfd.DefaultExt = ".gz";
         if (sfd.ShowDialog (this) != DialogResult.OK) return;

         List<int> toExport = null;
         switch (what) {
            default: what.ThrowUnexpected (); break;
            case WhatToExport.All: break;
            case WhatToExport.Selected:
               toExport = getSelectedLineIndexes (int.MaxValue, out var truncated);
               if (toExport == null) return; //Nothing to export
               break;
            case WhatToExport.Matched:
               toExport = lf.GetMatchedList (0);
               toExport = lf.ConvertToLines (toExport);
               break;
         }

         indicateProcessing ();
         setSearchStatus (Invariant.Format ("Exporting {0:n0} lines...", what.ToString ().ToLowerInvariant ()));
         lf.Export (toExport, sfd.FileName, cancellationTokenSource.Token);
      }
      private void exportMatchedToolStripMenuItem_Click (object sender, EventArgs e) {
         export (WhatToExport.Matched);
      }

      private void exportSelectedToolStripMenuItem_Click (object sender, EventArgs e) {
         export (WhatToExport.Selected);
      }

      private void exportAllToolStripMenuItem_Click (object sender, EventArgs e) {
         export (WhatToExport.All);
      }


      //Copy the selection to the clipboard
      private void copyToClipboard () {
         if (lf == null) return;
         int copied = 0;
         var toExport = getSelectedLineIndexes (settings.MaxCopyLines, out var truncated);
         if (toExport == null || toExport.Count == 0) {
            Clipboard.SetText (String.Empty);
            goto EXIT_RTN;
         }

         int memLimit = 64 * 1024 * 1024;

         StringBuilder sb = new StringBuilder (128 * (1 + toExport.Count));
         foreach (int lineIdx in toExport) {
            ++copied;
            sb.AppendLine (lf.GetLine (lineIdx));
            if (sb.Length > memLimit) {
               truncated = true;
               break;
            }
         }
         Clipboard.SetText (sb.ToString ());

      EXIT_RTN:
         var fmt = truncated ? "Copied {0:n0} lines (truncated!)" : "Copied {0:n0} lines";
         setSearchStatus (Invariant.Format (fmt, copied));
      }

      private void contextMenuCopy_Click (object sender, EventArgs e) {
         copyToClipboard ();
      }

      private void toolStripCopyMenuItem_Click (object sender, EventArgs e) {
         copyToClipboard ();
      }



      private void contextMenu_Opening (object sender, System.ComponentModel.CancelEventArgs e) {
         if (listLines.SelectedObjects.Count > this.settings.MaxCopyLines) {
            contextMenuCopy.Enabled = false;
            exportSelectedToolStripMenuItem.Enabled = false;
            return;
         }

         contextMenuCopy.Enabled = true;
         exportSelectedToolStripMenuItem.Enabled = true;
      }


      private void toolButtonSearch_Click (object sender, EventArgs e) {
         if (dropdownEncoding.SelectedIndex == -1) {
            dropdownEncoding.Select ();
            throw new Exception ("The encoding is not selected");
         }
         SearchFile ();
      }

      private void menuFileOpen_Click (object sender, EventArgs e) {
         ShowOpenDialogAndLoad (null);
      }
      private void ShowOpenDialogAndLoad (String initialDir) {
         OpenFileDialog openFileDialog = new OpenFileDialog ();
         openFileDialog.Filter = "All Files|*.*";
         openFileDialog.FileName = "*.*";
         openFileDialog.Title = "Select file to view";

         if (initialDir != null)
            openFileDialog.InitialDirectory = initialDir;
         else {
            var top = fileHistory.Top;
            if (!String.IsNullOrEmpty (top))
               openFileDialog.InitialDirectory = Path.GetDirectoryName (top);
         }

         if (openFileDialog.ShowDialog (this) == System.Windows.Forms.DialogResult.OK)
            LoadFile (openFileDialog.FileName);
      }

      private void clearAll () {
         searchboxDriver.Clear ();
         lastQuery = null;
         this.listDatasource.Clear ();

         if (lineForm != null) {
            lineForm.Close ();
            lineForm.Dispose ();
            lineForm = null;
         }
      }

      /// <summary>
      /// Close the resources used for opening and processing the log file
      /// </summary>
      private void menuFileClose_Click (object sender, EventArgs e) {
         this.Text = Globals.TITLE;

         // Clear any existing filters/reset values
         clearAll ();
         listLines.ClearObjects ();
         listDatasource.SetContent (0);

         setLogFile (null);

         menuFileClose.Enabled = false;
         statusLabelMain.Text = "";
         setSearchStatus ("");
      }

      /// <summary>
      /// Exits the application
      /// </summary>
      private void menuFileExit_Click (object sender, EventArgs e) {
         Application.Exit ();
      }

      private void menuHelpHelp_Click (object sender, EventArgs e) {
         String fn = IOUtils.FindFileToRoot (Globals.LoadDir, "help.html", FindToTootFlags.ReturnNull);
         if (fn != null) {
            var psa = new ProcessStartInfo (fn);
            psa.UseShellExecute = true;
            psa.WorkingDirectory = Path.GetDirectoryName (fn);
            Process.Start (psa);
         }
      }

      private void menuHelpAbout_Click (object sender, EventArgs e) {
         using (FormAbout f = new FormAbout ()) {
            f.ShowDialog (this);
         }
      }


      private void menuToolsConfiguration_Click (object sender, EventArgs e) {
         using (FormSettings f = new FormSettings (this.settingsSource)) {
            f.ShowDialog (this);
            checkWarnings ();
            settings = settingsSource.Settings;
         }
      }


      private void indicateProcessing () {
         menuFileOpen.Enabled = false;
         menuFileExit.Enabled = false;
         btnSearch.Enabled = false;
         processing = true;
         statusProgress.Value = 0;
         setHourGlass ();
         cancellationTokenSource = new CancellationTokenSource ();
         listLines.Focus ();
      }
      private void indicateFinished () {
         menuFileOpen.Enabled = true;
         menuFileExit.Enabled = true;
         btnSearch.Enabled = true;
         processing = false;
         statusProgress.Value = 0;
         clrHourGlass ();
         Utils.FreeAndNil (ref cancellationTokenSource);
      }

      private void cancel () {
         if (cancellationTokenSource != null) {
            logger.Log ("Cancelling");
            this.cancellationTokenSource.Cancel ();
         }
      }

      private void statusProgress_Click (object sender, EventArgs e) {
         cancel ();
      }

      private void listLines_Scroll (object sender, ScrollEventArgs e) {
         if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            this.computeNeededTextLength ();
      }
      private void listLines_Resize (object sender, EventArgs e) {
         this.computeNeededTextLength ();
      }


      void ILogFileCallback.OnProgress (LogFile lf, int percent) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            statusProgress.Value = percent;
         }), null);
      }


      void ILogFileCallback.OnSearchComplete (SearchResult result) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            listLines.Refresh ();
            indicateFinished ();

            result.ThrowIfError ();
            int all = result.LogFile.PartialLineCount;
            int matched = result.NumMatches;
            int perc = all == 0 ? 0 : (int)(0.5 + 100.0 * matched / all);
            var msg = String.Format ("Matched {0:n0} / {1:n0} lines ({2}%, Search Terms: {3}),  # Duration: {4}",
                   matched,
                   all,
                   perc,
                   result.NumSearchTerms,
                   Pretty.PrintElapsedMs ((int)result.Duration.TotalMilliseconds)
            );
            if (result.Error != null) {
               setSearchStatus (msg + " [ERROR]", 0);
               handleViewSelection ();
               result.ThrowIfError ();
            } else if (result.Cancelled)
               setSearchStatus (msg + " [CANCELLED]", 0);
            else
               setSearchStatus (msg, matched);

            handleViewSelection ();
         }), null);
      }

      void ILogFileCallback.OnSearchPartial (LogFile lf, int firstMatch) {
         logger.Log ("OnSearchPartial (first={0})", firstMatch);
         synchronizationContext.Post (new SendOrPostCallback (o => {
            if (this.listDatasource.Filter != null) {
               firstMatch = this.listDatasource.Filter.IndexOf (firstMatch);
            }
            if (firstMatch >= 0)
               gotoAndSelectLogicalLineIndex (firstMatch);
         }), null);
      }

      private void setLogFile (LogFile newLF) {
         if (newLF != null)
            newLF.SetEncoding (getCurrentEncoding ());
         else
            listDatasource.SetContent (0);

         if (lf != null && lf != newLF) lf.Dispose ();
         lf = newLF;
         if (newLF == null) {
            showZipEntries (false);
            return;
         }

         if (newLF.PartialLineCount > 0) {
            olvcLineNumber.Width = 20 + fontMeasures.GetTextPixels (newLF.PartialLineCount.ToString (), olvcLineNumber.Text);

            int largestIndex = newLF.LongestPartialIndex;
            String largestLine = getLine (largestIndex);
            int w = fontMeasures.GetTextPixels (largestLine, 20);
            int min = listLines.Width - olvcLineNumber.Width - 20 - listLines.Margin.Horizontal;
            logger.Log ("-- w={0}, min={1}", w, min);
            if (w < min) w = min;
            olvcText.Width = w;

            logger.Log ("-- new width={0}", olvcText.Width);
            computeNeededTextLength ();
            logger.Log ("-- largest partial: {0} at {1}, largest line at {2}",
               largestLine.Length,
               largestIndex,
               newLF.LongestLineIndex);
            logger.Log ("-- Max width is {0} pixels, pixels in screen is {1}", w, listLines.LowLevelScrollPosition.X + listLines.Width - olvcLineNumber.Width);
         }
         listDatasource.SetContent (newLF.PartialLineCount);
         if (lineForm != null && !lineForm.IsClosed)
            lineForm.UpdateLogFile (newLF);


         if (lf.ZipEntries == null || lf.ZipEntries.Count == 0) {
            showZipEntries (false);
         } else {
            cbZipEntries.Items.Clear ();
            foreach (var e in lf.ZipEntries) cbZipEntries.Items.Add (e);

            cbZipEntries.SelectedIndex = lf.ZipEntries.SelectedEntry;
            showZipEntries (true);
         }
      }

      void ILogFileCallback.OnLoadComplete (Result result) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            indicateFinished ();
            setSearchStatus ("");
            menuFileClose.Enabled = true;

            var lf = result.LogFile;
            String part1 = String.Format ("{0:n0} lines / {1}", lf.PartialLineCount, Pretty.PrintSize (lf.Size));

            if (result.Error != null) {
               statusLabelMain.Text = part1 + " [ERROR]";
               result.ThrowIfError ();
            } else if (result.Cancelled) {
               statusLabelMain.Text = part1 + " [CANCELLED]";
            } else {
               String duration = Pretty.PrintElapsedMs ((int)result.Duration.TotalMilliseconds);
               statusLabelMain.Text = part1 + ", # Duration: " + duration;
            }

            //The logfile will not be set if we had errors. The state of the logfile is unpredictable then...
            if (result.Error == null) setLogFile (result.LogFile);
         }), null);
      }

      void ILogFileCallback.OnLoadCompletePartial (LogFile cloned) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            logger.Log (); //Separate by empty line
            setLogFile (cloned);
            statusLabelMain.Text = String.Format ("Loading...  {0:n0} lines / {1} so far.", cloned.PartialLineCount, Pretty.PrintSize (cloned.Size));
         }), null);
      }

      void ILogFileCallback.OnExportComplete (ExportResult result) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            indicateFinished ();

            result.ThrowIfError ();
            var msg = String.Format ("Exported {0:n0} lines,  # Duration: {1}",
                   result.NumExported,
                   Pretty.PrintElapsedMs ((int)result.Duration.TotalMilliseconds)
            );
            if (result.Error != null) {
               setSearchStatus (msg + " [ERROR]", 0);
               result.ThrowIfError ();
            } else if (result.Cancelled)
               setSearchStatus (msg + " [CANCELLED]", 0);
            else
               setSearchStatus (msg);
         }), null);
      }


      private void handleViewSelection () {
         if (lf == null || lf.PartialLineCount == 0) return;
         if (menuViewAll.Checked) {
            listDatasource.SetContent (lf.PartialLineCount);
            return;
         }
         if (menuViewMatched.Checked) {
            listDatasource.SetContent (lf.GetMatchedList (settings.NumContextLines));
            return;
         }
         listDatasource.SetContent (lf.GetUnmatchedList (settings.NumContextLines));
         return;
      }
      private void menuView_Click (object sender, EventArgs e) {
         var item = sender as ToolStripMenuItem;
         var owner = item.Owner;
         foreach (ToolStripMenuItem x in owner.Items) x.Checked = x == item;

         handleViewSelection ();
      }

      private void gotoToolStripMenuItem_Click (object sender, EventArgs e) {
         if (lf == null) return;
         gotoDialog ();
      }

      private void gotoDialog () {
         using (FormGoToLine f = new FormGoToLine ()) {
            if (f.ShowDialog (this) == DialogResult.OK)
               gotoLine (f.LineNumber, f.IsPartial);
         }
      }
      private void gotoLine (int line, bool isPartial) //PW nakijken
      {
         listLines.TopItemIndex = 0;
         if (lf == null) return;
         int index = (line < 0) ? 0 : line;
         if (!isPartial) index = lf.PartialFromLineNumber (index);

         index = lf.PartialToLogicalIndex (index, listDatasource.Filter);
         gotoAndSelectLogicalLineIndex (index);
      }

      private void gotoAndSelectLogicalLineIndex (int index) {
         if (index < 0) index = 0;
         int N = listLines.GetItemCount ();
         if (N == 0) return;
         if (index >= N) index = N - 1;

         //if (index < 0 || index >= listLines.GetItemCount()) return;
         listLines.SelectedIndex = index;
         listLines.EnsureVisible (index);
         listLines.Update ();
      }

      private void FormMain_KeyDown (object sender, KeyEventArgs e) {
         if (e.Control) {
            switch (e.KeyCode) {
               default: return;
               case Keys.C:
                  copyToClipboard (); break;
               case Keys.F:
                  cbSearch.Focus (); break;
               case Keys.G:
                  gotoToolStripMenuItem_Click (this, null); break;
               case Keys.Home:
                  gotoLine (0, true); break;
               case Keys.End:
                  gotoLine (int.MaxValue, true); break;
               case Keys.F3:
                  gotoPrevHit (); break;
            }
            e.Handled = true;
            return;
         }

         if (e.Alt || e.Shift) return;
         switch (e.KeyCode) {
            default: return;
            case Keys.F3:
               gotoNextHit (); break;
         }
         e.Handled = true;
      }

      private void gotoNextHit () {
         if (lf == null) return;
         if (menuViewUnmatched.Selected)
            throw new Exception ("Goto prev/next hit impossible if filtered for unmatched.");

         listLines.TopItemIndex = 0;
         var filter = listDatasource.Filter;
         int index = listLines.SelectedIndex;
         logger.Log ("gotoNextHit (index={0}, filter={1})", index, filter == null ? 0 : 1);
         if (index >= 0) {
            if (filter != null) index = listDatasource.Filter[index];
         }

         index = lf.NextPartialHit (index);
         if (filter == null) goto DO_POSITION;
         int i = 0;
         for (; i < filter.Count; i++) {
            if (filter[i] >= index) break;
         }
         index = i;

      DO_POSITION:
         gotoAndSelectLogicalLineIndex (index);
      }

      private void gotoPrevHit () {
         if (lf == null) return;
         if (menuViewUnmatched.Selected)
            throw new Exception ("Goto prev/next hit impossible if filtered for unmatched.");

         listLines.TopItemIndex = 0;
         var filter = listDatasource.Filter;
         int index = listLines.SelectedIndex;
         if (index >= 0) {
            if (filter != null) index = listDatasource.Filter[index];
         }

         index = lf.PrevPartialHit (index);
         if (filter == null) goto DO_POSITION;
         int i = 0;
         for (; i < filter.Count; i++) {
            if (filter[i] > index) break;
         }
         index = i - 1;

      DO_POSITION:
         gotoAndSelectLogicalLineIndex (index);
      }


      private void FormMain_KeyPress (object sender, KeyPressEventArgs e) {
         logger.Log ("Keypress char={0} send={1}", (int)e.KeyChar, sender.GetType ().Name);
         if (sender is TextBox || sender is ComboBox) return;
         switch (e.KeyChar) {
            default: return;

            case (char)27: //escape
               cancel ();
               break;

            case (char)6:  //CTRL_F
            case (char)7:  //CTRL_G
            case (char)13: //Enter: activate line
               break;

            case '/':
               gotoNextHit (); break;
            case '?':
               gotoPrevHit (); break;
            case '<':
               gotoLine (0, true); break;
            case '>':
               gotoLine (int.MaxValue, true); break;
         }
         e.Handled = true;
      }

      private void setHourGlass () {
         Cursor.Current = Cursors.WaitCursor;
         UseWaitCursor = true;
      }
      private void clrHourGlass () {
         Cursor.Current = Cursors.Default;
         UseWaitCursor = false;
      }

      private void cbSearch_KeyPress (object sender, KeyPressEventArgs e) {
         if (e.KeyChar == '\r') //Enter key
         {
            toolButtonSearch_Click (btnSearch, null);
            listLines.Focus ();
            e.Handled = true;
         }
      }


      private void dropdownEncoding_SelectedIndexChanged (object sender, EventArgs e) {
         if (lf != null) lf.SetEncoding (getCurrentEncoding ());
      }

      private void btnResetSearch_Click (object sender, EventArgs e) {
         searchboxDriver.Clear ();
      }

      private void registerShellextToolStripMenuItem_Click (object sender, EventArgs e) {
         registerShellExt ();
      }

      private void registerShellExt () {
         String exe = Path.ChangeExtension (Assembly.GetEntryAssembly ().Location, ".exe");
         try {
            using (var rk = Registry.ClassesRoot.CreateSubKey (@"*\shell\BigFile", true)) {
               createRegEntries (rk, exe);
            }
            using (var rk = Registry.ClassesRoot.CreateSubKey (@"Directory\shell\BigFile", true)) {
               createRegEntries (rk, exe);
            }
         } catch (Exception e) {
            throw new BMException (e, "{0}\r\n\r\nYou might want to run BigFile as administrator and rerun.", e.Message);
         }
      }

      private void cbDbgLoad_SelectedIndexChanged (object sender, EventArgs e) {
         var cb = sender as ToolStripComboBox;
         int ix = cb.SelectedIndex;
         if (ix < 0) return;
         LogFile.DbgStr = (String)cb.Items[ix];
      }

      private void allToolStripMenuItem_Click (object sender, EventArgs e) {
         selectionHandler_Add (0, listLines.VirtualListSize);
      }

      private void clearAllToolStripMenuItem_Click (object sender, EventArgs e) {
         selectionHandler_Remove (0, listLines.VirtualListSize);
      }

      private void toggleToolStripMenuItem_Click (object sender, EventArgs e) {
         selectionHandler_Toggle (0, listLines.VirtualListSize);
      }

      private void selectionHandler_Add (int from, int to) {
         if (lf == null) return;
         lf.MarkSelected (from, to);
         listLines.Invalidate ();
         logger.Log ("done");
      }
      private void selectionHandler_Remove (int from, int to) {
         if (lf == null) return;
         lf.MarkUnselected (from, to);
         listLines.Invalidate ();
      }
      private void selectionHandler_Toggle (int from, int to) {
         if (lf == null) return;
         lf.ToggleSelected (from, to);
         listLines.Invalidate ();
      }

      /// <summary>
      /// Select all matching lines (without clearing the selected items)
      /// </summary>
      private void matchedToolStripMenuItem_Click (object sender, EventArgs e) {
         if (lf != null) { lf.SelectAllMatched (); selectionHandler.NotifyExternalChange (); }
      }

      /// <summary>
      /// Select all non-matching lines (without clearing the selected items)
      /// </summary>
      private void nonMatchedToolStripMenuItem_Click (object sender, EventArgs e) {
         if (lf != null) {
            lf.SelectAllNonMatched (); selectionHandler.NotifyExternalChange ();
         }
      }

      private void clearByMatchedToolStripMenuItem_Click (object sender, EventArgs e) {
         if (lf != null) {
            lf.UnselectAllMatched (); selectionHandler.NotifyExternalChange ();
         }
      }

      private void clearByNonMatchedToolStripMenuItem_Click (object sender, EventArgs e) {
         if (lf != null) {
            lf.UnselectAllNonMatched (); selectionHandler.NotifyExternalChange ();
         }
      }

      private void listLines_CellRightClick (object sender, CellRightClickEventArgs e) {
         e.Handled = true;
      }

      private void createRegEntries (RegistryKey key, String exe) {
         key.SetValue ("", "BigFile");
         key.SetValue ("icon", exe);
         using (var rk = key.CreateSubKey (@"command", true)) {
            rk.SetValue ("", String.Format ("\"{0}\"  \"%1\"", exe));
         }

      }
   }

}
