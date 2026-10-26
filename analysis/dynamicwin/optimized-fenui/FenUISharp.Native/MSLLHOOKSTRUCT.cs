namespace FenUISharp.Native;

public struct MSLLHOOKSTRUCT
{
	public POINT pt;

	public uint mouseData;

	public uint flags;

	public uint time;

	public nint dwExtraInfo;
}
