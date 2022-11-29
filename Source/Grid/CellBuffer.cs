using System.Diagnostics;
using System.Drawing;

namespace Bitmanager.Grid {
   internal sealed class CellBuffer {
      private Cell[,] _cells;
      private int _maxRows, _numRows;
      private int _maxCols, _numCols;

      public CellBuffer () {

         _cells = new Cell[0, 0];
      }

      public void SetDimensions (int rows, int cols) {
         if (rows > _maxRows || cols > _maxCols) {
            _maxRows = Math.Max (rows, _maxRows * 2);
            _maxCols = Math.Max (cols, _maxCols * 2);
            _cells = new Cell[_maxRows, _maxCols];
         }
         _numRows = rows;
         _numCols = cols;
      }

      public void Clear () {
         for (var y = 0; y < _numRows; y++)
            for (var x = 0; x < _numCols; x++)
               _cells[y, x] = Cell.Empty;
      }

      public void ClearColumn (int index) {
         if (index < 0) return;
         if (index >= _numCols) return;

         for (var y = 0; y < _numRows; y++)
            _cells[y, index] = Cell.Empty;
      }

      public bool TrySet (int row, int column, in Cell value) {
         Debug.Assert (column >= 0);
         Debug.Assert (column < _numCols);
         Debug.Assert (row >= 0);
         Debug.Assert (row < _numRows);

         ref var cell = ref _cells[row, column];
         var changed = cell != value;

         cell = value;

         return changed;
      }

      public int CropRow (int index) {
         return (index % _numRows + _numRows) % _numRows;
      }
   }
}