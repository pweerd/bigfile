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

using Bitmanager.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static Bitmanager.Grid.Gdi32;

namespace Bitmanager.Grid {
   /// <summary>
   /// Defines a base class for high performance grid controls.
   /// </summary>
   [System.ComponentModel.DesignerCategory ("")]
   public class RawGrid : Control {
      public delegate Cell ON_GETCELL (object sender, int row, int col);
      public delegate void ON_SELECTED_INDEX_CHANGED (object sender, EventArgs e);
      public event ON_GETCELL OnGetCell;
      public event EventHandler SelectedIndexChanged;
      public event EventHandler RowCountChanged;
      protected readonly HScrollBar hScrollBar;
      protected readonly VScrollBar vScrollBar;

      private readonly Graphics _graphics;
      private readonly IntPtr _graphicsHdc;
      private readonly DisplayBuffer _displayBuffer;
      private FontManager _fontManager;
      private long _maxHorizontalOffset, _maxVerticalOffset;
      private long _horizontalOffset, _verticalOffset;

      protected int _selectedRow;
      protected int _focusRow;
      private int _rowCount;

      private List<InternalColumn> _columns;

      private int _clientWidth, _clientHeight; //without scrollbars
      public static readonly Logger logger = Logs.CreateLogger ("bigfile", "grid");



      public RawGrid () {
         DoubleBuffered = false;
         _columns = new ();
         hScrollBar = new HScrollBar ();
         hScrollBar.Dock = DockStyle.Bottom;
         vScrollBar = new VScrollBar ();
         vScrollBar.Dock = DockStyle.Right;
         _graphics = CreateGraphics ();
         _graphicsHdc = _graphics.GetHdc ();
         _fontManager = new FontManager ();
         _displayBuffer = new DisplayBuffer (_graphicsHdc, _fontManager);

         Font = new Font ("Microsoft Sans Serif", 12);
         BackColor = Color.LightGray;

         _fontManager.Load (Font);
         hScrollBar.Scroll += handleScroll;
         vScrollBar.Scroll += handleScroll;
         Controls.Add (hScrollBar);
         Controls.Add (vScrollBar);

         _selectedRow = -1;
         _focusRow = -1;
      }
      protected override void OnResize (EventArgs e) {
         _clientWidth = Width;
         if (vScrollBar.Visible) _clientWidth -= vScrollBar.Width;
         _clientHeight = Height;
         if (hScrollBar.Visible) _clientHeight -= hScrollBar.Height;
         logger.Log ("OnResize: h={0} ({1}), w={2} ({3})", _clientHeight, Height, _clientWidth, Width);
         RecomputeScrollBars ();
         Invalidate ();
         base.OnResize (e);
      }

      /// <summary>
      /// Resolves a grid row into a 'real' row-index. 
      /// This enabeles support for filering
      /// </summary>
      public virtual int GridRowToRow (int row) {
         return row;
      }

      /// <summary>
      /// Resolves a 'real' row into a grid row-index. 
      /// This enabeles support for filering
      /// </summary>
      public virtual int RowToGridRow (int row, bool returnGE = false) {
         return row;
      }

      public int FocusRow => _focusRow;
      public int SelectedIndex {
         get { return _selectedRow; }
         set {
            int ix;
            if (value < 0) ix = -1;
            else ix = (value >= RowCount) ? RowCount - 1 : value;
            if (ix != _selectedRow) {
               _selectedRow = ix;
               OnSelectedIndexChanged (ix);
               Invalidate ();
            }
         }
      }

      protected virtual void OnSelectedIndexChanged(int ix) {
         if (ix >= 0) _focusRow = ix;
         if (SelectedIndexChanged != null) {
            SelectedIndexChanged (this, EventArgs.Empty);
         }
      }


      public UpdateableColumns Columns => new UpdateableColumns (this, _columns);


      internal void CreateInternalColumns (List<Column> list) {
         List<InternalColumn> tmp = new List<InternalColumn> (list.Count);
         int offset = 0;
         for (int i = 0; i < list.Count; i++) {
            var c = list[i];
            tmp.Add (new InternalColumn (this, c, offset));
            offset += c.Width;
         }
         _columns = tmp;
         ColumnsChanged?.Invoke (this, EventArgs.Empty);
         //DumpColumns ();
      }

      protected virtual void OnRowCountChanged (int newCount) {
         if (RowCountChanged != null) RowCountChanged (this, EventArgs.Empty);
      }
      public int RowCount { get { return _rowCount; }
         set {
            if (_rowCount != value) {
               _rowCount = value < 0 ? 0 : value;
               _focusRow = _rowCount > 0 ? 0 : -1;
               OnRowCountChanged (_rowCount);
               RecomputeScrollBars ();
               Invalidate ();
            }
         }
      }

      public Rectangle GetCellBounds (int row, int col) {
         int rh = RowHeight;
         var c = _columns[col];
         int y = (int)((rh * (long)row) - _verticalOffset);
         return new Rectangle ((int)(c.GlobalOffset - _horizontalOffset), y, c.GlobalOffsetPlusWidth, rh);
      }

      double hScrollBarMultiplier=1;
      double vScrollBarMultiplier;

      private static int adjustMultiplierUp (double val, double multiplier, int max) {
         var ret =  (int)Math.Ceiling (val / multiplier);
         return ret < max ? ret : max - 1;
      }
      private static int adjustMultiplierDown (double val, double multiplier, int max) {
         var ret = (int) (.5 + val / multiplier);
         return ret < max ? ret : max - 1;
      }

      protected void RecomputeScrollBars() {
         const int MAX_VALUE = (int.MaxValue - 10000) & ~16;
         const int MAX_VALUE2 = int.MaxValue - 1;
         if (_clientHeight <= 0 || _clientWidth <= 0) {
            vScrollBar.Value = 0;
            hScrollBar.Value = 0;
            _verticalOffset = 0;
            _maxVerticalOffset = 0;
            _horizontalOffset = 0;
            _maxHorizontalOffset = 0;
            return;
         }

         //Reconstruct vertical scrollbar
         vScrollBarMultiplier = 1.0;
         int max = MAX_VALUE; 
         int rowHeight = RowHeight;
         int rowsPerWindow = _clientHeight / rowHeight;
         long neededPixels = Math.Max (0, (rowHeight * (long)_rowCount) - _clientHeight);
         _maxVerticalOffset = neededPixels;
         if (_verticalOffset > neededPixels) _verticalOffset = neededPixels;

         logger.Log ("Needed pixels (V): {0}, c={1}, rh={2}", neededPixels, _rowCount, rowHeight);
         if (_maxVerticalOffset <= 0) {
            vScrollBar.Enabled = false;
            goto SETUP_HORIZONTAL;
         }
         //neededPixels += rowHeight / 2;
         vScrollBar.Enabled = true;
         if (neededPixels < max)
            max = (int)neededPixels;
         else {
            vScrollBarMultiplier = (neededPixels) / (double)max;
         }

         int smallChange = rowHeight;
         int largeChange = rowsPerWindow * rowHeight;
         vScrollBar.Value = 0;
         vScrollBar.SmallChange = adjustMultiplierUp (smallChange, vScrollBarMultiplier, MAX_VALUE2);
         vScrollBar.LargeChange = adjustMultiplierDown (largeChange, vScrollBarMultiplier, MAX_VALUE2);
         vScrollBar.Maximum = adjustMultiplierUp (neededPixels + largeChange, vScrollBarMultiplier, MAX_VALUE2);
         logger.Log ("Max (V): {0}, mult={1}, small={2}, {3}, {4}, large={5}, {6}, {7}, max={8}, {9}, {10}", max, vScrollBarMultiplier,
            smallChange, vScrollBar.SmallChange, vScrollBarMultiplier * vScrollBar.SmallChange,
            largeChange, vScrollBar.LargeChange, vScrollBarMultiplier * vScrollBar.LargeChange,
            neededPixels, vScrollBar.Maximum, vScrollBarMultiplier * vScrollBar.Maximum);

      //Reconstruct horizontal bar
      SETUP_HORIZONTAL:
         hScrollBarMultiplier = 1.0;
         max = MAX_VALUE;
         neededPixels = Math.Max(0, _columns.Count == 0 ? 0 : _columns[^1].GlobalOffsetPlusWidth - _clientWidth);
         _maxHorizontalOffset = neededPixels;
         if (_horizontalOffset > neededPixels) _horizontalOffset = neededPixels;

         if (neededPixels <= 0) {
            hScrollBar.Enabled = false;
            goto EXIT_RTN;
         }
         hScrollBar.Enabled = true;
         if (neededPixels < max)
            max = (int)neededPixels;
         else {
            hScrollBarMultiplier = (neededPixels) / (double)max;
         }

         smallChange = _clientWidth / 10;
         largeChange = _clientWidth - 10;

         hScrollBar.Value = 0;
         hScrollBar.SmallChange = adjustMultiplierUp(smallChange, hScrollBarMultiplier, MAX_VALUE2);
         hScrollBar.LargeChange = adjustMultiplierDown (largeChange, hScrollBarMultiplier, MAX_VALUE2);
         hScrollBar.Maximum = adjustMultiplierUp (neededPixels + _clientWidth, hScrollBarMultiplier, MAX_VALUE2);
         logger.Log ("Max (H): {0}, mult={1}, small={2}, {3}, {4}, large={5}, {6}, {7}, max={8}, {9}, {10}", max, hScrollBarMultiplier,
            smallChange, hScrollBar.SmallChange, hScrollBarMultiplier * hScrollBar.SmallChange,
            largeChange, hScrollBar.LargeChange, hScrollBarMultiplier * hScrollBar.LargeChange,
            neededPixels, hScrollBar.Maximum, hScrollBarMultiplier * hScrollBar.Maximum);

      EXIT_RTN:
         logger.Log ("MaxVerticalOffset={0}, max height={1}, clientH={2} ({3} rowpixels)", _maxVerticalOffset, _rowCount*(long)rowHeight, _clientHeight, rowsPerWindow * rowHeight);
         logger.Log ("MaxHorizontalOffset={0}, max width={1}, clientW={2}", _maxHorizontalOffset, _columns[^1].GlobalOffsetPlusWidth, _clientWidth);
         return;
      }


      private void DumpColumns () {
         logger.Log ("Dumping columns:");
         for (int i = 0; i < _columns.Count; i++) {
            var c = _columns[i];
            logger.Log ("-- [{0}]: w={1}, offs={2}", i, c.Width, c.GlobalOffset);
         }

      }


      /// <summary>
      /// Horizontal offset applied to the content of the control.
      /// It's highly recommended to use this property over placing the <c>Grid</c> in a scrollable panel. This way the grid can avoid processing and rendering of invisible columns (columns that are out of the scrolling area).
      /// </summary>
      public long HorizontalOffset {
         get => _horizontalOffset;
         set {
            if (_horizontalOffset == value) return;
            _horizontalOffset = MathUtils.Clipped (value, _maxHorizontalOffset);
            updateScrollbarPosition (hScrollBar, _horizontalOffset / hScrollBarMultiplier);

            HorizontalOffsetChanged?.Invoke (this, EventArgs.Empty);
            Invalidate ();
         }
      }

      /// <summary>
      /// Vertical offset applied to the content of the control.
      /// It's highly recommended to use this property over placing the <c>Grid</c> in a scrollable panel. This way the grid can avoid processing and rendering of invisible rows (rows that are out of the scrolling area).
      /// </summary>
      public long VerticalOffset {
         get => _verticalOffset;
         set {
            if (_verticalOffset == value) return;
            _verticalOffset = MathUtils.Clipped(value, _maxVerticalOffset);
            updateScrollbarPosition (vScrollBar, _verticalOffset / vScrollBarMultiplier);

            VerticalOffsetChanged?.Invoke (this, EventArgs.Empty);
            Invalidate ();
         }
      }

      bool _updatingScrollbar;
      private void updateScrollbarPosition (ScrollBar sb, double pos) {
         _updatingScrollbar = true;
         try {
            int newValue = (int)(.5 + pos);
            if (newValue < sb.Minimum) newValue = sb.Minimum;
            else if (newValue > sb.Maximum) newValue = sb.Maximum;
            sb.Value = newValue;
         } finally {
            _updatingScrollbar = false;
         }
      }

      private void handleScroll (object sender, ScrollEventArgs e) {
         if (!_updatingScrollbar) {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll) {
               this.VerticalOffset = (long)(e.NewValue * vScrollBarMultiplier);
            } else {
               this.HorizontalOffset = (long)(e.NewValue * hScrollBarMultiplier);
            }
            //logger.Log ("e.scroll={0}, orient={1}, offset={2}", e.NewValue, e.ScrollOrientation, VerticalOffset);
         }
      }


      /// <summary>
      /// Height of a single row. This height includes a cell height and one extra pixel reserved for a bottom border.
      /// </summary>
      /// <remarks>
      /// This is a read only property. The height of a cell is derived directly from the font size and can by altered by changing it.
      /// </remarks>
      public int RowHeight => _fontManager.FontHeight + 1;


      protected override void Dispose (bool disposing) {
         if (disposing) {
            _graphics.Dispose ();
            _displayBuffer.Dispose ();
            _fontManager.Dispose ();
         }

         base.Dispose (disposing);
      }

      /// <summary>
      /// A virtual method to be overwritten by a deriving class to provides the cell content and layout for requested cell coordinates.
      /// </summary>
      protected virtual Cell GetCell (int row, int col) {
         return (OnGetCell != null) ? OnGetCell(this, row, col) : CreateCell (col);
      }

      public virtual Cell CreateCell(int col) {
         return col < _columns.Count ? new Cell (_columns[col]) : new Cell(this);
      }

      public void SetColumnWidth (int col, int width, bool refresh = true) {
         InternalColumn.SetColumnWidth (_columns, col, width);
         if (refresh) {
            RecomputeScrollBars ();
            Invalidate ();
         }
      }

      public int MeasureTextWidth (string txt) {
         if (string.IsNullOrEmpty (txt)) return 0;
         return Gdi32.Measure (_graphicsHdc, txt, _fontManager.GetHdc (FontStyle.Regular)).Width;
      }



      protected override void OnPaint (PaintEventArgs e) {
         long virtualX = HorizontalOffset;
         long virtualY = VerticalOffset;
         long virtualXEnd = virtualX + _clientWidth;

         long rh = RowHeight;
         int rowStart = (int)(virtualY / rh);
         int rowEnd = 1+(int)((virtualY + _clientHeight) / rh);
         if (rowEnd > _rowCount) rowEnd = _rowCount;

         int colStart = 0;
         for (; colStart < _columns.Count && _columns[colStart].GlobalOffsetPlusWidth <= virtualX; colStart++) ;
         int colEnd = _columns.Count;
         for (; colEnd > colStart && _columns[colEnd-1].GlobalOffset > virtualXEnd; colEnd--) ;

         //logger.Log ("clip=({0}, {1}), ({2}, {3})", e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height);
         _displayBuffer.Init (_clientHeight, _clientWidth);
         //logger.Log ("rows=({0}, {1}), cols=({2}, {3})", rowStart, rowEnd, colStart, colEnd);

         Rectangle bufRect = new Rectangle (0, 0, 0, (int)rh);
         for (int col = colStart; col < colEnd; col++) {
            var column = _columns[col];
            int x = (int)(column.GlobalOffset - virtualX);
            bufRect.X = x;
            bufRect.Width = column.OuterWidth;
            _displayBuffer.ClearColumn (column.EffectiveBackColor, x, column.OuterWidth);

            int y =(int)(rowStart * rh - virtualY);
            for (int row = rowStart; row < rowEnd; row++) {
//               logger.Log ("-- getCell ({0}, {1}", row, col);
//               logger.Log ("-- cellRect=({0}, {1}), ({2}, {3})", bufRect.X, bufRect.Y, bufRect.Width, bufRect.Height);
               Cell cell = GetCell (row, col);
               //_displayBuffer.ClearRect (cell.BackgroundColor ?? this.BackColor, bufRect);
               bufRect.Y = y;
               _displayBuffer.PrintCell (cell, ref bufRect);
               y += (int)rh;
            }
         }
         Gdi32.BitBlt (_graphicsHdc, 0, 0, _clientWidth, _clientHeight, _displayBuffer.Hdc, 0, 0, Gdi32.TernaryRasterOperations.SRCCOPY);
      }

      public void GotoCell (int row, int col=-1, bool select=true) {
         if (row >= 0) VerticalOffset = RowHeight * (long)row;
         if (col >= 0 && col < _columns.Count) HorizontalOffset = _columns[col].GlobalOffset;
         if (select) SelectedIndex = row;
         _focusRow = row;
         Invalidate ();
      }

      public void MakeCellVisible (int row, int col = -1, bool select = true) {
         int rowHeight = RowHeight;
         if (row < 0) row = 0;
         else if (row >= _rowCount) row = _rowCount - 1;

         long vOffset = rowHeight * (long)row;

         if (vOffset < _verticalOffset) {
            VerticalOffset = vOffset;
            goto SELECT;
         }
         if (vOffset + rowHeight - VerticalOffset > _clientHeight) {
            VerticalOffset = vOffset - (_clientHeight - rowHeight);
            goto SELECT;
         }

      SELECT:
         if (select) SelectedIndex = row;
         _focusRow = row;

         if (col < 0 ) goto EXIT_RTN;
         if (col >= _columns.Count) col = _columns.Count - 1;
         HorizontalOffset = _columns[col].GlobalOffset;

      EXIT_RTN:
         return;
      }

      protected override bool IsInputKey (Keys keyData) {
         bool ret;
         switch (keyData & Keys.KeyCode) {
            case Keys.Left:
            case Keys.Right:
            case Keys.Up:
            case Keys.Down:
            case Keys.PageUp:
            case Keys.PageDown:
            case Keys.Home:
            case Keys.End:
               ret = true; break;
            default: ret = base.IsInputKey (keyData); break;
         }
         //logger.Log ("Old IsInputKey ({0}) -> {1} (was {2})", keyData, ret, base.IsInputKey (keyData));
         return ret;
      }


      protected override void OnMouseWheel (MouseEventArgs e) {
         base.OnMouseWheel (e);
         int delta = 1; // e.Delta < 0 ? -1 : 1;
         int visibleRows = _clientHeight / RowHeight;
         int max = Math.Min (1, visibleRows / 2);
         if (RowCount > 1000) delta *= 3;
         else if (RowCount > 100) delta *= 2;

         if (max > delta) delta = max;
         if (e.Delta > 0) delta = -delta;

         VerticalOffset = Math.Min (_verticalOffset + delta * RowHeight, _maxVerticalOffset);
      }

      protected override void OnMouseDown (MouseEventArgs e) {
         base.OnMouseDown (e);
         int row = GetRowFromLocation (e.Y);
         if (row >= 0) _focusRow = row;
      }

      protected override void OnKeyDown (KeyEventArgs e) {
         int visibleRows;
         base.OnKeyDown (e);
         if (e.Handled || _rowCount==0 || _columns.Count==0) return;
         switch (e.KeyCode) {
            case Keys.Home:
               HorizontalOffset = 0;
               if (e.Control) VerticalOffset = 0;
               break;
            case Keys.End:
               if (e.Control) {
                  VerticalOffset = _maxVerticalOffset;
                  HorizontalOffset = 0;
               } else {
                  HorizontalOffset = _maxHorizontalOffset;
               }
               break;
            case Keys.Up:
               MakeCellVisible (_focusRow - 1, -1, true);
               break;
            case Keys.Down:
               MakeCellVisible (_focusRow + 1, -1, true);
               break;
            case Keys.PageUp:
               visibleRows = _clientHeight / RowHeight -1;
               MakeCellVisible (_focusRow - visibleRows, -1, true);
               break;
            case Keys.PageDown:
               visibleRows = _clientHeight / RowHeight - 1;
               MakeCellVisible (_focusRow + visibleRows, -1, true);
               break;
         }
      }
      protected override void OnKeyUp (KeyEventArgs e) {
         base.OnKeyUp (e);
      }
      protected override void OnKeyPress (KeyPressEventArgs e) {
         base.OnKeyPress (e);
      }
      protected override void OnPaintBackground (PaintEventArgs e) { }

      protected override void OnBackColorChanged (EventArgs e) {
         base.OnBackColorChanged (e);
         Invalidate ();
      }

      protected override void OnFontChanged (EventArgs e) {
         _fontManager.Load (Font);
         RecomputeScrollBars ();
         Invalidate ();
         base.OnFontChanged (e);
      }

      public Rectangle InnerClientRectangle {
         get {
            return new Rectangle (0, 0, _clientWidth, _clientHeight);
         }
      }


      /// <summary>
      /// Returns the row based on relative coordinates, or -1 if the coordinates do not point in a row
      /// </summary>
      public int GetRowFromLocation (int y) {
         int row = (int)((_verticalOffset + y) / RowHeight);
         if (row < 0) row = -1;
         else if (row >= RowCount) row = RowCount - 1;
         return row;
      }

      /// <summary>
      /// Returns the row/col based on relative coordinates, or -1 if the coordinates do not point in a row
      /// </summary>
      public int GetRowAndColFromLocation (int x, int y, out int col) {
         long virtualX = x + HorizontalOffset;
         for (int i = 0; i < _columns.Count; i++) {
            var c = _columns[i];
            if (virtualX >= c.GlobalOffsetPlusWidth) continue;
            if (virtualX < c.GlobalOffset) continue;
            col = i;
            goto EXIT_RTN;
         }
         col = -1;

      EXIT_RTN:
         return GetRowFromLocation (y);
      }


      /// <summary>
      /// Returns the row based on the current mouse location, or -1 if the coordinates do not point in a row
      /// </summary>
      public int GetRowFromLocation () {
         Point p = PointToClient (MousePosition);
         return (p.X >= 0 && p.X < _clientWidth && p.Y >= 0 && p.Y < _clientHeight) ? GetRowFromLocation (p.Y) : -1;
      }
      /// <summary>
      /// Returns the row/col based on relative coordinates, or -1 if the coordinates do not point in a row
      /// </summary>
      public int GetRowAndColFromLocation (out int col) {
         Point p = PointToClient (MousePosition);
         if (p.X >= 0 && p.X < _clientWidth && p.Y >= 0 && p.Y < _clientHeight) return GetRowAndColFromLocation (p.X, p.Y, out col);
         col = -1;
         return -1;
      }



      public event EventHandler<EventArgs> ColumnsChanged;
      public event EventHandler<EventArgs> HorizontalOffsetChanged;
      public event EventHandler<EventArgs> VerticalOffsetChanged;
   }
}
