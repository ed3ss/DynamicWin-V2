using System.Runtime.InteropServices;

namespace FenUISharp.Native;

public struct PAINTSTRUCT
{
	public nint hdc;

	public bool fErase;

	public RECT rcPaint;

	public bool fRestore;

	public bool fIncUpdate;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public byte[] rgbReserved;
}
