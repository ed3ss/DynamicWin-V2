using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DynamicWin;

internal static class ForegroundPolicy
{
    internal static bool ExplorerIsForeground()
    {
        GetWindowThreadProcessId(GetForegroundWindow(), out uint id);
        try { using var process = Process.GetProcessById((int)id); return process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase); }
        catch { return false; }
    }

    internal static bool ShouldYield()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero || hwnd == GetShellWindow() || hwnd == GetDesktopWindow()) return false;
        GetWindowThreadProcessId(hwnd, out uint id);
        if (id == Environment.ProcessId) return false;
        try
        {
            using var process = Process.GetProcessById((int)id);
            if (process.ProcessName.StartsWith("FortniteClient", StringComparison.OrdinalIgnoreCase)) return true;
        }
        catch { }
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        return GetWindowRect(hwnd, out var rect) && GetMonitorInfoW(MonitorFromWindow(hwnd, 2), ref info)
            && CoversMonitor(rect.Left, rect.Top, rect.Right, rect.Bottom, info.Monitor.Left, info.Monitor.Top, info.Monitor.Right, info.Monitor.Bottom);
    }

    internal static bool CoversMonitor(int l, int t, int r, int b, int ml, int mt, int mr, int mb)
        => mr > ml && mb > mt && l <= ml && t <= mt && r >= mr && b >= mb;

    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct MonitorInfo { public int Size; public Rect Monitor, Work; public uint Flags; }
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern IntPtr GetShellWindow();
    [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint id);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool GetMonitorInfoW(IntPtr monitor, ref MonitorInfo info);
}
