using Bitmanager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bitmanager.BigFile.Hooks {
   public class WindowHook {
      private readonly Logger logger;
      private readonly IntPtr prevWndProc;
      private readonly Win32WndProc ourWndProc;
      public WindowHook (Control control) {
         ourWndProc = HookedWndProc;
         IntPtr hwnd = control.Handle;
         logger = Logs.CreateLogger ("Bigfile", "hook");
         logger.Log ("Hooking... h={0:X}", hwnd);
         prevWndProc = SetWindowLong (hwnd, GWL_WNDPROC, new HandleRef(this,Marshal.GetFunctionPointerForDelegate (ourWndProc)));
         if (prevWndProc == IntPtr.Zero) {
            String err = OS.GetMessageStr (Marshal.GetLastWin32Error ());
            logger.Log ("Hooking error: {0}", err);
         }
         logger.Log ("Hooking done -> {0:X}", prevWndProc);

      }

      protected const int WM_RBUTTONDOWN = 0x0204;
      protected const int WM_RBUTTONUP = 0x0205;
      protected const int WM_CONTEXTMENU = 0x7B;

      protected IntPtr HookedWndProc (IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) {
         //logger.Log ("Msg={0} (0x{0:X} wp={1}, lp={2}", msg, wParam, lParam);
         switch (msg) {
            case WM_RBUTTONDOWN: return IntPtr.Zero;
            case WM_RBUTTONUP:   return DefWindowProc (hWnd, msg, wParam, lParam);
               //m.Msg = WM_CONTEXTMENU;
               //return;
         }
         return CallWindowProc (prevWndProc, hWnd, msg, wParam, lParam);
      }

      protected const int GWL_WNDPROC = -4;

      [DllImport ("user32")]
      protected static extern IntPtr SetWindowLongW (IntPtr hWnd, int nIndex, Win32WndProc newProc);

      [DllImport ("user32")]
      protected static extern IntPtr SetWindowLongPtr (IntPtr hWnd, int nIndex, Win32WndProc newProc);

      [DllImport ("user32")]
      protected static extern IntPtr CallWindowProc (IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

      // A delegate that matches Win32 WNDPROC:
      protected delegate IntPtr Win32WndProc (IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

      [DllImport ("user32.dll")]
      protected static extern IntPtr DefWindowProc (IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

      //SetWindowLong won't work correctly for 64-bit: we should use SetWindowLongPtr instead.  On
      //32-bit, SetWindowLongPtr is just #defined as SetWindowLong.  SetWindowLong really should 
      //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
      //it'll be OK.
      public static IntPtr SetWindowLong (IntPtr hWnd, int nIndex, HandleRef dwNewLong) {
         if (IntPtr.Size == 4) {
            return SetWindowLongPtr32 (hWnd, nIndex, dwNewLong);
         }
         return SetWindowLongPtr64 (hWnd, nIndex, dwNewLong);
      }

      [DllImport ("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
      public static extern IntPtr SetWindowLongPtr32 (IntPtr hWnd, int nIndex, HandleRef dwNewLong);
      [DllImport ("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
      public static extern IntPtr SetWindowLongPtr64 (IntPtr hWnd, int nIndex, HandleRef dwNewLong);

   }


}
