using System.Runtime.InteropServices;
using FenUISharp.Native;

namespace FenUISharp;

public struct DROPFILES
{
	public uint pFiles;

	public POINT pt;

	[MarshalAs(UnmanagedType.Bool)]
	public bool fNC;

	[MarshalAs(UnmanagedType.Bool)]
	public bool fWide;
}
