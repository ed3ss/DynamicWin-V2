using System;
using System.Diagnostics.CodeAnalysis;

namespace FenUISharp;

public struct MouseInputCode
{
	public MouseInputButton button { get; init; }

	public MouseInputState state { get; init; }

	public MouseInputCode(MouseInputButton btn, MouseInputState state)
	{
		button = btn;
		this.state = state;
	}

	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj != null)
		{
			if (button == ((MouseInputCode)obj).button)
			{
				return state == ((MouseInputCode)obj).state;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(button, state);
	}
}
