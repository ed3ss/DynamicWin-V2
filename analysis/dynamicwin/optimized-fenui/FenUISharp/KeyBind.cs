using System;

namespace FenUISharp;

public struct KeyBind
{
	public KeyBindFlags Flags { get; init; }

	public int VKCode { get; init; }

	public int[] AliasVKCodes { get; init; }

	public bool IgnoreFocus { get; init; }

	public Action? OnKeybindExecuted { get; set; }
}
