using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using FenUISharp.Mathematics;
using FenUISharp.Native;

namespace FenUISharp;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class DragDropHandler : IDropTarget
{
	internal FDropData? lastDropData;

	private const int DV_E_FORMATETC = -2147221404;

	private WeakReference<FWindow> window { get; set; }

	public FWindow Window
	{
		get
		{
			if (!window.TryGetTarget(out FWindow target))
			{
				throw new Exception("Window not set.");
			}
			return target;
		}
	}

	public Action<FDropData?>? dragEnter { get; set; }

	public Action<FDropData?>? dragOver { get; set; }

	public Action? dragLeave { get; set; }

	public Action<FDropData?>? dragDrop { get; set; }

	public Vector2 LastMouseDragPosition { get; private set; }

	public MultiAccess<DROPEFFECT> dropEffect { get; set; } = new MultiAccess<DROPEFFECT>(DROPEFFECT.None);

	public bool IsDragDropActionInProgress { get; set; }

	public DragDropHandler(FWindow root)
	{
		window = new WeakReference<FWindow>(root);
	}

	public void DragEnter([In] nint pDataObj, [In] uint grfKeyState, [In] POINT pt, [In][Out] ref uint pdwEffect)
	{
		pdwEffect = (uint)dropEffect.Value;
		lastDropData = HandleDropAction(pDataObj);
		IsDragDropActionInProgress = true;
		Win32APIs.ScreenToClient(Window.hWnd, ref pt);
		LastMouseDragPosition = new Vector2(pt.x, pt.y);
		dragEnter?.Invoke(lastDropData);
	}

	void IDropTarget.DragOver(uint grfKeyState, POINT pt, ref uint pdwEffect)
	{
		pdwEffect = (uint)dropEffect.Value;
		IsDragDropActionInProgress = true;
		Win32APIs.ScreenToClient(Window.hWnd, ref pt);
		LastMouseDragPosition = new Vector2(pt.x, pt.y);
		dragOver?.Invoke(lastDropData);
	}

	void IDropTarget.DragLeave()
	{
		IsDragDropActionInProgress = false;
		dragLeave?.Invoke();
	}

	public void Drop([In] nint pDataObj, [In] uint grfKeyState, [In] POINT pt, [In][Out] ref uint pdwEffect)
	{
		lastDropData = HandleDropAction(pDataObj);
		Win32APIs.ScreenToClient(Window.hWnd, ref pt);
		LastMouseDragPosition = new Vector2(pt.x, pt.y);
		dragDrop?.Invoke(lastDropData);
	}

	private FDropData? HandleDropAction([In] nint pDataObj)
	{
		IDataObject dataObject = (IDataObject)Marshal.GetObjectForIUnknown(pDataObj);
		(DropType, int, TYMED)[] array = new(DropType, int, TYMED)[5]
		{
			(DropType.FileDrop, 15, TYMED.TYMED_HGLOBAL),
			(DropType.UnicodeText, 13, TYMED.TYMED_HGLOBAL),
			(DropType.AnsiText, 1, TYMED.TYMED_HGLOBAL),
			(DropType.Bitmap, 2, TYMED.TYMED_GDI),
			(DropType.DeviceIndependentBitmap, 8, TYMED.TYMED_HGLOBAL)
		};
		for (int i = 0; i < array.Length; i++)
		{
			(DropType, int, TYMED) tuple = array[i];
			DropType item = tuple.Item1;
			int item2 = tuple.Item2;
			TYMED item3 = tuple.Item3;
			FORMATETC format = new FORMATETC
			{
				cfFormat = (short)item2,
				ptd = IntPtr.Zero,
				dwAspect = DVASPECT.DVASPECT_CONTENT,
				lindex = -1,
				tymed = item3
			};
			try
			{
				if (dataObject.QueryGetData(ref format) != 0)
				{
					continue;
				}
				dataObject.GetData(ref format, out var medium);
				try
				{
					switch (item)
					{
					case DropType.FileDrop:
					{
						uint num = DragDropRegistration.DragQueryFile(medium.unionmember, uint.MaxValue, null, 0u);
						string[] array2 = new string[num];
						for (uint num2 = 0u; num2 < num; num2++)
						{
							StringBuilder stringBuilder = new StringBuilder(260);
							if (DragDropRegistration.DragQueryFile(medium.unionmember, num2, stringBuilder, (uint)stringBuilder.Capacity) != 0)
							{
								array2[num2] = stringBuilder.ToString();
							}
						}
						return new FDropData(item, array2);
					}
					case DropType.UnicodeText:
					{
						string droppedText2 = GetDroppedText(medium.unionmember, isUnicode: true);
						return new FDropData(item, (droppedText2 == null) ? "" : droppedText2);
					}
					case DropType.AnsiText:
					{
						string droppedText = GetDroppedText(medium.unionmember, isUnicode: false);
						return new FDropData(item, (droppedText == null) ? "" : droppedText);
					}
					case DropType.Bitmap:
					{
						nint unionmember = medium.unionmember;
						return new FDropData(item, unionmember);
					}
					case DropType.DeviceIndependentBitmap:
						return new FDropData(item, medium.unionmember);
					}
				}
				finally
				{
					DragDropRegistration.ReleaseStgMedium(ref medium);
				}
			}
			catch (COMException ex) when (ex.ErrorCode == -2147221404)
			{
			}
		}
		return null;
	}

	private string GetDroppedText(nint hGlobal, bool isUnicode)
	{
		nint num = GlobalLock(hGlobal);
		if (num == IntPtr.Zero)
		{
			return "";
		}
		try
		{
			int num2 = ((IntPtr)GlobalSize(hGlobal)).ToInt32();
			if (num2 <= 0)
			{
				return "";
			}
			byte[] array = new byte[num2];
			Marshal.Copy(num, array, 0, num2);
			if (isUnicode)
			{
				return Encoding.Unicode.GetString(array).TrimEnd('\0');
			}
			return Encoding.Default.GetString(array).TrimEnd('\0');
		}
		finally
		{
			GlobalUnlock(hGlobal);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint GlobalLock(nint hMem);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GlobalUnlock(nint hMem);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint GlobalSize(nint hMem);
}
