using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using FenUISharp.Logging;
using FenUISharp.Native;

namespace FenUISharp;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class DataObject : IDataObject
{
	private readonly Dictionary<int, object> _data = new Dictionary<int, object>();

	private const int S_OK = 0;

	private const int DATA_S_SAMEFORMATETC = 262448;

	private const int E_NOTIMPL = -2147467263;

	private const int E_UNEXPECTED = -2147418113;

	private const int DV_E_FORMATETC = -2147221404;

	private const int DV_E_TYMED = -2147221399;

	private const int CF_HDROP = 15;

	private const int CF_UNICODETEXT = 13;

	private const int CF_TEXT = 1;

	private const uint GMEM_MOVEABLE = 2u;

	private const uint GMEM_ZEROINIT = 64u;

	public void SetData(int format, object data)
	{
		_data[format] = data;
	}

	public int GetData(ref FORMATETC format, out STGMEDIUM medium)
	{
		medium = default(STGMEDIUM);
		if (!_data.ContainsKey(format.cfFormat))
		{
			return -2147221404;
		}
		try
		{
			if (format.cfFormat == 15 && _data[format.cfFormat] is string[] files)
			{
				if ((format.tymed & TYMED.TYMED_HGLOBAL) == 0)
				{
					return -2147221399;
				}
				nint unionmember = CreateHDrop(files);
				medium.tymed = TYMED.TYMED_HGLOBAL;
				medium.unionmember = unionmember;
				medium.pUnkForRelease = IntPtr.Zero;
				return 0;
			}
			if ((format.cfFormat == 13 || format.cfFormat == 1) && _data[format.cfFormat] is string text)
			{
				if ((format.tymed & TYMED.TYMED_HGLOBAL) == 0)
				{
					return -2147221399;
				}
				nint unionmember2 = CreateTextData(text, format.cfFormat == 13);
				medium.tymed = TYMED.TYMED_HGLOBAL;
				medium.unionmember = unionmember2;
				medium.pUnkForRelease = IntPtr.Zero;
				return 0;
			}
		}
		catch (Exception ex)
		{
			FLogger.Error("GetData failed: " + ex.Message);
			return -2147418113;
		}
		return -2147221404;
	}

	public int GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
	{
		if (!_data.ContainsKey(format.cfFormat))
		{
			return -2147221404;
		}
		try
		{
			if (format.cfFormat == 15 && _data[format.cfFormat] is string[] files)
			{
				if ((format.tymed & TYMED.TYMED_HGLOBAL) == 0)
				{
					return -2147221399;
				}
				nint unionmember = CreateHDrop(files);
				medium.tymed = TYMED.TYMED_HGLOBAL;
				medium.unionmember = unionmember;
				medium.pUnkForRelease = IntPtr.Zero;
				return 0;
			}
			if ((format.cfFormat == 13 || format.cfFormat == 1) && _data[format.cfFormat] is string text)
			{
				if ((format.tymed & TYMED.TYMED_HGLOBAL) == 0)
				{
					return -2147221399;
				}
				nint unionmember2 = CreateTextData(text, format.cfFormat == 13);
				medium.tymed = TYMED.TYMED_HGLOBAL;
				medium.unionmember = unionmember2;
				medium.pUnkForRelease = IntPtr.Zero;
				return 0;
			}
		}
		catch (Exception ex)
		{
			FLogger.Error("GetData failed: " + ex.Message);
			return -2147418113;
		}
		return -2147221404;
	}

	public int QueryGetData(ref FORMATETC format)
	{
		if (_data.ContainsKey(format.cfFormat))
		{
			if (format.cfFormat == 15 && (format.tymed & TYMED.TYMED_HGLOBAL) != TYMED.TYMED_NULL)
			{
				return 0;
			}
			if ((format.cfFormat == 13 || format.cfFormat == 1) && (format.tymed & TYMED.TYMED_HGLOBAL) != TYMED.TYMED_NULL)
			{
				return 0;
			}
		}
		return -2147221404;
	}

	public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
	{
		formatOut = formatIn;
		return 262448;
	}

	public int SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
	{
		return -2147467263;
	}

	public int DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
	{
		connection = 0;
		return -2147467263;
	}

	public int DUnadvise(int connection)
	{
		return -2147467263;
	}

	public int EnumDAdvise(out IEnumSTATDATA enumAdvise)
	{
		enumAdvise = null;
		return -2147467263;
	}

	private nint CreateHDrop(string[] files)
	{
		int num = Marshal.SizeOf<DROPFILES>();
		int num2 = 0;
		string[] array = files;
		foreach (string text in array)
		{
			num2 += (text.Length + 1) * 2;
		}
		num2 += 2;
		int num3 = num + num2;
		nint num4 = GlobalAlloc(66u, (nuint)num3);
		if (num4 == IntPtr.Zero)
		{
			throw new OutOfMemoryException("Failed to allocate global memory for HDROP");
		}
		nint num5 = GlobalLock(num4);
		if (num5 == IntPtr.Zero)
		{
			GlobalFree(num4);
			throw new InvalidOperationException("Failed to lock global memory");
		}
		try
		{
			Marshal.StructureToPtr(new DROPFILES
			{
				pFiles = (uint)num,
				pt = new POINT
				{
					x = 0,
					y = 0
				},
				fNC = false,
				fWide = true
			}, num5, fDeleteOld: false);
			nint num6 = IntPtr.Add(num5, num);
			array = files;
			for (int i = 0; i < array.Length; i++)
			{
				string fullPath = Path.GetFullPath(array[i]);
				byte[] bytes = Encoding.Unicode.GetBytes(fullPath + "\0");
				Marshal.Copy(bytes, 0, num6, bytes.Length);
				num6 = IntPtr.Add(num6, bytes.Length);
			}
			Marshal.WriteInt16(num6, 0);
		}
		finally
		{
			GlobalUnlock(num4);
		}
		return num4;
	}

	private nint CreateTextData(string text, bool unicode)
	{
		byte[] array = ((!unicode) ? Encoding.Default.GetBytes(text + "\0") : Encoding.Unicode.GetBytes(text + "\0"));
		nint num = GlobalAlloc(2u, (nuint)array.Length);
		if (num == IntPtr.Zero)
		{
			throw new OutOfMemoryException();
		}
		nint num2 = GlobalLock(num);
		if (num2 == IntPtr.Zero)
		{
			GlobalFree(num);
			throw new InvalidOperationException("Failed to lock global memory");
		}
		try
		{
			Marshal.Copy(array, 0, num2, array.Length);
		}
		finally
		{
			GlobalUnlock(num);
		}
		return num;
	}

	[DllImport("kernel32.dll")]
	private static extern nint GlobalAlloc(uint uFlags, nuint dwBytes);

	[DllImport("kernel32.dll")]
	private static extern nint GlobalLock(nint hMem);

	[DllImport("kernel32.dll")]
	private static extern bool GlobalUnlock(nint hMem);

	[DllImport("kernel32.dll")]
	private static extern nint GlobalFree(nint hMem);

	void IDataObject.DUnadvise(int connection)
	{
		throw new NotImplementedException();
	}

	public int EnumFormatEtc(DATADIR direction, out IEnumFORMATETC ppenumFormatEtc)
	{
		if (direction == DATADIR.DATADIR_GET)
		{
			List<FORMATETC> list = new List<FORMATETC>();
			if (_data.ContainsKey(15))
			{
				list.Add(new FORMATETC
				{
					cfFormat = 15,
					dwAspect = DVASPECT.DVASPECT_CONTENT,
					lindex = -1,
					tymed = TYMED.TYMED_HGLOBAL,
					ptd = IntPtr.Zero
				});
			}
			if (_data.ContainsKey(13))
			{
				list.Add(new FORMATETC
				{
					cfFormat = 13,
					dwAspect = DVASPECT.DVASPECT_CONTENT,
					lindex = -1,
					tymed = TYMED.TYMED_HGLOBAL,
					ptd = IntPtr.Zero
				});
			}
			if (_data.ContainsKey(1))
			{
				list.Add(new FORMATETC
				{
					cfFormat = 1,
					dwAspect = DVASPECT.DVASPECT_CONTENT,
					lindex = -1,
					tymed = TYMED.TYMED_HGLOBAL,
					ptd = IntPtr.Zero
				});
			}
			ppenumFormatEtc = new FormatEtcEnumerator(list.ToArray());
			return 0;
		}
		ppenumFormatEtc = null;
		return -2147467263;
	}

	void IDataObject.GetData(ref FORMATETC format, out STGMEDIUM medium)
	{
		int data = GetData(ref format, out medium);
		if (data != 0)
		{
			Marshal.ThrowExceptionForHR(data);
		}
	}

	void IDataObject.GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
	{
		int dataHere = GetDataHere(ref format, ref medium);
		if (dataHere != 0)
		{
			Marshal.ThrowExceptionForHR(dataHere);
		}
	}

	void IDataObject.SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
	{
		throw new NotImplementedException();
	}

	public IEnumFORMATETC EnumFormatEtc(DATADIR direction)
	{
		if (direction == DATADIR.DATADIR_GET)
		{
			List<FORMATETC> list = new List<FORMATETC>();
			if (_data.ContainsKey(15))
			{
				list.Add(new FORMATETC
				{
					cfFormat = 15,
					dwAspect = DVASPECT.DVASPECT_CONTENT,
					lindex = -1,
					tymed = TYMED.TYMED_HGLOBAL,
					ptd = IntPtr.Zero
				});
			}
			if (_data.ContainsKey(13))
			{
				list.Add(new FORMATETC
				{
					cfFormat = 13,
					dwAspect = DVASPECT.DVASPECT_CONTENT,
					lindex = -1,
					tymed = TYMED.TYMED_HGLOBAL,
					ptd = IntPtr.Zero
				});
			}
			if (_data.ContainsKey(1))
			{
				list.Add(new FORMATETC
				{
					cfFormat = 1,
					dwAspect = DVASPECT.DVASPECT_CONTENT,
					lindex = -1,
					tymed = TYMED.TYMED_HGLOBAL,
					ptd = IntPtr.Zero
				});
			}
			return new FormatEtcEnumerator(list.ToArray());
		}
		return null;
	}
}
