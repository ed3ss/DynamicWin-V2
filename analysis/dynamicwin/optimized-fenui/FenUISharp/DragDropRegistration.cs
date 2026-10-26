using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FenUISharp;

public static class DragDropRegistration
{
	private static bool _hasBeenInitialized;

	public static void Initialize()
	{
		if (!_hasBeenInitialized)
		{
			_hasBeenInitialized = true;
			OleInitialize(IntPtr.Zero);
		}
	}

	[DllImport("ole32.dll", ExactSpelling = true)]
	public static extern int OleInitialize(nint pvReserved);

	[DllImport("ole32.dll", ExactSpelling = true, SetLastError = true)]
	public static extern int RegisterDragDrop(nint hwnd, nint pDropTarget);

	[DllImport("ole32.dll")]
	public static extern int RevokeDragDrop(nint hwnd);

	[DllImport("shell32.dll")]
	public static extern void DragFinish(nint hDrop);

	[DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern int OleGetData([MarshalAs(UnmanagedType.Interface)] IDataObject pDataObj, ref FORMATETC pformatetcIn, out STGMEDIUM pmedium);

	[DllImport("ole32.dll")]
	public static extern int OleQueryGetData([MarshalAs(UnmanagedType.Interface)] IDataObject pDataObj, ref FORMATETC pformatetc);

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	public static extern uint DragQueryFile(nint hDrop, uint iFile, StringBuilder? lpszFile, uint cch);

	[DllImport("ole32.dll")]
	public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);
}
