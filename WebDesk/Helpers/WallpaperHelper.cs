using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WebDesk.Helpers
{
    public static class WallpaperHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr child, IntPtr parent);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr window, int index, int newLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private static readonly IntPtr HWND_BOTTOM = new(1);

        public static void SetWallpaperWindow(Window window)
        {
            IntPtr progman = FindWindow("Progman", string.Empty);  // Changed from null to string.Empty
            var helper = new WindowInteropHelper(window);
            
            SetParent(helper.Handle, progman);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, WS_EX_NOACTIVATE);
            
            window.Left = 0;
            window.Top = 0;
            window.Width = SystemParameters.PrimaryScreenWidth;
            window.Height = SystemParameters.PrimaryScreenHeight;

            SetWindowPos(helper.Handle, HWND_BOTTOM, 0, 0, 0, 0, 0x0010);
        }
    }
}