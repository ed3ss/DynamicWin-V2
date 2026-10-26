using System;

namespace FenUISharp.Native;

[Flags]
public enum SetWindowPosFlags : uint
{
	SWP_NOMOVE = 2u,
	SWP_NOSIZE = 1u,
	SWP_SHOWWINDOW = 0x40u,
	SWP_NOZORDER = 4u,
	SWP_NOACTIVATE = 0x10u
}
