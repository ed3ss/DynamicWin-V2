namespace FenUISharp.Native;

public struct RECT
{
	public int left;

	public int top;

	public int right;

	public int bottom;

	public int Width => right - left;

	public int Height => bottom - top;
}
