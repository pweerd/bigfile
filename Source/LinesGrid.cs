using Bitmanager.Core;
using Bitmanager.Grid;
using System.Runtime.InteropServices;
using System.Text;

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
            filter = value;
            RowCount = (value != null) ? value.Count : lf.PartialLineCount;
            RecomputeScrollBars ();
            GotoCell (0, 0, false);
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
         this.selectedBackColor = ((SolidBrush)SystemBrushes.ActiveCaption).Color;
         this.selectedForeColor = ((SolidBrush)SystemBrushes.ActiveCaptionText).Color;
         BackColor = Color.White;
         ForeColor = Color.Black;
         RowCount = 0;
         Font = new Font ("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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

      protected void updateColumnWidths() {
         if (lf != null && RowCount > 0) {
            int lastIndex = GridRowToRow (RowCount - 1);
            if (lastIndex < 0) lastIndex = lf.PartialLineCount - 1;
            lastIndex = lf.PartialToLineNumber (lastIndex);
            int w = base.MeasureTextWidth (lastIndex.ToString ());
            SetColumnWidth (0, Math.Max(50, 10 + w));
            logger.Log ("Set col-0 width: {0}", w);

            //int largestIx = 0;
            //int largestLen = 0;
            //for (int i = 0; i < lf.PartialLineCount; i++) {
            //   String x = lf.GetPartialLine (i);
            //   if (x.Length > largestLen) {
            //      largestLen = x.Length;
            //      largestIx = i;
            //   }
            //}
            //logger.Log ("Largest at line {0}, chars={1}", largestIx, largestLen);

            //int largestIndex = lf.LongestPartialIndex;
            //String content = lf.GetPartialLine (largestIndex);
            //w = base.MeasureTextWidth (lf.GetPartialLine (largestIndex));
            //SetColumnWidth (1, 25000 + w);
            //logger.Log ("Set col-1 width: {0} at line {1}, chars={2}", w, largestIndex, content.Length);
         }
      }

      public void SetFilter (List<int> filter) {
         this.Filter = filter;
         base.RowCount = filter==null ? lf.PartialLineCount: filter.Count;
      }

      protected override void OnFontChanged (EventArgs e) {
         base.OnFontChanged (e);
         updateColumnWidths ();
         Invalidate ();
      }

      public override int GridRowToRow (int row) {
         return (filter != null && row >= 0 && row < filter.Count) ? filter[row] : row;
      }
      public override int RowToGridRow (int row, bool returnGE=false) {
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
               int lineNo = lf.GetOptRealLineNumber (row);
               if (lineNo >= 0) ret.Text = Invariant.Format ("{0}", lineNo);
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



   //public class LinesGrid: RawGrid {
   //   private static readonly String def;

   //   public LinesGrid() {
   //      using (var c  = Columns) {
   //         c.Add (new Bitmanager.Grid.Column (100));
   //         c.Add (new Bitmanager.Grid.Column (250));
   //      }
         
   //   }

   //   //protected override Cell GetCell (int row, int col) {
   //   //   Cell cell = CreateCell (col);
   //   //   if (col == 0) {
   //   //      cell.Text = row.ToString ();
   //   //   } else {
   //   //      cell.Text = def;
   //   //   }
   //   //   return cell;
   //   //}

   //   static LinesGrid () {
   //      String space = new string (' ', 32);
   //      var sb = new StringBuilder ();
   //      for (int i = 0; i < 1000; i++) {
   //         sb.Append ('|');
   //         sb.Append (10 * i);
   //         int fill = 10 - sb.Length % 10;
   //         sb.Append (space, 0, fill);
   //      }
   //      def = sb.ToString();
   //   }
   //}
}
