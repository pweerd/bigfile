﻿using Bitmanager.Core;
using DynamicGrid.Buffers;
using DynamicGrid.Data;
using DynamicGrid.Interop;
using DynamicGrid.Managers;
using DynamicGrid.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicGrid {
   /// <summary>
   /// Defines a base class for high performance grid controls.
   /// </summary>
   [System.ComponentModel.DesignerCategory ("")]
   public class RawGrid : Control {
      protected readonly HScrollBar hScrollBar;
      protected readonly VScrollBar vScrollBar;

      private readonly Graphics _graphics;
      private readonly IntPtr _graphicsHdc;
      private readonly CellBuffer _cellBuffer;
      private readonly DisplayBuffer _displayBuffer;
      private readonly FontManager _fontManager;

      private Rectangle _invalidDataRegion = Rectangle.Empty;
      private Point _mousePosition;
      private (int Row, int Column) _mouseCell;
      private bool _isMouseOverControl;
      private bool _isMouseOverGrid;
      private bool _isMouseDownOverGrid;

      private readonly List<ColumnPlacement> _columns;
      private readonly List<SectorPlacement> _sectors;

      private int width, height; //without scrollbars
      private int rowCount;
      private int maxLineWidth;
      public static readonly Logger logger = Logs.CreateLogger("bigfile", "grid");



      public RawGrid () {
         _columns = new ();
         _sectors = new ();
         hScrollBar = new HScrollBar ();
         hScrollBar.Dock = DockStyle.Bottom;
         vScrollBar = new VScrollBar ();
         vScrollBar.Dock = DockStyle.Right;
         _graphics = CreateGraphics ();
         _graphicsHdc = _graphics.GetHdc ();
         _fontManager = new FontManager ();
         _cellBuffer = new CellBuffer ();
         _displayBuffer = new DisplayBuffer (_graphicsHdc, _fontManager);

         Font = new Font ("Microsoft Sans Serif", 12);
         BackColor = Color.LightGray;

         _fontManager.Load (Font);
         hScrollBar.Scroll += handleScroll_Scroll;
         vScrollBar.Scroll += handleScroll_Scroll;
         Controls.Add (hScrollBar);
         Controls.Add (vScrollBar);
      }
      protected override void OnResize (EventArgs e) {
         base.OnResize (e);
         width = Width;
         if (vScrollBar.Visible) width -= vScrollBar.Width;
         height = Height;
         if (hScrollBar.Visible) height -= hScrollBar.Height;
         logger.Log ("OnResize: h={0} ({1}), w={2} ({3})", height, Height, width, Width);
         recomputeVScrollBar ();
      }

      private void handleScroll_Scroll (object sender, ScrollEventArgs e) {
         if (e.ScrollOrientation == ScrollOrientation.VerticalScroll) {
            this.VerticalOffset = (long)(e.NewValue * vScrollBarMultiplier);
         } else {
            this.HorizontalOffset = (long)(e.NewValue * hScrollBarMultiplier);
         }
         logger.Log ("e.scroll={0}, orient={1}, offset={2}", e.NewValue, e.ScrollOrientation, VerticalOffset);
      }

      public int RowCount { get { return rowCount; }
         set {
            rowCount = value < 0 ? 0: value;
            recomputeVScrollBar ();
            logger.Log ("min={0}, max={1}, multiplier={2}", vScrollBar.Minimum, vScrollBar.Maximum, vScrollBarMultiplier);

         }
      }

      double hScrollBarMultiplier=1;
      double vScrollBarMultiplier;
      private void recomputeVScrollBar() {
         const int MAX_VALUE = (int.MaxValue - 10000) & ~16;
         vScrollBarMultiplier = 1.0;
         int max = MAX_VALUE; 
         int rowHeight = RowHeight;
         int rowsPerWindow = height / rowHeight;

         long neededPixels = (rowHeight * (long)rowCount) - height;
         logger.Log ("Needed pixels (H): {0}, c={1}, rh={2}", neededPixels, rowCount, rowHeight);
         if (neededPixels <= 0) {
            vScrollBar.Enabled = false;
            return;
         }
         neededPixels += rowHeight / 2;
         vScrollBar.Enabled = true;
         if (neededPixels < max)
            max = (int)neededPixels;
         else {
            vScrollBarMultiplier = (neededPixels) / (double)max;
         }
         vScrollBar.SmallChange = (int)(.99999 + rowHeight / vScrollBarMultiplier);
         vScrollBar.LargeChange = (int)(((rowsPerWindow - 1) * rowHeight) / vScrollBarMultiplier);
         logger.Log ("Max (H): {0}, mult={1}", max, vScrollBarMultiplier);
         vScrollBar.Maximum = max + vScrollBar.LargeChange;
      }

      /// <summary>
      /// Gets or sets column widths. The provided collection is also being used to determine the number of columns to be displayed.
      /// </summary>
      public IEnumerable<int> Columns {
         get => _columns.Select (c => c.Width);
         set {
            ColumnPlacementResolver.CalculatePlacement (value, Width, _columns, _sectors);

            UpdateVisibleColumns ();
            ResizeBuffers ();
            RefreshData ();
            Refresh ();
            ProcessMouseEvents ();

            ColumnsChanged?.Invoke (this, EventArgs.Empty);

            DumpColumns ();
         }
      }

      private void DumpColumns () {
         logger.Log ("Dumping sectors:");
         for (int i = 0; i < _sectors.Count; i++) {
            var sector = _sectors[i];
            logger.Log ("-- [{0}]: w={1}, offs={2}", i, sector.Width, sector.Offset);
         }
         logger.Log ("Dumping columns:");
         for (int i = 0; i < _columns.Count; i++) {
            var c = _columns[i];
            logger.Log ("-- [{0}]: w={1}, offs={2}, sect={3}, sectIx={4}, sectOffs={5}", i, c.Width, c.GlobalOffset, c.Sector, c.SectorIndex, c.SectorOffset);
         }

      }


      private long _horizontalOffset;
      /// <summary>
      /// Horizontal offset applied to the content of the control.
      /// It's highly recommended to use this property over placing the <c>Grid</c> in a scrollable panel. This way the grid can avoid processing and rendering of invisible columns (columns that are out of the scrolling area).
      /// </summary>
      public long HorizontalOffset {
         get => _horizontalOffset;
         set {
            if (_horizontalOffset == value) return;
            _horizontalOffset = value;

            UpdateVisibleColumns ();
            UpdateData ();
            Refresh ();
            ProcessMouseEvents ();

            HorizontalOffsetChanged?.Invoke (this, EventArgs.Empty);
         }
      }

      private long _verticalOffset;
      /// <summary>
      /// Vertical offset applied to the content of the control.
      /// It's highly recommended to use this property over placing the <c>Grid</c> in a scrollable panel. This way the grid can avoid processing and rendering of invisible rows (rows that are out of the scrolling area).
      /// </summary>
      public long VerticalOffset {
         get => _verticalOffset;
         set {
            if (_verticalOffset == value) return;
            _verticalOffset = value;

            UpdateVisibleRows ();
            UpdateData ();
            Invalidate ();
            Refresh ();
            ProcessMouseEvents ();

            VerticalOffsetChanged?.Invoke (this, EventArgs.Empty);
         }
      }

      /// <summary>
      /// Height of a single row. This height includes a cell height and one extra pixel reserved for a bottom border.
      /// </summary>
      /// <remarks>
      /// This is a read only property. The height of a cell is derived directly from the font size and can by altered by changing it.
      /// </remarks>
      public int RowHeight => _fontManager.FontHeight + 1;

      /// <summary>
      /// A <c>Rectangle</c> representing column and row indexes visible within the current working area of the grid.
      /// </summary>
      /// <remarks>
      /// The value of this property can be affected by both the size of the control, as well as applied <see cref="HorizontalOffset"/> and <see cref="VerticalOffset"/>.
      /// </remarks>
      public Rectangle VisibleCells => new Rectangle (
         VisibleColumns.MinColumn,
         VisibleRows.MinRow,
         VisibleColumns.MaxColumn - VisibleColumns.MinColumn + 1,
         VisibleRows.MaxRow - VisibleRows.MinRow + 1);

      /// <summary>
      /// An (inclusive) range of column indexes that are visible within the current working area of the grid.
      /// </summary>
      /// <remarks>
      /// Range of visible columns is directly affected by the control width, as well as configured <see cref="Columns"/> and the applied <see cref="HorizontalOffset"/>.
      /// </remarks>
      public (int MinColumn, int MaxColumn) VisibleColumns { get; private set; }
      private void UpdateVisibleColumns () {
         var oldMinColumn = VisibleColumns.MinColumn;
         var newMinColumn = 0;
         while (newMinColumn < _columns.Count - 1 && _columns[newMinColumn].GlobalOffsetPlusWidth <= HorizontalOffset)
            newMinColumn++;

         var oldMaxColumn = VisibleColumns.MaxColumn;
         var newMaxColumn = newMinColumn;
         while (newMaxColumn < _columns.Count - 1 && _columns[newMaxColumn].GlobalOffsetPlusWidth < HorizontalOffset + Width)
            newMaxColumn++;

         VisibleColumns = (newMinColumn, newMaxColumn);

         for (int c = newMinColumn; c <= newMaxColumn && c < oldMinColumn; c++) {
            _cellBuffer.ClearColumn (_cellBuffer.CropRow (c));
            _displayBuffer.ClearColumn (BackColor, _columns[c].SectorOffset + _sectors[_columns[c].Sector].Offset, _columns[c].Width);
            InvalidateColumnData (c);
         }
         for (int c = newMaxColumn; c >= newMinColumn && c > oldMaxColumn; c--) {
            _cellBuffer.ClearColumn (_cellBuffer.CropRow (c));
            _displayBuffer.ClearColumn (BackColor,_columns[c].SectorOffset + _sectors[_columns[c].Sector].Offset, _columns[c].Width);
            InvalidateColumnData (c);
         }
      }

      /// <summary>
      /// An (inclusive) range of row indexes that are visible within the current working area of the grid.
      /// </summary>
      /// <remarks>
      /// Range of visible rows is directly affected by the control height, as well as the configured <see cref="RowHeight"/> and the applied <see cref="HorizontalOffset"/>.
      /// </remarks>
      public (int MinRow, int MaxRow) VisibleRows { get; private set; }
      private void UpdateVisibleRows () {
         var oldMinRow = VisibleRows.MinRow;
         var newMinRow = VerticalOffset >= 0
            ? VerticalOffset / RowHeight
            : (VerticalOffset + 1) / RowHeight - 1;

         var oldMaxRow = VisibleRows.MaxRow;
         var newMaxRow = VerticalOffset + Height > 0
            ? (VerticalOffset + Height - 1) / RowHeight
            : (VerticalOffset + Height) / RowHeight - 1;

         VisibleRows = ((int)newMinRow, (int)newMaxRow);

         for (int r = (int)newMinRow; r <= newMaxRow && r < oldMinRow; r++)
            InvalidateRowData (r);
         for (int r = (int)newMaxRow; r >= newMinRow && r > oldMaxRow; r--)
            InvalidateRowData (r);
      }

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
      /// <remarks>
      /// When the data invalidation occurs, this method will be called only for cells within the <see cref="VisibleCells"/> region.
      /// </remarks>
      protected virtual Cell GetCell (int rowIndex, int columnIndex) => Cell.Empty;

      private void ResizeBuffers () {
         if (!_columns.Any ()) return;

         var rows = Height / RowHeight + 2;

         _cellBuffer.SetDimensions (rows, _columns.Max (p => p.SectorIndex + 1));
         _displayBuffer.SetDimensions (rows * RowHeight, _sectors.Max (p => p.Offset + p.Width));
         InvalidateBuffers ();
      }

      private void InvalidateBuffers () {
         _cellBuffer.Clear ();
         _displayBuffer.Clear (BackColor);

         InvalidateData ();
      }

      /// <summary>
      /// Invalidates content and layout of all cells within the <see cref="VisibleCells"/> region.
      /// </summary>
      /// <seealso cref="UpdateData"/>
      public void InvalidateData () {
         var (minColumn, maxColumn) = VisibleColumns;
         var (minRow, maxRow) = VisibleRows;

         InvalidateData (minRow, maxRow, minColumn, maxColumn);
      }

      /// <summary>
      /// Invalidates content and layout of all visible cells within a given row.
      /// </summary>
      /// <param name="row">Index of a row to be invalidated.</param>
      /// <seealso cref="UpdateData"/>
      public void InvalidateRowData (int row) {
         var (minColumn, maxColumn) = VisibleColumns;

         InvalidateData (row, row, minColumn, maxColumn);
      }

      /// <summary>
      /// Invalidates content and layout of all visible cells within a given column.
      /// </summary>
      /// <param name="column">Index of a column to be invalidated.</param>
      /// <seealso cref="UpdateData"/>
      public void InvalidateColumnData (int column) {
         var (minRow, maxRow) = VisibleRows;

         InvalidateData (minRow, maxRow, column, column);
      }

      /// <summary>
      /// Invalidates the content and layout of a specific cell.
      /// </summary>
      /// <param name="row">Row index of a cell to be invalidated.</param>
      /// <param name="column">Column index of a cell to be invalidated.</param>
      /// <seealso cref="UpdateData"/>
      public void InvalidateCellData (int row, int column) {
         InvalidateData (row, row, column, column);
      }

      /// <summary>
      /// Invalidates content and layout of all visible cells within the specified region
      /// </summary>
      /// <seealso cref="UpdateData"/>
      public void InvalidateData (int minRow, int maxRow, int minColumn, int maxColumn) {
         var region = new Rectangle (
            minColumn,
            minRow,
            maxColumn - minColumn + 1,
            maxRow - minRow + 1);

         _invalidDataRegion = RectangleUtils.Union (
            Rectangle.Intersect (VisibleCells, _invalidDataRegion),
            Rectangle.Intersect (VisibleCells, region));
      }

      /// <summary>
      /// Causes the grid to fetch (using the <see cref="GetCell(int, int)"/> method) the current content and layout of invalidated cells and paints those cells on an internal buffer if changed.
      /// </summary>
      /// <remarks>
      /// Execution of this method will result in an invalidation of a control region affected by the data update. The visual changed can be later flushed to the screen using the <see cref="Control.Update"/> method or by waiting for the next draw cycle.
      /// This operation clears the accumulated data invalidation region.
      /// </remarks>
      public void UpdateData () {
         if (IsDisposed) return;

         _invalidDataRegion = Rectangle.Intersect (VisibleCells, _invalidDataRegion);

         if (_invalidDataRegion.IsEmpty) return;
         if (_columns.Count == 0) return;

         var (minRow, maxRow) = VisibleRows;
         var (minColumn, maxColumn) = VisibleColumns;

         minColumn = Math.Max (minColumn, _invalidDataRegion.Left);
         maxColumn = Math.Min (maxColumn, _invalidDataRegion.Right - 1);
         minRow = Math.Max (minRow, _invalidDataRegion.Top);
         maxRow = Math.Min (maxRow, _invalidDataRegion.Bottom - 1);

         _invalidDataRegion = Rectangle.Empty;

         CellRenderingContext renderingContext = new CellRenderingContext ();

         for (int rowIndex = minRow; rowIndex <= maxRow; rowIndex++)
            for (int columnIndex = minColumn; columnIndex <= maxColumn; columnIndex++)
               UpdateCellData (rowIndex, columnIndex, GetCell (rowIndex, columnIndex), ref renderingContext);

         Invalidate (renderingContext.InvalidatedRect);
      }

      private void UpdateCellData (int rowIndex, int columnIndex, in Cell cell, ref CellRenderingContext renderingContext) {
         var croppedRowIndex = _cellBuffer.CropRow (rowIndex);
         var croppedColumnIndex = _columns[columnIndex].SectorIndex;
         var changed = _cellBuffer.TrySet (croppedRowIndex, croppedColumnIndex, in cell);

         if (!changed) return;

         long rowHeight = RowHeight;
         var size = new Size (
            _columns[columnIndex].Width - 1,
            (int)rowHeight - 1);
         var realPosition = new Point (
            _columns[columnIndex].GlobalOffset - (int)HorizontalOffset,
            (int)(rowHeight * rowIndex - VerticalOffset));
         var croppedPosition = new Point (
            _columns[columnIndex].SectorOffset + _sectors[_columns[columnIndex].Sector].Offset,
            (int)(rowHeight * croppedRowIndex));
         var realRectangle = new Rectangle (realPosition, size);
         var croppedRectangle = new Rectangle (croppedPosition, size);

         _displayBuffer.setBackColor (cell.BackgroundColor ?? BackColor);
         _displayBuffer.setForeColor (cell.ForegroundColor ?? ForeColor);
         _displayBuffer.setTextAlignment (cell.TextAlignment);
         Gdi32.PrintText (_displayBuffer.Hdc, croppedRectangle, cell.TextAlignment, cell.Text);

         renderingContext.InvalidatedRect = RectangleUtils.Union (renderingContext.InvalidatedRect, realRectangle);
      }

      /// <summary>
      /// Calls the <see cref="InvalidateData"/> and <see cref="UpdateData"/> methods in succession.
      /// </summary>
      public void RefreshData () {
         InvalidateData ();
         UpdateData ();
      }

      protected override void OnSizeChanged (EventArgs e) {
         base.OnSizeChanged (e);

         UpdateVisibleRows ();

         Columns = Columns.ToList ();
      }

      protected override void OnPaint (PaintEventArgs e) {
         //protected void OnPaint2 (PaintEventArgs e) {
         long virtualX = e.ClipRectangle.X + HorizontalOffset;
         long virtualY = e.ClipRectangle.Y + VerticalOffset;
         long virtualXEnd = virtualX + e.ClipRectangle.Width;

         long rh = RowHeight;
         int rowStart = (int)(virtualY / rh);
         int rowEnd = 1+(int)((virtualY + e.ClipRectangle.Width) / rh);

         int colStart = 0;
         for (; colStart < _columns.Count && _columns[colStart].GlobalOffsetPlusWidth <= virtualX; colStart++) ;
         int colEnd = _columns.Count;
         for (; colEnd > colStart && _columns[colEnd-1].GlobalOffset > virtualXEnd; colEnd--) ;

         //logger.Log ("clip=({0}, {1}), ({2}, {3})", e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height);
         _displayBuffer.SetDimensions (e.ClipRectangle.Height, e.ClipRectangle.Width);
         _displayBuffer.Clear (BackColor);
         //logger.Log ("rows=({0}, {1}), cols=({2}, {3})", rowStart, rowEnd, colStart, colEnd);

         Rectangle bufRect = new Rectangle (0, 0, 0, (int)rh);
         for (int col = colStart; col < colEnd; col++) {
            var column = _columns[col];
            int x = (int)(column.GlobalOffset - virtualX);
            bufRect.X = x;
            bufRect.Width = column.Width;

            int y=(int)(rowStart * rh - virtualY);
            for (int row = rowStart; row < rowEnd; row++) {
               logger.Log ("-- getCell ({0}, {1}", row, col);
               logger.Log ("-- cellRect=({0}, {1}), ({2}, {3})", bufRect.X, bufRect.Y, bufRect.Width, bufRect.Height);
               Cell cell = GetCell (row, col);
               //_displayBuffer.ClearRect (cell.BackgroundColor ?? this.BackColor, bufRect);
               bufRect.Y = y;
               _displayBuffer.PrintCell (
                  cell.BackgroundColor ?? this.BackColor,
                  cell.ForegroundColor ?? this.ForeColor,
                  bufRect,
                  cell.FontStyle,
                  cell.TextAlignment,
                  cell.Text
               );
               y += (int)rh;
            }
         }

         //Gdi32.Copy (_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
         Gdi32.BitBlt (_graphicsHdc, e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height, _displayBuffer.Hdc, 0, 0, Gdi32.TernaryRasterOperations.SRCCOPY);
      }

      protected override void OnPaintBackground (PaintEventArgs e) { }

      protected override void OnBackColorChanged (EventArgs e) {
         base.OnBackColorChanged (e);

         InvalidateBuffers ();
      }

      protected override void OnFontChanged (EventArgs e) {
         base.OnFontChanged (e);

         _fontManager.Load (Font);

         UpdateVisibleRows ();
         ResizeBuffers ();
         RefreshData ();
         Refresh ();
         ProcessMouseEvents ();
      }

      protected override void OnClick (EventArgs e) {
         base.OnClick (e);

         if (_isMouseOverGrid)
            OnCellClicked (CreateMouseCellEventArgs ());
      }

      protected override void OnDoubleClick (EventArgs e) {
         base.OnDoubleClick (e);

         if (_isMouseOverGrid)
            OnCellDoubleClicked (CreateMouseCellEventArgs ());
      }

      protected override void OnMouseDown (MouseEventArgs e) {
         base.OnMouseDown (e);

         if (_isMouseOverGrid) {
            _isMouseDownOverGrid = true;
            OnMouseDownOverCell (CreateMouseCellEventArgs ());
         }
      }

      protected override void OnMouseUp (MouseEventArgs e) {
         base.OnMouseUp (e);

         if (_isMouseDownOverGrid) {
            _isMouseDownOverGrid = false;
            OnMouseUpOverCell (CreateMouseCellEventArgs ());
            ProcessMouseEvents ();
         }
      }

      protected override void OnMouseMove (MouseEventArgs e) {
         base.OnMouseMove (e);

         _mousePosition = e.Location;
         ProcessMouseEvents ();
      }

      protected override void OnMouseEnter (EventArgs e) {
         base.OnMouseEnter (e);

         _isMouseOverControl = true;
      }

      protected override void OnMouseLeave (EventArgs e) {
         base.OnMouseLeave (e);

         _isMouseOverControl = false;
         ProcessMouseEvents ();
      }

      private void ProcessMouseEvents () {
         if (!_isMouseOverControl && !_isMouseOverGrid) return;

         var x = _mousePosition.X + HorizontalOffset;
         var y = _mousePosition.Y + VerticalOffset;

         var oldMouseCell = _mouseCell;
         var oldIsMouseOverGrid = _isMouseOverGrid;

         _mouseCell = (
            (int)(y >= 0 ? y / RowHeight : (y - RowHeight + 1) / RowHeight),
            ColumnPlacementResolver.GetColumnIndex (_columns, (int)x, _mouseCell.Column));
         _isMouseOverGrid =
            _isMouseDownOverGrid ||
            _isMouseOverControl &&
            _columns.Count > 0 &&
            x >= 0 &&
            x < _columns[_columns.Count - 1].GlobalOffsetPlusWidth;

         var mouseCellChanged = _mouseCell != oldMouseCell;
         var isMouseOverGridChanged = _isMouseOverGrid != oldIsMouseOverGrid;

         switch ((_isMouseDownOverGrid, _isMouseOverGrid, isMouseOverGridChanged, mouseCellChanged)) {
            case (true, _, _, true):
            case (false, true, false, true):
               OnMouseMovedOverGrid (CreateMouseCellEventArgs ());
               break;
            case (false, true, true, _):
               OnMouseEnteredGrid (EventArgs.Empty);
               break;
            case (false, false, true, _):
               OnMouseLeftGrid (EventArgs.Empty);
               break;
         };
      }

      private MouseCellEventArgs CreateMouseCellEventArgs () {
         var cellRect = new Rectangle (
            (int)(_columns[_mouseCell.Column].GlobalOffset - HorizontalOffset),
            (int)(_mouseCell.Row * (long)RowHeight - VerticalOffset),
            _columns[_mouseCell.Column].Width,
            RowHeight);

         return new MouseCellEventArgs (_mouseCell.Row, _mouseCell.Column, MouseButtons, cellRect);
      }

      /// <summary>
      /// Raises the <see cref="CellClicked"/> event
      /// </summary>
      protected virtual void OnCellClicked (MouseCellEventArgs e) => CellClicked?.Invoke (this, e);
      /// <summary>
      /// Raises the <see cref="CellDoubleClicked"/> event
      /// </summary>
      protected virtual void OnCellDoubleClicked (MouseCellEventArgs e) => CellDoubleClicked?.Invoke (this, e);
      /// <summary>
      /// Raises the <see cref="MouseDownOverCell"/> event
      /// </summary>
      protected virtual void OnMouseDownOverCell (MouseCellEventArgs e) => MouseDownOverCell?.Invoke (this, e);
      /// <summary>
      /// Raises the <see cref="MouseUpOverCell"/> event
      /// </summary>
      protected virtual void OnMouseUpOverCell (MouseCellEventArgs e) => MouseUpOverCell?.Invoke (this, e);
      /// <summary>
      /// Raises the <see cref="MouseMovedOverGrid"/> event
      /// </summary>
      protected virtual void OnMouseMovedOverGrid (MouseCellEventArgs e) => MouseMovedOverGrid?.Invoke (this, e);
      /// <summary>
      /// Raises the <see cref="MouseEnteredGrid"/> event
      /// </summary>
      protected virtual void OnMouseEnteredGrid (EventArgs e) => MouseEnteredGrid?.Invoke (this, e);
      /// <summary>
      /// Raises the <see cref="MouseLeftGrid"/> event
      /// </summary>
      protected virtual void OnMouseLeftGrid (EventArgs e) => MouseLeftGrid?.Invoke (this, e);

      public event EventHandler<EventArgs> ColumnsChanged;
      public event EventHandler<EventArgs> HorizontalOffsetChanged;
      public event EventHandler<EventArgs> VerticalOffsetChanged;
      public event EventHandler<MouseCellEventArgs> CellClicked;
      public event EventHandler<MouseCellEventArgs> CellDoubleClicked;
      public event EventHandler<MouseCellEventArgs> MouseDownOverCell;
      public event EventHandler<MouseCellEventArgs> MouseUpOverCell;
      public event EventHandler<MouseCellEventArgs> MouseMovedOverGrid;
      public event EventHandler<EventArgs> MouseEnteredGrid;
      public event EventHandler<EventArgs> MouseLeftGrid;
   }
}
