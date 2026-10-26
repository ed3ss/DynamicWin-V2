using System.Runtime.InteropServices;

namespace FenUISharp;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("00000121-0000-0000-C000-000000000046")]
public interface IDropSource
{
	[PreserveSig]
	int QueryContinueDrag([MarshalAs(UnmanagedType.Bool)] bool fEscapePressed, uint grfKeyState);

	[PreserveSig]
	int GiveFeedback(uint dwEffect);
}
