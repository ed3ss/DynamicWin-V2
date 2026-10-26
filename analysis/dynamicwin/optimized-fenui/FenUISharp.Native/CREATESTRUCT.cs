using System.Runtime.InteropServices;

namespace FenUISharp.Native;

public struct CREATESTRUCT
{
	public nint lpCreateParams;

	public nint hInstance;

	public nint hMenu;

	public nint hwndParent;

	public int cy;

	public int cx;

	public int y;

	public int x;

	public int style;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpszName;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string lpszClass;

	public uint dwExStyle;
}
