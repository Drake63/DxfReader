using System;
using System.Runtime.InteropServices;

namespace DxfOpener
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    static class WindowPlacement
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT wp);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT wp);

        public static bool SetPlacement(IntPtr hWhd, ref WINDOWPLACEMENT wp)
        {
            return SetWindowPlacement(hWhd, ref wp);
        }

        public static bool GetPlacement(IntPtr hWnd, out WINDOWPLACEMENT wp)
        {
            return GetWindowPlacement(hWnd, out wp);
        }
    }
}