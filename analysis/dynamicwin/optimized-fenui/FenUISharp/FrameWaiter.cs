using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace FenUISharp;

// Wait without occupying a CPU core during the final milliseconds of a frame.
internal sealed class FrameWaiter : IDisposable
{
    private IntPtr timer = CreateWaitableTimerExW(IntPtr.Zero, null, 2, 0x00100002);

    public void Wait(double milliseconds)
    {
        if (milliseconds <= 0) return;
        long due = -Math.Max(1, (long)(milliseconds * 10000));
        if (timer != IntPtr.Zero && SetWaitableTimer(timer, ref due, 0, IntPtr.Zero, IntPtr.Zero, false))
        {
            if (WaitForSingleObject(timer, 1000) == 0) return;
        }
        Thread.Sleep(Math.Max(1, (int)Math.Ceiling(milliseconds)));
    }

    public void Dispose()
    {
        if (timer != IntPtr.Zero) CloseHandle(timer);
        timer = IntPtr.Zero;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWaitableTimerExW(IntPtr attributes, string? name, uint flags, uint access);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWaitableTimer(IntPtr timer, ref long due, int period, IntPtr completion, IntPtr argument, [MarshalAs(UnmanagedType.Bool)] bool resume);
    [DllImport("kernel32.dll")]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);
}
