using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Bitmanager.Grid {
   public static class Gdi32 {
      public static void SetBackgroundColor (IntPtr hdc, Color color) {
         var value = (((color.B << 8) | color.G) << 8) | color.R;
         SetBkColor (hdc, value);
      }
      public static void SetForegroundColor (IntPtr hdc, Color color) {
         var value = (((color.B << 8) | color.G) << 8) | color.R;
         SetTextColor (hdc, value);
      }

      public static void Copy (IntPtr source, Point sourcePosition, IntPtr target, Point targetPosition, Size size) {
         if (size.Width == 0 || size.Height == 0) return;

         BitBlt (target, targetPosition.X, targetPosition.Y, size.Width, size.Height, source, sourcePosition.X, sourcePosition.Y, TernaryRasterOperations.SRCCOPY);
      }

      public static void SetTextAlignemnt (IntPtr hdc, HorizontalAlignment alignment) {
         var value = alignment switch {
            HorizontalAlignment.Left => Alignment.LEFT,
            HorizontalAlignment.Center => Alignment.CENTER,
            HorizontalAlignment.Right => Alignment.RIGHT,
            _ => throw new InvalidEnumArgumentException (nameof (alignment), (int)alignment, typeof (HorizontalAlignment))
         };

         SetTextAlign (hdc, value);
      }

      public static void PrintText (IntPtr hdc, ref GDIRECT rect, HorizontalAlignment alignment, string text) {
         int txtLen = text == null ? 0 : text.Length;

         int x = rect.Left;
         int y = rect.Top;
         switch (alignment) {
            case HorizontalAlignment.Left: break;
            case HorizontalAlignment.Right: y = rect.Right; break;
            case HorizontalAlignment.Center: x = (x + rect.Right) / 2; break;
         };

         ExtTextOut (hdc, x, y, ETOOptions.OPAQUE | ETOOptions.CLIPPED, ref rect, text, (uint)txtLen, IntPtr.Zero);
      }
      [DllImport ("user32.dll")]
      static extern int FrameRect (IntPtr hdc, [In] ref GDIRECT lprc, IntPtr hbr);
      [DllImport ("gdi32.dll")]
      static extern IntPtr GetStockObject (int id);

      public static void PrintText (IntPtr hdc, ref GDIRECT rect, int xPos, string text) {
         int txtLen = text == null ? 0 : text.Length;
         ExtTextOut (hdc, xPos, rect.Top, ETOOptions.OPAQUE | ETOOptions.CLIPPED, ref rect, text, (uint)txtLen, IntPtr.Zero);
         GDIRECT rc = new GDIRECT (0, 0, 0, 0);
         DrawText (hdc, text, txtLen, ref rc, DT_CALCRECT);

         rc.Top += rect.Top;
         rc.Bottom += rect.Top;
         rc.Left += rect.Left;
         rc.Right += rect.Left;

         //For debugging puposes, draw a frame around the text
         //FrameRect (hdc, ref rc, GetStockObject (4));




      }

      public static void Fill (IntPtr hdc, ref GDIRECT gdiRect) {
         ExtTextOut (hdc, gdiRect.Left, gdiRect.Top, ETOOptions.OPAQUE | ETOOptions.CLIPPED, ref gdiRect, null, 0, IntPtr.Zero);
      }

      public static void Delete (IntPtr hdc) {
         DeleteObject (hdc);
      }

      public static void Select (IntPtr hdc, IntPtr obj) {
         SelectObject (hdc, obj);
      }

      public static Size Measure (IntPtr hdc, string text, IntPtr font) {
         int len = text == null ? 0 : text.Length;
         Select (hdc, font);

         GDIRECT rc = new GDIRECT (0, 0, 0, 0);
         //GetTextExtentPoint32 (hdc, text, len, out var size);
         DrawText (hdc, text, len, ref rc, DT_CALCRECT | DT_NOCLIP | DT_LEFT | DT_SINGLELINE);

         return new Size (rc.Right, rc.Bottom);
      }

      private const int DT_TOP = 0x00000000;
      private const int DT_LEFT = 0x00000000;
      private const int DT_CENTER = 0x00000001;
      private const int DT_RIGHT = 0x00000002;
      private const int DT_VCENTER = 0x00000004;
      private const int DT_BOTTOM = 0x00000008;
      private const int DT_WORDBREAK = 0x00000010;
      private const int DT_SINGLELINE = 0x00000020;
      private const int DT_EXPANDTABS = 0x00000040;
      private const int DT_TABSTOP = 0x00000080;
      private const int DT_NOCLIP = 0x00000100;
      private const int DT_EXTERNALLEADING = 0x00000200;
      private const int DT_CALCRECT = 0x00000400;
      private const int DT_NOPREFIX = 0x00000800;
      private const int DT_INTERNAL = 0x00001000;
      private const int DT_EDITCONTROL = 0x00002000;
      private const int DT_PATH_ELLIPSIS = 0x00004000;
      private const int DT_END_ELLIPSIS = 0x00008000;
      private const int DT_MODIFYSTRING = 0x00010000;
      private const int DT_RTLREADING = 0x00020000;
      private const int DT_WORD_ELLIPSIS = 0x00040000;
      private const int DT_NOFULLWIDTHCHARBREAK = 0x00080000;
      private const int DT_HIDEPREFIX = 0x00100000;
      private const int DT_PREFIXONLY = 0x00200000;

      [DllImport ("user32.dll")]
      public static extern int DrawText (IntPtr hDC, string lpString, int nCount, ref GDIRECT lpRect, uint uFormat);


      [DllImport ("gdi32.dll", EntryPoint = "ExtTextOutW")]
      private static extern bool ExtTextOut (IntPtr hdc, int X, int Y, ETOOptions fuOptions, [In] ref GDIRECT lprc, [MarshalAs (UnmanagedType.LPWStr)] string lpString, uint cbCount, [In] IntPtr lpDx);

      [DllImport ("gdi32.dll")]
      private static extern uint SetBkColor (IntPtr hdc, int crColor);

      [DllImport ("gdi32.dll")]
      private static extern uint SetTextColor (IntPtr hdc, int crColor);

      [DllImport ("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
      [return: MarshalAs (UnmanagedType.Bool)]
      internal static extern bool BitBlt ([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

      [DllImport ("gdi32.dll")]
      private static extern uint SetTextAlign (IntPtr hdc, Alignment fMode);

      [DllImport ("gdi32.dll", EntryPoint = "DeleteObject")]
      [return: MarshalAs (UnmanagedType.Bool)]
      private static extern bool DeleteObject ([In] IntPtr hObject);

      [DllImport ("gdi32.dll", EntryPoint = "SelectObject")]
      public static extern IntPtr SelectObject ([In] IntPtr hdc, [In] IntPtr hgdiobj);

      [DllImport ("gdi32.dll")]
      static extern bool GetTextExtentPoint32 (IntPtr hdc, string lpString, int cbString, out SIZE lpSize);

      internal enum TernaryRasterOperations : uint {
         NONE = 0x0,
         SRCCOPY = 0x00CC0020,
         SRCPAINT = 0x00EE0086,
         SRCAND = 0x008800C6,
         SRCINVERT = 0x00660046,
         SRCERASE = 0x00440328,
         NOTSRCCOPY = 0x00330008,
         NOTSRCERASE = 0x001100A6,
         MERGECOPY = 0x00C000CA,
         MERGEPAINT = 0x00BB0226,
         PATCOPY = 0x00F00021,
         PATPAINT = 0x00FB0A09,
         PATINVERT = 0x005A0049,
         DSTINVERT = 0x00550009,
         BLACKNESS = 0x00000042,
         WHITENESS = 0x00FF0062,
         CAPTUREBLT = 0x40000000
      }

      [Flags]
      internal enum ETOOptions : uint {
         CLIPPED = 0x4,
         GLYPH_INDEX = 0x10,
         IGNORELANGUAGE = 0x1000,
         NUMERICSLATIN = 0x800,
         NUMERICSLOCAL = 0x400,
         OPAQUE = 0x2,
         PDY = 0x2000,
         RTLREADING = 0x800,
      }

      [Flags]
      private enum Alignment : uint {
         LEFT = 0,
         RIGHT = 2,
         CENTER = 6,

         TOP = 0,
         BOTTOM = 8,
         BASELINE = 24
      }

      [StructLayout (LayoutKind.Sequential)]
      public struct GDIRECT {
         public int Left;
         public int Top;
         public int Right;
         public int Bottom;

         public GDIRECT (int left, int top, int right, int bottom) {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
         }
      }

      [StructLayout (LayoutKind.Sequential)]
      public struct SIZE {
         public int cx;
         public int cy;

         public SIZE (int cx, int cy) {
            this.cx = cx;
            this.cy = cy;
         }
      }
   }
}
