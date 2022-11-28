using DynamicGrid.Interop;
using DynamicGrid.Managers;
using System;
using System.Drawing;

namespace DynamicGrid.Buffers {
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
      public void SetDimensions (int h, int w) {
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
         Gdi32.Fill (Hdc, new Rectangle (0, 0, Width, Height));
      }

      public void ClearColumn (Color color, int offset, int width) {
         setBackColor (color);
         Gdi32.Fill (Hdc, new Rectangle (offset, 0, width, Height));
      }
      public void ClearRect (Color color, Rectangle rect) {
         int overflow;
         overflow = rect.Right - Width;
         if (overflow > 0) rect.Width = rect.Width - overflow;
         overflow = rect.Bottom - Height;
         if (overflow > 0) rect.Height = rect.Height - overflow;
         if (rect.Width > 0 && rect.Height > 0) {
            setBackColor (color);
            Gdi32.Fill (Hdc, rect);
         }
      }

      public void PrintCell (Color backColor, Color foreColor, Rectangle rect, FontStyle fontStyle, HorizontalAlignment textAlignment, string text) {
         int x = rect.X;
         switch (textAlignment) {
            case HorizontalAlignment.Right:
               x = rect.Right; break;
            case HorizontalAlignment.Center:
               x = (x + rect.Right) / 2; break;
         }
         int overflow;
         overflow = rect.Right - Width;
         if (overflow > 0) rect.Width = rect.Width - overflow;
         overflow = rect.Bottom - Height;
         if (overflow > 0) rect.Height = rect.Height - overflow;

         if (rect.Width > 0 && rect.Height > 0) {
            setBackColor (backColor);
            setForeColor (foreColor);
            setFontStyle (fontStyle);
            setTextAlignment (textAlignment);
            Gdi32.Fill (Hdc, rect);
            Gdi32.PrintText (Hdc, rect, x, text);
         }
      }
   }
}
