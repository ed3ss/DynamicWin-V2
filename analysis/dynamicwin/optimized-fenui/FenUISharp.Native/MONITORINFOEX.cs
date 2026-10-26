using System.Runtime.InteropServices;

namespace FenUISharp.Native;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct MONITORINFOEX
{
	public int cbSize;

	public RECT rcMonitor;

	public RECT rcWork;

	public uint dwFlags;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string szDevice;
}
