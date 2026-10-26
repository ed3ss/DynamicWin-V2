using System.Runtime.InteropServices;

namespace FenUISharp;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class DropSource : IDropSource
{
	private const int S_OK = 0;

	private const int DRAGDROP_S_DROP = 262400;

	private const int DRAGDROP_S_CANCEL = 262401;

	private const int DRAGDROP_S_USEDEFAULTCURSORS = 262402;

	private const uint MK_LBUTTON = 1u;

	private const uint MK_RBUTTON = 2u;

	public int QueryContinueDrag(bool fEscapePressed, uint grfKeyState)
	{
		if (fEscapePressed)
		{
			return 262401;
		}
		bool num = (grfKeyState & 1) != 0;
		bool flag = (grfKeyState & 2) != 0;
		if (!num && !flag)
		{
			return 262400;
		}
		return 0;
	}

	public int GiveFeedback(uint dwEffect)
	{
		return 262402;
	}
}
