using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using FenUISharp.Logging;
using FenUISharp.Native;

namespace FenUISharp;

public static class DragSourceHelper
{
	private const int CF_HDROP = 15;

	private const int CF_UNICODETEXT = 13;

	private const int CF_TEXT = 1;

	private const int DRAGDROP_S_DROP = 262400;

	private const int DRAGDROP_S_CANCEL = 262401;

	private const int SM_CXDRAG = 68;

	private const int SM_CYDRAG = 69;

	public static DROPEFFECT DragFiles(string[] filePaths, DROPEFFECT allowedEffects = DROPEFFECT.Copy | DROPEFFECT.Move | DROPEFFECT.Link, Action<string[]> onMoveComplete = null)
	{
		if (filePaths == null || filePaths.Length == 0)
		{
			throw new ArgumentException("File paths cannot be null or empty");
		}
		if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
		{
			throw new InvalidOperationException("Drag operations must be performed on an STA thread");
		}
		string[] array = new string[filePaths.Length];
		for (int i = 0; i < filePaths.Length; i++)
		{
			array[i] = Path.GetFullPath(filePaths[i]);
			if (!File.Exists(array[i]) && !Directory.Exists(array[i]))
			{
				FLogger.Log("Warning: Path does not exist: " + array[i]);
			}
		}
		try
		{
			DataObject dataObject = new DataObject();
			dataObject.SetData(15, array);
			DropSource pDropSource = new DropSource();
			FLogger.Log($"Starting drag operation with {array.Length} files");
			FLogger.Log($"Allowed effects: {allowedEffects}");
			string[] array2 = array;
			foreach (string text in array2)
			{
				FLogger.Log("  - " + text);
			}
			uint pdwEffect = 0u;
			int num = DoDragDrop(dataObject, pDropSource, (uint)allowedEffects, ref pdwEffect);
			FLogger.Log($"DoDragDrop returned: 0x{num:X8}, effect: {pdwEffect}");
			switch (num)
			{
			case 262400:
			{
				DROPEFFECT dROPEFFECT = (DROPEFFECT)pdwEffect;
				if (dROPEFFECT == DROPEFFECT.Move && onMoveComplete != null)
				{
					try
					{
						onMoveComplete(array);
					}
					catch (Exception ex)
					{
						FLogger.Log("Move completion failed: " + ex.Message);
					}
				}
				return dROPEFFECT;
			}
			case 262401:
				return DROPEFFECT.None;
			default:
				FLogger.Log($"DoDragDrop failed with HRESULT: 0x{num:X8}");
				return DROPEFFECT.None;
			}
		}
		catch (Exception ex2)
		{
			FLogger.Log("Drag operation failed: " + ex2.Message);
			return DROPEFFECT.None;
		}
	}

	public static bool ShouldStartDrag(POINT startPoint, POINT currentPoint)
	{
		int systemMetrics = GetSystemMetrics(68);
		int systemMetrics2 = GetSystemMetrics(69);
		if (Math.Abs(currentPoint.x - startPoint.x) < systemMetrics)
		{
			return Math.Abs(currentPoint.y - startPoint.y) >= systemMetrics2;
		}
		return true;
	}

	public static DROPEFFECT DragText(string text, DROPEFFECT allowedEffects = DROPEFFECT.Copy)
	{
		if (string.IsNullOrEmpty(text))
		{
			throw new ArgumentException("Text cannot be null or empty");
		}
		if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
		{
			throw new InvalidOperationException("Drag operations must be performed on an STA thread");
		}
		DataObject dataObject = new DataObject();
		dataObject.SetData(13, text);
		dataObject.SetData(1, text);
		DropSource pDropSource = new DropSource();
		uint pdwEffect = 0u;
		DoDragDrop(dataObject, pDropSource, (uint)allowedEffects, ref pdwEffect);
		return (DROPEFFECT)pdwEffect;
	}

	[DllImport("ole32.dll")]
	private static extern int DoDragDrop([MarshalAs(UnmanagedType.Interface)] IDataObject pDataObj, [MarshalAs(UnmanagedType.Interface)] IDropSource pDropSource, uint dwOKEffects, ref uint pdwEffect);

	[DllImport("user32.dll")]
	private static extern int GetSystemMetrics(int nIndex);
}
