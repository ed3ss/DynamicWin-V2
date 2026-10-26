using System;

namespace FenUISharp.Native;

internal static class DPI_AWARENESS_CONTEXT
{
	public static readonly nint UNAWARE = new IntPtr(-1);

	public static readonly nint SYSTEM_AWARE = new IntPtr(-2);

	public static readonly nint PER_MONITOR_AWARE = new IntPtr(-3);

	public static readonly nint PER_MONITOR_AWARE_V2 = new IntPtr(-4);

	public static readonly nint UNAWARE_GDISCALED = new IntPtr(-5);
}
