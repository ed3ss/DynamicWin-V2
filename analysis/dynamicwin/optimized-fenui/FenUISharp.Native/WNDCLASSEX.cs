using System.Runtime.InteropServices;

namespace FenUISharp.Native;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct WNDCLASSEX
{
	public uint cbSize;

	public uint style;

	public WndProcDelegate lpfnWndProc;

	public int cbClsExtra;

	public int cbWndExtra;

	public nint hInstance;

	public nint hIcon;

	public nint hCursor;

	public nint hbrBackground;

	public string lpszMenuName;

	public string lpszClassName;

	public nint hIconSm;
}
