using System;
using System.Drawing;
using static Bitmanager.Grid.Gdi32;

namespace Bitmanager.Grid {
   internal sealed class DisplayBuffer : IDisposable {
      private readonly IntPtr _parentHdc;
      private readonly FontManager _fontManager;


      private BufferedGraphics _buffer;
      public IntPtr Hdc { get; private set; }

      private int _maxHeight, _maxWidth;

      public int Width { get; private set; }
      public int Height { get; private set; }


      private Color _backColor;
      private Color _foreColor;
      private HorizontalAlignment _textAlignment;
      private FontStyle _fontStyle;


      public DisplayBuffer (IntPtr parentHdc, FontManager fmgr) {
         _parentHdc = parentHdc;
         _fontManager = fmgr;
         _backColor = Color.White;
         _foreColor = Color.Black;
         _textAlignment = HorizontalAlignment.Left;
         _fontStyle = FontStyle.Regular;
      }
      public void Init (int h, int w) {
         if (h > _maxHeight || w > _maxWidth) {
            _maxHeight = Math.Max (h, _maxHeight * 2);
            _maxWidth = Math.Max (w, _maxWidth * 2);
            Dispose ();
            _buffer = BufferedGraphicsManager.Current.Allocate (_parentHdc, new Rectangle (0, 0, _maxWidth, _maxHeight));
            Hdc = _buffer.Graphics.GetHdc ();
            Gdi32.SetBackgroundColor (Hdc, _backColor);
            Gdi32.SetForegroundColor (Hdc, _foreColor);
            Gdi32.SetTextAlignemnt (Hdc, _textAlignment);
            Gdi32.SelectObject (Hdc, _fontManager.GetHdc (_fontStyle));
         }
         Height = h;
         Width = w;
         Gdi32.SelectObject (Hdc, _fontManager.GetHdc (_fontStyle));
      }

      public void setBackColor (Color c) {
         if (_backColor != c) {
            _backColor = c;
            Gdi32.SetBackgroundColor (Hdc, c);
         }
      }
      public void setForeColor (Color c) {
         if (_foreColor != c) {
            _foreColor = c;
            Gdi32.SetForegroundColor (Hdc, c);
         }
      }
      public void setTextAlignment (HorizontalAlignment textAlignment) {
         if (_textAlignment != textAlignment) {
            _textAlignment = textAlignment;
            Gdi32.SetTextAlignemnt (Hdc, textAlignment);
         }
      }

      public void setFontStyle (FontStyle fontStyle) {
         if (fontStyle != _fontStyle) {
            _fontStyle = fontStyle;
            Gdi32.SelectObject (Hdc, _fontManager.GetHdc (fontStyle));
         }
      }

      public void Dispose () {
         if (_buffer == null) return;

         _buffer.Graphics.ReleaseHdc (Hdc);
         _buffer.Dispose ();

         _buffer = null;
         Hdc = IntPtr.Zero;
      }

      public void Clear (Color color) {
         setBackColor (color);
         var rect = new GDIRECT (0, 0, Width, Height);
         Gdi32.Fill (Hdc, ref rect);
      }

      public void ClearColumn (Color color, int offset, int width) {
         setBackColor (color);
         var rect = new GDIRECT (offset, 0, Width, Height);
         Gdi32.Fill (Hdc, ref rect);
      }

      public void PrintCell (Cell cell, ref Rectangle rect) {
         int left = rect.Left + cell.HorizontalPadding.Left;
         int right = rect.Right - cell.HorizontalPadding.Right;
         int x;
         switch (cell.Alignment) {
            case HorizontalAlignment.Left:
               x = left; break;
            case HorizontalAlignment.Right:
               x = right; break;
            case HorizontalAlignment.Center:
               x = (left+right) / 2; break;
            default: throw new Exception("Unexpected Alignment: " + cell.Alignment);
         }

         if (rect.Width > 0 && rect.Height > 0) {
            var gdiRect = new GDIRECT (rect.X, rect.Y, rect.Right, rect.Bottom);
            setBackColor (cell.BackColor);
            setForeColor (cell.ForeColor);
            setFontStyle (cell.FontStyle);
            setTextAlignment (cell.Alignment);
            Gdi32.Fill (Hdc, ref gdiRect);

            gdiRect.Left += cell.HorizontalPadding.Left;
            gdiRect.Right -= cell.HorizontalPadding.Right;
            Gdi32.PrintText (Hdc, ref gdiRect, x, cell.Text);
         }
      }
   }
}
