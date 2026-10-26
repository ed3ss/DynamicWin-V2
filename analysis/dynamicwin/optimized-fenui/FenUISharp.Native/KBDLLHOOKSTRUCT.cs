namespace FenUISharp.Native;

public struct KBDLLHOOKSTRUCT
{
	public int vkCode;

	public int scanCode;

	public int flags;

	public int time;

	public nint dwExtraInfo;
}
