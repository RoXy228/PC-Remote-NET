using System;
using System.Runtime.InteropServices;
using System.Windows;

public static class TrayPositionHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct NOTIFYICONIDENTIFIER
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public Guid guidItem;
    }

    [DllImport("shell32.dll")]
    static extern int Shell_NotifyIconGetRect(
        ref NOTIFYICONIDENTIFIER identifier,
        out RECT iconLocation);

    public static System.Windows.Point GetTrayIconPosition(IntPtr windowHandle)
    {
        var identifier = new NOTIFYICONIDENTIFIER
        {
            cbSize = Marshal.SizeOf<NOTIFYICONIDENTIFIER>(),
            hWnd = windowHandle,
            uID = 0
        };

        Shell_NotifyIconGetRect(ref identifier, out RECT rect);

        return new System.Windows.Point(
              rect.left + (rect.right - rect.left) / 2,
            rect.top
        );
    }
}