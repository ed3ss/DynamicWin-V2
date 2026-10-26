using System.Runtime.InteropServices;
using FenUISharp.Native;

namespace FenUISharp;

[ComImport]
[Guid("00000122-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDropTarget
{
	void DragEnter([In] nint pDataObj, [In] uint grfKeyState, [In] POINT pt, [In][Out] ref uint pdwEffect);

	void DragOver([In] uint grfKeyState, [In] POINT pt, [In][Out] ref uint pdwEffect);

	void DragLeave();

	void Drop([In] nint pDataObj, [In] uint grfKeyState, [In] POINT pt, [In][Out] ref uint pdwEffect);
}
