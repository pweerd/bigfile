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

using Bitmanager.Core;
using Bitmanager.Grid;
using Microsoft.VisualBasic.ApplicationServices;
using System.Runtime.InteropServices;
using System.Text;
using static Bitmanager.BigFile.GridLines;
using Timer = System.Windows.Forms.Timer;

namespace Bitmanager.BigFile {

   public class GridLines : RawGrid {
      [DllImport ("user32.dll")]
      static extern IntPtr DefWindowProc (IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);
      protected const int WM_RBUTTONDOWN = 0x0204;
      protected const int WM_RBUTTONUP = 0x0205;
      protected const int WM_CONTEXTMENU = 0x7B;
      private const int WM_MOUSEWHEEL = 0x20A;
      private const int MK_CONTROL = 0x0008;
      public delegate void FontTick (Object sender, FontTickArgs e);  // delegate
      public event FontTick OnFontTick;


      private LogFile lf;
      private List<int> filter;
      public List<int> Filter {
         get { return filter; }
         set {
            if (value != filter) {
               filter = value;
               RowCount = (value != null) ? value.Count : ((lf == null) ? 0 : lf.PartialLineCount);
               RecomputeScrollBars ();
               GotoCell (0, 0, false);
            }
         }
      }
      private Settings _settings;
      public Settings Settings { 
         get {
            return _settings;
         } set {
            _settings = value;
            Invalidate ();
         } 
      }
      private readonly Color selectedForeColor;
      private readonly Color selectedBackColor;

      public GridLines () {
         this.selectedBackColor = Color.FromArgb (0, 120, 215);
         this.selectedForeColor = Color.White;
         BackColor = Color.White;
         ForeColor = Color.Black;
         RowCount = 0;
         Font = new Font ("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
         using (var cols = Columns) {
            cols.Clear ();
            cols.Add (new Column (100, HorizontalAlignment.Right));
            cols.Add (new Column (100000, HorizontalAlignment.Left));
            cols[0].BackColor = Color.FromArgb (0, 240, 240, 240);
            cols[1].BackColor = Color.White;
         }
      }

      public void SetLogFile (LogFile lf, bool isFirst) {
         this.lf = lf;

         if (lf == null) {
            base.RowCount = 0;
         } else {
            base.RowCount = lf.PartialLineCount;
            if (isFirst) { SelectedIndex = -1; GotoCell (0, 0); }
            updateColumnWidths ();
         }
      }

      private int measurePartialLineWidth (int index) {
         String txt = lf.GetPartialLine (index, -1, TabsReplacer.INSTANCE);
         return base.MeasureTextWidth (txt);
      }
      
      private PartialLineStats getWidestLine () {
         int maxIndex=-1;
         int maxWidth = 0;
     
         foreach (var ll in lf.LargestPartialLines) {
            int w = measurePartialLineWidth(ll.Index);
            if (w <= maxWidth) continue;
            maxIndex = ll.Index;
            maxWidth = w;
         }
         return new PartialLineStats(maxIndex, maxWidth);
      }
      protected void updateColumnWidths() {
         if (lf != null && RowCount > 0) {
            int lastPartial = GridRowToRow (RowCount - 1);
            if (lastPartial < 0) lastPartial = lf.PartialLineCount - 1;
            int lastLine = lf.PartialToLineNumber (lastPartial) + lf.SkippedLines;
            int w = base.MeasureTextWidth (lastLine.ToString ());
            SetColumnWidth (0, Math.Max(50, 10 + w), false);
            logger.Log ("Set col-0 width: {0}", w);

            var ll = getWidestLine ();
            logger.Log("Longest char line: len={0} at {1}", ll.Length, ll.Index);
            SetColumnWidth (1, ll.Length + 500, true);
         }
      }

      protected override void OnFontChanged (EventArgs e) {
         base.OnFontChanged (e);
         updateColumnWidths ();
         Invalidate ();
      }

      /// <summary>
      /// Convert a row into a real row by using an eventual filter.
      /// It is allowed to use the count of the filter as a row
      /// </summary>
      public override int GridRowToRow (int row) {
         if (row < 0 || filter==null) return row;
         return (filter.Count == row) ? filter[row - 1] + 1 : filter[row];
      }
      /// <summary>
      /// Try to find a real row-index in the eventual active filter
      /// </summary>
      public override int RowToGridRow (int row, bool returnGE = false) {
         if (filter == null || row < 0) return row;

         //Invariant: filter[i] < row && filter[j] >= row
         int i = -1;
         int j = filter.Count;
         while (j - i > 1) {
            int m = (i + j) / 2;
            if (filter[m] >= row) j = m; else i = m;
         }
         if (j >= filter.Count) return -1;
         if (!returnGE && filter[j] != row) return -1;
         return j;
      }

      protected override Cell GetCell (int row, int col) {
         Cell ret = CreateCell (col);
         if (row < 0 || lf==null) goto EXIT_RTN;
         row = GridRowToRow (row);

         if (row >= lf.PartialLineCount) goto EXIT_RTN;
         int flags = lf.GetLineFlags (row);
         if (col == 0) {
            if ((flags & LineFlags.CONTINUATION) == 0) {
               int lineNo = lf.OptPartialToLineNumber (row);
               if (lineNo >= 0) ret.Text = Invariant.Format ("{0}", lineNo+lf.SkippedLines);
            }
         } else {
            if ((flags & LineFlags.MATCHED) != 0 && _settings != null) {
               ret.BackColor = _settings.HighlightColor;
               if ((flags & LineFlags.SELECTED) != 0) {
                  ret.BackColor = _settings.SelectedHighlightColor;
                  ret.ForeColor = selectedForeColor;
               }
            } else if ((flags & LineFlags.SELECTED) != 0) {
               ret.ForeColor = selectedForeColor;
               ret.BackColor = selectedBackColor;
            }

            ret.Text = lf.GetPartialLine (row, -1, TabsReplacer.INSTANCE); //PW need to check: see GetLimitedLine 
         }

      EXIT_RTN:
         return ret;
      }


      public String GetTooltipForRow (int row) {
         if (lf == null || lf.PartialLineCount == 0) return null;


         int partial = GridRowToRow (row);
         int line = lf.PartialToLineNumber (partial);
         int skipped = lf.SkippedLines;
         int partialByteLen = lf.GetPartialLineLengthInBytes (partial);
         int lineByteLen = lf.GetLineLengthInBytes (line);
         int partialCharLen = lf.GetPartialLineLengthInChars (partial);
         int lineCharLen = lf.GetLineLengthInBytes (line);
         long offset = lf.GetLineOffset (line);

         var sb = new StringBuilder ();
         sb.AppendFormat (Invariant.Culture, "Line: {0}", line + skipped);
         if (skipped > 0) sb.AppendFormat (Invariant.Culture, " (internal={0})", line);
         sb.AppendFormat (Invariant.Culture, ", Grid-index: {0}", row);
         sb.AppendFormat (Invariant.Culture, ", Chars: {0} ({1} bytes)", lineCharLen, lineByteLen);

         sb.AppendFormat (Invariant.Culture, "\nOffset: {0} (0x{0:X}), pretty: {1}", offset, Pretty.PrintSize (offset));
         sb.AppendFormat (Invariant.Culture, "\nPartial index: {0}, chars: {1} ({2} bytes)", partial, partialCharLen, partialByteLen);
         return sb.ToString ();
      }



      protected override void OnKeyDown (KeyEventArgs e) {
         if (RowCount == 0 || e.KeyCode != Keys.End || e.Control) goto BASE;

         int row = FocusRow;
         if (row < 0) goto BASE;

         int partialIdx = GridRowToRow (row);
         int w = measurePartialLineWidth (partialIdx);
         if (w < 0) goto BASE;
         logger.Log ("GOTO line-end FocusRow={0}, partial={1}, w={2}", row, partialIdx, w);
         int cw = InnerClientRectangle.Width;
         HorizontalOffset = (w <= cw) ? 0 : w - cw / 2; ;
         e.Handled = true;

      BASE:
         base.OnKeyDown (e);
      }


      RowToolTip _rowTooltip;

      protected override void OnMouseLeave (EventArgs e) {
         logger.Log ("Mouseleave");
         base.OnMouseLeave (e);
         if (_rowTooltip != null) _rowTooltip.Stop ();
      }
      protected override void OnMouseMove (MouseEventArgs e) {
         base.OnMouseMove (e);
         int row = base.GetRowAndColFromLocation (e.X, e.Y, out var col);
         //logger.Log ("MouseMove: row={0}, col={1}", row, col);
         if (row < 0 || (col != 0 && e.X > 15)) { //also show the tooltip if we scrolled to the right and the mouse is over the first 15 pixels
            if (_rowTooltip != null) _rowTooltip.Stop ();
         } else {
            if (_rowTooltip == null) _rowTooltip = new RowToolTip (this, null);//, logger.Clone("tooltip"));
            _rowTooltip.Start (e.X, e.Y, row);
         }
      }

      protected override void WndProc (ref Message m) {
         switch (m.Msg) {
            case WM_MOUSEWHEEL:
               int wp = (int)(long)m.WParam;
               if ((wp & MK_CONTROL) == 0) break;
               if (OnFontTick != null) OnFontTick (this, new FontTickArgs (wp >> 16));
               return;
               //   case WM_RBUTTONDOWN: return;
               //   case WM_RBUTTONUP:
               //      DefWindowProc (m.HWnd, m.Msg, m.WParam, m.LParam);
               //      return;
         }
         base.WndProc (ref m);
      }


      public void SetFontSizePt (float size) {
         Font f = Font;
         if (Math.Abs (size - f.SizeInPoints) > .1f) Font = new Font (f.FontFamily, size, f.Style, GraphicsUnit.Point);
      }
      public void AddFontSizePt (float delta) {
         SetFontSizePt (delta + Font.SizeInPoints);
      }

      public class FontTickArgs : EventArgs {
         public int Delta;

         public FontTickArgs (int delta) {
            Delta = delta;
         }
      }


   }
}
