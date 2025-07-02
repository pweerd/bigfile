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
//#define ENABLE_DUMP_SELECTION
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
using Bitmanager.Grid;
using static Bitmanager.BigFile.GridLines;
using Bitmanager.Text;

namespace Bitmanager.BigFile {
   /// <summary>
   /// Main form for the application
   /// </summary>
   public partial class FormMain : Form, ILogFileCallback {
      private static readonly Logger logger = Globals.MainLogger;
      private LogFile lf;
      public LogFile LogFile => lf;
      private readonly SynchronizationContext synchronizationContext;
      private readonly SearchHistory searchboxDriver;
      private CancellationTokenSource cancellationTokenSource;
      private bool processing;
      private SettingsSource settingsSource;
      private Settings settings;
      public Settings Settings => settings;
      private ParserNode<SearchContext> lastQuery;
      private readonly SelectionHandler selectionHandler;
      private readonly float initialFontSize;

      public static Form Self;

      public FormMain () {
         Self = this;
         InitializeComponent ();
         this.Text = Globals.TITLE;
         gridLines = new GridLines ();

         synchronizationContext = SynchronizationContext.Current;

         cbEncoding.Items.Clear ();
         cbEncoding.Items.Add ("Utf8");
         cbEncoding.Items.Add ("Latin (ext)");
         cbEncoding.Items.Add ("Utf16");
         cbEncoding.Items.Add ("Utf16BE");

         cbFontSize.Items.Add ("A");
         cbFontSize.Items.Add ("A+");
         cbFontSize.Items.Add ("A++");
         cbFontSize.Items.Add ("A+++");
         cbFontSize.SelectedIndex = 0;
         cbFontSize.SelectedIndexChanged += cbFontSize_SelectedIndexChanged;
         initialFontSize = gridLines.Font.SizeInPoints;

         //if (Globals.IsDebug) {
         //   encodings[4] = new Utf81 ();
         //   encodings[5] = new Utf82 ();
         //   dropdownEncoding.Items.Add ("Utf8 with arr");
         //   dropdownEncoding.Items.Add ("Utf8 with ptrs");
         //}
         cbEncoding.SelectedIndex = 0;
         gridLines.OnFontTick += GridLines_OnFontTick;


         searchboxDriver = new SearchHistory (cbSearch);
         btnResetSearch.Visible = Globals.IsDebug;
         btnWarning.Visible = false;
         btnResplit.Visible = false;

         showZipEntries (false);
         selectionHandler = new SelectionHandler (gridLines);
         selectionHandler.OnAddSelection += selectionHandler_Add;
         selectionHandler.OnRemoveSelection += selectionHandler_Remove;
         selectionHandler.OnToggleSelection += selectionHandler_Toggle;

         foreach (var c in this.toolStrip.Items) {
            var cb = c as ToolStripComboBox;
            if (cb != null) cb.DropDown += AdjustDropDownWidth;
         }
      }

      #region FONT_TICKERS

      private void GridLines_MouseDoubleClick (object sender, MouseEventArgs e) {
         activateRow (gridLines.FocusRow);
      }

      private void GridLines_OnFontTick (object sender, FontTickArgs e) {
         float newSize = gridLines.Font.SizeInPoints + (e.Delta < 0 ? -1f : 1f);
         if (newSize < 6) newSize = 6;
         float diff = newSize - initialFontSize;
         int ix = (int)(diff + .5);
         logger.Log ("FONT: size={3}, newSize={0}, diff={1}, ix={2}", newSize, diff, ix, gridLines.Font.SizeInPoints);

         if (diff > -.05f && ix >= 0 && ix < cbFontSize.Items.Count)
            cbFontSize.SelectedIndex = ix;
         setGridFontSize (newSize);
      }

      private void cbFontSize_SelectedIndexChanged (object sender, EventArgs e) {
         int ix = cbFontSize.SelectedIndex;
         if (ix < 0) return;
         setGridFontSize (initialFontSize + ix);
      }

      private void setGridFontSize (float sizeInPt) {
         gridLines.SetFontSizePt (sizeInPt);
         gridLines.Focus ();
      }
      #endregion

      private void setEncodingComboFromEncoding (Encoding c) {
         int ix = 0;
         int cp = c.CodePage;
         switch (c.CodePage) {
            case FileEncoding.CP_EXTENDED_LATIN: ix = 1; break;
            case FileEncoding.CP_UTF16: ix = 2; break;
            case FileEncoding.CP_UTF16BE: ix = 3; break;
         }
         cbEncoding.SelectedIndex = ix;
      }
      private Encoding getCurrentEncoding () {
         int sel = cbEncoding.SelectedIndex;
         //logger.Log("Selected encoding idx={0}", sel);
         if (sel < 0) sel = 0;
         switch (sel) {
            case 0: return FileEncoding.Utf8;
            case 1: return FileEncoding.ExtendedLatin;
            case 2: return FileEncoding.Utf16;
            case 3: return FileEncoding.Utf16BE;
            default: throw new BMException ("Unexpected encoding-index: {0}", sel);
         }
      }

      FileHistory fileHistory, directoryHistory;
      GridLines gridLines;

      private void FormMain_Load (object sender, EventArgs e) {
         Bitmanager.Core.GlobalExceptionHandler.Hook ();
         //GCSettings.LatencyMode = GCLatencyMode.Batch;

         this.settingsSource = new SettingsSource (true);
         this.settings = this.settingsSource.Settings;
         this.settingsSource.Dump ("initial load");
         this.fileHistory = new FileHistory ("fh_");
         this.directoryHistory = new FileHistory ("dh_");

         //this.statusStrip.Padding = new System.Windows.Forms.Padding (1, 0, 10, 0);


         //Async checking of valid history: this takes time in case of network drives
         Task.Run (() => {
            fileHistory.RemoveInvalid (File.Exists);
            directoryHistory.RemoveInvalid (Directory.Exists);
            synchronizationContext.Post (new SendOrPostCallback (o => {
               createRecentItems ();
            }), null);
         });

         this.cbEncoding.SelectedIndex = 0;

         contextMenu.Items.Clear ();
         contextMenu.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.toolStripCopyMenuItem.Clone(),
            this.selectToolStripMenuItem.Clone(),
            this.exportToolStripMenuItem.Clone()});

         menuFileClose.Enabled = false;


         SuspendLayout ();
         gridLines.Settings = settings;
         gridLines.AllowDrop = true;
         gridLines.CausesValidation = false;

         panelMain.Controls.Add (gridLines);
         gridLines.Dock = DockStyle.Fill;
         gridLines.ContextMenuStrip = this.contextMenu;

         this.gridLines.DragDrop += new System.Windows.Forms.DragEventHandler (this.FormMain_DragDrop);
         this.gridLines.DragEnter += new System.Windows.Forms.DragEventHandler (this.FormMain_DragEnter);
         this.gridLines.KeyDown += new System.Windows.Forms.KeyEventHandler (this.FormMain_KeyDown);
         this.gridLines.KeyPress += new System.Windows.Forms.KeyPressEventHandler (this.FormMain_KeyPress);
         this.gridLines.MouseDoubleClick += GridLines_MouseDoubleClick;
         ResumeLayout ();


         cbZipEngine.SelectedIndex = 0;
         if (Globals.IsDebug) {
            string fn = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
            fn = IOUtils.FindFileToRoot (fn, @"UnitTests\data\test.txt", FindToTootFlags.Except);
            LoadFile (fn);
         } else {
            toolStripSeparator4.Visible = false;
            cbZipEngine.Visible = false;
         }

         var sb = new StringBuilder ();
         sb.Append ("You can enter boolean expressions (AND, OR, NOT) in the search bar.");
         sb.Append ("\nAlso, you can specify search-types by using a colon. Like cs:Paris, to do a case sensitive search for Paris");
         sb.Append ("\nFollowing types are supported:");
         sb.Append ("\n- cs: for case-sensitive search (default is non case sensitive)");
         sb.Append ("\n- csr: for case-sensitive regex search");
         sb.Append ("\n- r: for case-insensitive regex search");
         sb.Append ("\n\nExample: \"string with blanks\" AND r:en$");
         sb.Append ("\nRedoing searches for previous parts are extremely fast.");
         sb.Append ("\nCancel by <esc> or clicking in progress bar");

         btnSearch.ToolTipText = sb.ToString ();
         btnSearch.Tag = new TooltipTimes (4000, 30000);
         cbSearch.ToolTipText = btnSearch.ToolTipText;
         cbSearch.Tag = btnSearch.Tag;

         sb.Clear ();
         sb.Append ("Limits are specified as <skip>/<size>, or just skip.");
         sb.Append ("\n    <skip> can be a number (#lines to skip) or a size (eg: 10MB)");
         sb.Append ("\n    <size> is the maximum load size, like 4GB");
         sb.Append ("\n\nExamples:");
         sb.Append ("\n    \"1000\" skips 1000 lines and loads the rest");
         sb.Append ("\n    \"1000/4GB\" skips 1000 lines and loads a maximum of 4GB");
         sb.Append ("\n    \"4GB/3GB\" skips the lines in the 1st 4GB and loads a maximum of 3GB");
         cbLoadLimits.ToolTipText = sb.ToString ();
         cbLoadLimits.Tag = new TooltipTimes (1000, 30000);

         new ToolStripToolTipHelper (toolStrip, null);

         checkWarnings ();

         int left, top, width, height;
         SettingsSource.LoadFormPosition (out left, out top, out width, out height);
         if (left > 0) Left = left;
         if (top > 0) Top = top;
         if (width > 300) Width = width;
         if (height > 200) Height = height;

         //Parse the arguments from our main entrypoint
         var args = Program.Arguments;
         if (args.Length>0) {
            string startFile = args[0];
            if (File.Exists (startFile)) LoadFile (startFile);
            else if (Directory.Exists (startFile)) ShowOpenDialogAndLoad (startFile);
            else statusLabelMain.Text = Invariant.Format ("ERROR: {0} does not exist.", startFile);
         }
      }

      private void checkWarnings () {
         var sb = new StringBuilder ();
         if (!Globals.CanCompress) {
            sb.Append ("Memory compression is disabled because bmucore_XX.dll is not found or too old.");
         }
         if (!Globals.CanInternalGZip) {
            if (sb.Length > 0) sb.Append ("\n\n");
            sb.Append ("Internal gUnzipping is done via sharpZipLib (much slower) because bmucore_XX.dll is not found or too old.");
         }

         if (sb.Length == 0) {
            btnWarning.Visible = false;
            return;
         }

         sb.AppendFormat (Invariant.Culture, "\n\nVersion of {0} is {1}.", Globals.UCoreDll, Globals.UCoreDllVersion)
            .Append ("\nYou can install Bitmanager's core components from https://bitmanager.nl/distrib");
         string msg = sb.ToString ();
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

            //Only save positions when we are not maximized or minimized
            if (this.WindowState == FormWindowState.Normal) SettingsSource.SaveFormPosition (Left, Top, Width, Height);
            
            fileHistory.Save ();
            directoryHistory.Save ();
            ArchiveCache.Instance.Clear ();
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

      private void FormMain_DragEnter (object sender, DragEventArgs e) {
         if (processing) return;
         if (e.Data.GetDataPresent (DataFormats.FileDrop)) {
            e.Effect = DragDropEffects.Copy;
         } else {
            e.Effect = DragDropEffects.None;
         }
      }

      private void createRecentItems () {
         createRecentItems (fileHistory, menuRecentFiles);
         createRecentItems (directoryHistory, menuRecentFolders);
      }

      private void createRecentItems (FileHistory hist, ToolStripMenuItem menuItem) {
         string[] history = hist.Items;

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
         string fn = ((ToolStripMenuItem)sender).Text;
         if (!string.IsNullOrEmpty (fn)) {
            if (Directory.Exists (fn))
               ShowOpenDialogAndLoad (fn);
            else
               LoadFile (fn);
         }
      }


      /// <summary>
      /// Creates a LogFile object and let it asynchronously load the file
      /// </summary>
      private void LoadFile (string filePath, string zipEntry = null) {
         long maxLoadSize = long.MaxValue;
         long skip = 0;
         string tmp = cbLoadLimits.Text.TrimToNull();
         if (tmp != null) {
            cbLoadLimits.AddHistory (tmp);
            string[] args = tmp.Split ('/', StringSplitOptions.TrimEntries);
            switch (args.Length) {
               case 1:
                  skip = toSkip (args[0]);
                  break;
               case 2:
                  skip = toSkip (args[0]);
                  long x = args[1].Length == 0 ? 0 : Pretty.ParseSize (args[1]);
                  if (x > 0) maxLoadSize = x;
                  break;
            }
         }

         int maxPartial = Invariant.ToInt32 (cbSplit.Text, 0);
         cbSplit.AddHistory (cbSplit.Text);

         FormGoToLine.ResetGoto ();
         fileHistory.Add (filePath);
         directoryHistory.Add (Path.GetDirectoryName (filePath));
         createRecentItems ();
         indicateProcessing ();

         // Clear any existing filters/reset values
         clearAll ();

         this.Text = Globals.CreateTitle (filePath);

         statusLabelMain.Text = "Loading...";
         setSearchStatus (string.Empty);
         isFirstPartialLog = true;
         new LogFile (this, settings, getCurrentEncoding (), maxPartial, maxLoadSize, skip).Load (filePath, cancellationTokenSource.Token, zipEntry);
      }

      private static long toSkip (string txt) {
         if (string.IsNullOrEmpty (txt)) return 0;
         long tmp;
         if (Invariant.TryParse (txt, out tmp)) return tmp;
         return -Pretty.ParseSize (txt);
      }

      private void setSearchStatus (string txt, int count = 1) {
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

         return lf.ConvertToLines (selected);
      }




      FormLine lineForm;
      private void activateRow (int row) {
         if (row >= 0) {
            FormLine fl;
            if ((Control.ModifierKeys & Keys.Alt) != 0)
               fl = new FormLine ();
            else {
               fl = lineForm;
               if (fl == null || fl.IsClosed) fl = lineForm = new FormLine ();
            }
            fl.ShowLine (DesktopLocation, settings, lf, gridLines.Filter, row, lastQuery);
         }
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
            Clipboard.SetText (string.Empty);
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
         contextMenuCopy.Enabled = true;
         exportSelectedToolStripMenuItem.Enabled = true;
      }


      private void toolButtonSearch_Click (object sender, EventArgs e) {
         if (cbEncoding.SelectedIndex == -1) {
            cbEncoding.Select ();
            throw new Exception ("The encoding is not selected");
         }
         SearchFile ();
      }

      private void menuFileOpen_Click (object sender, EventArgs e) {
         ShowOpenDialogAndLoad (null);
      }
      private void ShowOpenDialogAndLoad (string initialDir) {
         OpenFileDialog openFileDialog = new OpenFileDialog ();
         openFileDialog.Filter = "All Files|*.*";
         openFileDialog.FileName = "*.*";
         openFileDialog.Title = "Select file to view";

         if (initialDir != null)
            openFileDialog.InitialDirectory = initialDir;
         else {
            var top = fileHistory.Top;
            if (!string.IsNullOrEmpty (top))
               openFileDialog.InitialDirectory = Path.GetDirectoryName (top);
         }

         if (openFileDialog.ShowDialog (this) == System.Windows.Forms.DialogResult.OK)
            LoadFile (openFileDialog.FileName);
      }

      private void clearAll () {
         searchboxDriver.Clear ();
         //if (lf != null && selectionHandler.AnySelected) lf.MarkUnselected (0, lf.PartialLineCount);
         selectionHandler.Clear ();
         lastQuery = null;
         gridLines.SetLogFile(null, true);

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
         setLogFile (null);  //PW in clearAll?

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
         string fn = IOUtils.FindFileToRoot (Globals.LoadDir, "help.html", FindToTootFlags.ReturnNull);
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
         gridLines.Focus ();
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


      void ILogFileCallback.OnProgress (LogFile lf, int percent) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            statusProgress.Value = percent;
         }), null);
      }


      void ILogFileCallback.OnSearchComplete (SearchResult result) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            gridLines.Invalidate ();
            handleViewSelection ();
            indicateFinished ();

            result.ThrowIfError ();
            int all = result.LogFile.PartialLineCount;
            int matched = result.NumMatches;
            int perc = all == 0 ? 0 : (int)(0.5 + 100.0 * matched / all);
            var msg = string.Format ("Matched {0:n0} / {1:n0} rows ({2}%, Search Terms: {3}),  # Duration: {4}",
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

      /// <summary>
      /// Accepts a partial index, returns a grid row index
      /// </summary>
      private int nextFilteredMatchedRow (int partialIdx) {
         var ret = nextFilteredPartialRow2 (partialIdx);
         logger.Log ("nextFilteredPartialMatch({0}) --> {1}", partialIdx, ret);
         return ret;
      }
      private int nextFilteredPartialRow2 (int partialIdx) {
         logger.Log ("nextFilteredPartialMatch({0})", partialIdx);
         if (partialIdx < 0) return -1;
         while (true) {
            int m = gridLines.RowToGridRow (partialIdx);
            if (m >= 0) return m;

            partialIdx = lf.NextPartialHit (partialIdx);
            if (partialIdx >= lf.PartialLineCount) break;
         }
         return int.MaxValue;
      }

      private int prevFilteredMatchedRow (int partialIdx) {
         if (partialIdx < 0) return -1;
         while (true) {
            int m = gridLines.RowToGridRow (partialIdx);
            if (m >= 0) return m;

            partialIdx = lf.PrevPartialHit (partialIdx);
            if (partialIdx < 0) break;
         }
         return -1;
      }
      void ILogFileCallback.OnSearchPartial (LogFile lf, int firstMatch) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            gridLines.Filter = null;
            //No NotifyExternalChange(). Not needed and the next select might take a long time in case of many lines.
            //It will cause the progressbar not updated.
            //Same in OnSearchComplete
            //selectionHandler.NotifyExternalChange ();
            positionToFirstHit (firstMatch);
         }), null);
      }

      bool isFirstPartialLog;
      private void setLogFile (LogFile newLF) {
         if (newLF != null) {
            newLF.SetEncoding (getCurrentEncoding ());
            if (lf != null) {
               if (!isFirstPartialLog) newLF.CopyStateBitsFrom (lf);
               lf.Dispose ();
            }
         }

         lf = newLF;
         if (newLF == null) {
            gridLines.SetLogFile (null, true);
            showZipEntries (false);
            return;
         }

         bool first = isFirstPartialLog;
         if (first) {
            selectViewAll ();
            selectionHandler.Clear ();
            isFirstPartialLog = false;
         }
         gridLines.SetLogFile (newLF, first);
         if (lineForm != null && !lineForm.IsClosed)
            lineForm.UpdateLogFile (newLF);


         if (lf.ZipEntries == null || lf.ZipEntries.Count == 0) {
            showZipEntries (false);
         } else {
            if (zipEntriesAutoCompleter == null) {
               zipEntriesAutoCompleter = new AutoCompleter (cbZipEntries.TextBox, null, 20, true);
               zipEntriesAutoCompleter.SelectionChanged += ZipEntriesAutoCompleter_SelectionChanged;
            }

            bool same = sameAutoCompleteEntries (lf.ZipEntries);
            logger.Log ("Existing AC={0}, new={1}, same={2}", zipEntriesAutoCompleter.Count, lf.ZipEntries.Count, same);
            if (!same) {
               zipEntriesAutoCompleter.Disable ();
               try {
                  zipEntriesAutoCompleter.Clear ();
                  foreach (var e in lf.ZipEntries) zipEntriesAutoCompleter.UncheckedAdd (createAcItem (e));
                  zipEntriesAutoCompleter.SelectedItem = zipEntriesAutoCompleter[lf.ZipEntries.SelectedItemIndex];
               } finally {
                  zipEntriesAutoCompleter.Enable ();
               }
            }

            showZipEntries (true);
         }
      }

      private bool sameAutoCompleteEntries(ZipEntries zipEntries) {
         if (zipEntriesAutoCompleter.Count != zipEntries.Count) return false;
         if (zipEntries.Count == 0) return true;
         return object.ReferenceEquals (zipEntries[0], zipEntriesAutoCompleter[0].Tag);
      }

      private AutoCompleter zipEntriesAutoCompleter;
      private void showZipEntries (bool visible) {
         cbZipEntries.Visible = visible;
         toolStripSeparator3.Visible = visible;
         if (!visible) {
            zipEntriesAutoCompleter?.Clear ();
         }
      }

      private static IAutoCompleteItem createAcItem (ZipEntry e) {
         var ret = new AutoCompleteItem (e.ToString (), false);
         ret.Tag= e;
         return ret;
      }

      private void ZipEntriesAutoCompleter_SelectionChanged (SelectionChangedArgs args) {
         if (args.Item == null || !cbZipEntries.Visible || args.Sender.Disabled) return;

         var entry = (ZipEntry)args.Item.Tag;
         LoadFile (entry.ArchiveName, entry.FullName);
      }



      void ILogFileCallback.OnLoadComplete (Result result) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            indicateFinished ();
            setSearchStatus ("");
            menuFileClose.Enabled = true;
            var sb = new StringBuilder ();

            if (result.Error != null) {
               sb.Append (" [ERROR]");
               statusLabelMain.Text = sb.ToString();
               result.ThrowIfError ();
            } 

            var lf = result.LogFile;
            logger.Log ("Detected2: {0}", lf.DetectedEncoding.Current.CodePage);
            setEncodingComboFromEncoding (lf.DetectedEncoding.Current);

            sb.AppendFormat (Invariant.Culture, "{0:n0} lines / {1}", lf.LineCount, Pretty.PrintSize (lf.Size));
            if (lf.SkippedLines > 0) {
               sb.AppendFormat (Invariant.Culture, ", ({0:n0} skipped)", lf.SkippedLines);
            }

            if (result.Cancelled) {
               sb.Append (" [PARTIAL LOADED]");
            } else {
               sb.Append (", # Duration: ");
               sb.Append (Pretty.PrintElapsedMs ((int)result.Duration.TotalMilliseconds));
            }
            statusLabelMain.Text = sb.ToString ();
            setLogFile (lf);

         }), null);
      }

      void ILogFileCallback.OnLoadCompletePartial (LogFile cloned) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            logger.Log (); //Separate by empty line
            logger.Log ("Detected: {0}", cloned.DetectedEncoding.Current);
            setEncodingComboFromEncoding (cloned.DetectedEncoding.Current);
            if (cloned.IsSkipping) {
               statusLabelMain.Text = Invariant.Format ("Skipping...  {0:n0} lines / {1} so far.", cloned.SkippedLines, Pretty.PrintSize (cloned.SkippedSize));
            } else {
               setLogFile (cloned);
               statusLabelMain.Text = string.Format ("Loading...  {0:n0} lines / {1} so far.", cloned.PartialLineCount, Pretty.PrintSize (cloned.Size));
            }
         }), null);
      }

      void ILogFileCallback.OnExportComplete (ExportResult result) {
         synchronizationContext.Post (new SendOrPostCallback (o => {
            indicateFinished ();

            result.ThrowIfError ();
            var msg = string.Format ("Exported {0:n0} lines,  # Duration: {1}",
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
            gridLines.Filter = null;
            return;
         }
         if (menuViewMatched.Checked) {
            gridLines.Filter = lf.GetMatchedList (settings.NumContextLines);
            return;
         }
         gridLines.Filter = lf.GetUnmatchedList (settings.NumContextLines);
         return;
      }

      private void menuView_Click (object sender, EventArgs e) {
         var item = sender as ToolStripMenuItem;
         var owner = item.Owner;
         foreach (ToolStripMenuItem x in owner.Items) x.Checked = x == item;

         handleViewSelection ();
      }

      private void selectViewAll () {
         foreach (ToolStripMenuItem x in menuViewAll.Owner.Items) {
            x.Checked = x == menuViewAll;
         }
         handleViewSelection ();
      }

      private void gotoToolStripMenuItem_Click (object sender, EventArgs e) {
         if (lf == null) return;
         gotoDialog ();
      }

      private void gotoDialog () {
         using (FormGoToLine f = new FormGoToLine ()) {
            if (f.ShowDialog (this) == DialogResult.OK)
               gotoLine (f.LineNumber, f.GotoType);
         }
      }
      private void gotoLine (int index, GotoType type) //PW nakijken
      {
         if (lf == null) { 
            gridLines.GotoCell (0, 0);
            return; 
         }
         bool select = true;
         if (type == GotoType.Row) goto HANDLE_GOTO;

         if (type == GotoType.Line) index = lf.PartialFromLineNumber (index - lf.SkippedLines);

         if (gridLines.Filter != null) {
            int tmp = gridLines.RowToGridRow (index, true);
            select = tmp >= 0 && tmp < gridLines.Filter.Count && gridLines.Filter[tmp] == index;
            index = tmp;
         }

         HANDLE_GOTO:
         gridLines.MakeCellVisible (index, 0, select);
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
                  gotoLine (0, GotoType.Row); break;
               case Keys.End:
                  gotoLine (int.MaxValue, GotoType.Row); break;
               case Keys.F3:
                  gotoPrevHit (); break;

#if ENABLE_DUMP_SELECTION
               //Dumping current selection on Ctrl-F12
               case Keys.F12:
                  dumpSelected (lf.GetSelectedPartialLines (100000, out var _), "partial");
                  break;
#endif
                  
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

      //Debug code to check on selections
      private void dumpSelected (List<int> list, string what) {
         var logger = Globals.MainLogger;
         logger.Log ("Dumping {0} {1} lines", list.Count, what);
         for (int i = 0; i < list.Count; i++) {
            logger.Log ("-- {0}: {1}", i, list[i]);
         }
      }


      private void gotoNextHit (int? _row=null) {
         if (lf == null) return;
         if (menuViewUnmatched.Selected)
            throw new Exception ("Goto prev/next hit impossible if filtered for unmatched.");

         int row = (_row!=null) ? (int) _row : gridLines.FocusRow;
         int partialIdx = gridLines.GridRowToRow (row);
         int newRow = this.nextFilteredMatchedRow (lf.NextPartialHit (partialIdx));
         logger.Log ("gotoNextHit (parm={0}, startRow={1}, partialIdx={2}) -> row={3}", _row, row, partialIdx, newRow);

         gridLines.MakeCellVisible (newRow, 0, true);
      }

      private void gotoPrevHit (int? _row=null) {
         if (lf == null) return;
         if (menuViewUnmatched.Selected)
            throw new Exception ("Goto prev/next hit impossible if filtered for unmatched.");

         int row = (_row != null) ? (int)_row : gridLines.FocusRow;
         int partialIdx = gridLines.GridRowToRow (row);
         int newRow = this.prevFilteredMatchedRow (lf.PrevPartialHit (partialIdx));
         logger.Log ("gotoPrevHit (parm={0}, startRow={1}, partialIdx={2}) -> row={3}", _row, row, partialIdx, newRow);

         gridLines.MakeCellVisible (newRow, 0, true);
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
               break;
            case (char)13: //Enter: activate line
               activateRow (gridLines.FocusRow);
               break;

            case '/':
               gotoNextHit (); break;
            case '?':
               gotoPrevHit (); break;
            case '<':
               gotoLine (0, GotoType.Row); break;
            case '>':
               gotoLine (int.MaxValue, GotoType.Row); break;
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
            logger.Log ("enter in search");
            toolButtonSearch_Click (btnSearch, null);
            gridLines.Focus ();
            e.Handled = true;
         }
      }


      private void dropdownEncoding_SelectedIndexChanged (object sender, EventArgs e) {
         if (lf != null) {
            lf.SetEncoding (getCurrentEncoding ());
            gridLines.Invalidate ();
         }
      }

      private void btnResetSearch_Click (object sender, EventArgs e) {
         searchboxDriver.Clear ();
      }

      private void registerShellextToolStripMenuItem_Click (object sender, EventArgs e) {
         registerShellExt ();
      }

      private void cbZipEngine_SelectedIndexChanged (object sender, EventArgs e) {
         var cb = sender as ToolStripComboBox;
         int ix = cb.SelectedIndex;
         if (ix < 0) return;
         LogFile.DbgStr = (string)cb.Items[ix];
      }

      #region selection-logic
      private void allToolStripMenuItem_Click (object sender, EventArgs e) {
         selectionHandler_Add (0, gridLines.RowCount);
      }

      private void clearAllToolStripMenuItem_Click (object sender, EventArgs e) {
         selectionHandler_Remove (0, gridLines.RowCount);
      }

      private void toggleToolStripMenuItem_Click (object sender, EventArgs e) {
         selectionHandler_Toggle (0, gridLines.RowCount);
      }

      private void selectionHandler_Add (int from, int to) {
         if (lf == null) return;
         if (gridLines.Filter != null)
            lf.MarkSelected (from, to, gridLines.Filter);
         else
            lf.MarkSelected (from, to);
         gridLines.Invalidate ();
      }

      private void selectionHandler_Remove (int from, int to) {
         if (lf == null) return;
         if (gridLines.Filter != null)
            lf.MarkUnselected (from, to, gridLines.Filter);
         else
            lf.MarkUnselected (from, to);
         gridLines.Invalidate ();
      }

      private void selectionHandler_Toggle (int from, int to) {
         if (lf == null) return;
         if (gridLines.Filter != null)
            lf.ToggleSelected (from, to, gridLines.Filter);
         else
            lf.ToggleSelected (from, to);
         gridLines.Invalidate ();
      }
      #endregion


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


      private void registerShellExt () {
         string exe = Path.ChangeExtension (Assembly.GetEntryAssembly ().Location, ".exe");
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

      private void menuFindMultis_Click (object sender, EventArgs e) {
         handleSearchResult (lf.SearchAllMultiLines (), "multi");
      }

      private void menuFindSingles_Click (object sender, EventArgs e) {
         handleSearchResult (lf.SearchAllSingleLines (), "single");
      }

      private void positionToFirstHit (int partialHitIndex) {
         logger.Log ("positionToFirstHit (first partial={0})", partialHitIndex);
         if (partialHitIndex < 0) return;
         int row = nextFilteredMatchedRow (partialHitIndex);
         if (row >= 0)
            gridLines.MakeCellVisible (row, 0, true);
      }

      private void handleSearchResult(SearchResult result, string what) {
         result.ThrowIfError ();
         int all = result.LogFile.PartialLineCount;
         int matched = result.NumMatches;
         int perc = all == 0 ? 0 : (int)(0.5 + 100.0 * matched / all);
         var msg = string.Format ("Matched {0:n0} / {1:n0} {2} lines ({3}%), # Duration: {4}",
                  matched,
                  all,
                  what,
                  perc,
                  Pretty.PrintElapsedMs ((int)result.Duration.TotalMilliseconds)
         );
         setSearchStatus (msg, matched);
         positionToFirstHit (result.FirstHit);
      }

      private void menuResetMatched_Click (object sender, EventArgs e) {
         lf.ResetMatches ();
         gridLines.Invalidate ();
      }

      private void createRegEntries (RegistryKey key, string exe) {
         key.SetValue ("", "BigFile");
         key.SetValue ("icon", exe);
         using (var rk = key.CreateSubKey (@"command", true)) {
            rk.SetValue ("", string.Format ("\"{0}\"  \"%1\"", exe));
         }

      }


      private void AdjustDropDownWidth (object sender, EventArgs e) {
         var cb = sender as ToolStripComboBox;
         if (cb == null) return;

         var font = cb.Font;
         int width = cb.Width - SystemInformation.VerticalScrollBarWidth;
         foreach (var obj in cb.Items) {
            int w = TextRenderer.MeasureText (obj.ToString(), font).Width;
            if (w > width) width = w;
         }
         width += SystemInformation.VerticalScrollBarWidth;
         cb.DropDownWidth = width;
      }
   }

   public enum GotoType {
      Line, PartialLine, Row
   }
}
