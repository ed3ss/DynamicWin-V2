using System.Runtime.InteropServices;

namespace FenUISharp.Native;

public struct NOTIFYICONDATAA
{
	public int cbSize;

	public nint hWnd;

	public uint uID;

	public uint uFlags;

	public uint uCallbackMessage;

	public nint hIcon;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string szTip;
}
