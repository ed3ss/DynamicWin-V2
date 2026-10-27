using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using FenUISharp;

namespace DynamicWin;

// Native foreground events repair the overlay's Z order, without activating it.
internal sealed class TopmostGuard : IDisposable
{
    private readonly FWindow window;
    private readonly WinEventProc callback;
    private readonly Action check;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly IntPtr hook;
    private long nextCheck;
    private bool disposed;
    private bool hiddenForFullscreen;

    internal TopmostGuard(FWindow window)
    {
        this.window = window;
        check = Check;
        callback = (_, _, _, _, _, _, _) => Queue();
        hook = SetWinEventHook(3, 3, IntPtr.Zero, callback, 0, 0, 2); // FOREGROUND, OUTOFCONTEXT | SKIPOWNPROCESS
        window.Callbacks.OnPreUpdate += Watchdog;
        Queue();
    }

    private void Queue()
    {
        if (!disposed) window.WindowDispatcher.InvokeWithID(check, "dynamicwin.topmost");
    }

    private void Watchdog()
    {
        if (clock.ElapsedMilliseconds < nextCheck) return;
        nextCheck = clock.ElapsedMilliseconds + 2000;
        Queue(); // Also catches delayed promotions after an app launches.
    }

    private void Check()
    {
        if (disposed) return;
        if (ForegroundPolicy.ShouldYield())
        {
            if (IsWindowVisible(window.hWnd))
            {
                hiddenForFullscreen = true;
                ShowWindow(window.hWnd, 0); // Hide; never activate or compete with a game.
            }
            return;
        }
        if (hiddenForFullscreen)
        {
            hiddenForFullscreen = false;
            ShowWindow(window.hWnd, 8); // SW_SHOWNA: restore without activation.
        }
        if (!window.Properties.AlwaysOnTop || !IsWindowVisible(window.hWnd)) return;
        IntPtr foreground = GetForegroundWindow();
        GetWindowThreadProcessId(foreground, out uint process);
        if (process == Environment.ProcessId) return; // Keep our settings window usable.
        bool covered = false;
        IntPtr above = GetWindow(window.hWnd, 3); // GW_HWNDPREV
        for (int i = 0; above != IntPtr.Zero && i < 256; i++, above = GetWindow(above, 3))
        {
            if (above == foreground) { covered = true; break; }
        }
        bool topmost = (GetWindowLongPtrW(window.hWnd, -20).ToInt64() & 8) != 0;
        if (!topmost || covered)
            SetWindowPos(window.hWnd, new IntPtr(-1), 0, 0, 0, 0, 0x213); // NOSIZE | NOMOVE | NOACTIVATE | NOOWNERZORDER
        if (Environment.GetEnvironmentVariable("DYNAMICWIN_DIAGNOSTICS") == "1")
        {
            string status = $"topmost={(GetWindowLongPtrW(window.hWnd, -20).ToInt64() & 8) != 0}; focusPreserved={GetForegroundWindow() == foreground}; restored={!topmost || covered}";
            try { File.WriteAllText(Path.Combine(DynamicWin.AppDataPath, "topmost-status.txt"), status); }
            catch (IOException) { }
        }
    }

    public void Dispose()
    {
        disposed = true;
        window.Callbacks.OnPreUpdate -= Watchdog;
        if (hook != IntPtr.Zero) UnhookWinEvent(hook);
        GC.KeepAlive(callback);
    }

    private delegate void WinEventProc(IntPtr hook, uint eventType, IntPtr hwnd, int objectId, int childId, uint threadId, uint time);
    [DllImport("user32.dll")] private static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr module, WinEventProc proc, uint process, uint thread, uint flags);
    [DllImport("user32.dll")] private static extern bool UnhookWinEvent(IntPtr hook);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint process);
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hwnd, uint command);
    [DllImport("user32.dll")] private static extern IntPtr GetWindowLongPtrW(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hwnd, int command);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int w, int h, uint flags);
}
