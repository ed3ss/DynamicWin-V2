using System.Runtime.InteropServices;

namespace FenUISharp.Native;

public struct DEVMODE
{
	private const int CCHDEVICENAME = 32;

	private const int CCHFORMNAME = 32;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string dmDeviceName;

	public ushort dmSpecVersion;

	public ushort dmDriverVersion;

	public ushort dmSize;

	public ushort dmDriverExtra;

	public uint dmFields;

	public int dmPositionX;

	public int dmPositionY;

	public uint dmDisplayOrientation;

	public uint dmDisplayFixedOutput;

	public short dmColor;

	public short dmDuplex;

	public short dmYResolution;

	public short dmTTOption;

	public short dmCollate;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string dmFormName;

	public ushort dmLogPixels;

	public uint dmBitsPerPel;

	public uint dmPelsWidth;

	public uint dmPelsHeight;

	public uint dmDisplayFlags;

	public uint dmDisplayFrequency;

	public uint dmICMMethod;

	public uint dmICMIntent;

	public uint dmMediaType;

	public uint dmDitherType;

	public uint dmReserved1;

	public uint dmReserved2;

	public uint dmPanningWidth;

	public uint dmPanningHeight;
}
