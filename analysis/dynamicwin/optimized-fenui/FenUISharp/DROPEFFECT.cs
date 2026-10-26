using System;

namespace FenUISharp;

[Flags]
public enum DROPEFFECT : uint
{
	None = 0u,
	Copy = 1u,
	Move = 2u,
	Link = 4u,
	Scroll = 0x80000000u
}
