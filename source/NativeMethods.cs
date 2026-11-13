using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SpotifyGlassSidebar;
internal static class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)] private struct Margins { public int Left, Right, Top, Bottom; }
    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr h, int attribute, ref int value, int size);
    [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr h, ref Margins margins);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr FindWindow(string? cls, string name);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int RegisterWindowMessage(string name);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr h, int message, IntPtr wParam, IntPtr lParam);
    public static readonly int ShowMessage = RegisterWindowMessage("SpotifyGlassSidebar.RestoreWindow.v1");
    [StructLayout(LayoutKind.Sequential)] private struct WindowRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr h, out WindowRect rect);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int width, int height, uint flags);
    public static System.Drawing.Rectangle Bounds(Window window)
    {
        GetWindowRect(new WindowInteropHelper(window).Handle, out var r);
        return System.Drawing.Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
    }
    public static void Move(Window window, int x, int y) => SetWindowPos(new WindowInteropHelper(window).Handle, IntPtr.Zero, x, y, 0, 0, 0x0015);
    public static bool ApplyGlass(Window window)
    {
        var h = new WindowInteropHelper(window).Handle;
        HwndSource.FromHwnd(h).CompositionTarget.BackgroundColor = Colors.Transparent;
        var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        DwmExtendFrameIntoClientArea(h, ref margins);
        int dark = 1, corners = 2, backdrop = 3, border = 0x34543D;
        DwmSetWindowAttribute(h, 20, ref dark, 4);
        DwmSetWindowAttribute(h, 33, ref corners, 4);
        DwmSetWindowAttribute(h, 34, ref border, 4);
        return DwmSetWindowAttribute(h, 38, ref backdrop, 4) == 0;
    }
    public static void ShowExisting()
    {
        var h = FindWindow(null, "Spotify Glass Sidebar");
        if (h != IntPtr.Zero) { PostMessage(h, ShowMessage, IntPtr.Zero, IntPtr.Zero); SetForegroundWindow(h); }
    }
}