namespace FenUISharp.Native;

public struct MSG
{
	public nint hwnd;

	public uint message;

	public nuint wParam;

	public nint lParam;

	public uint time;

	public POINT pt;
}
