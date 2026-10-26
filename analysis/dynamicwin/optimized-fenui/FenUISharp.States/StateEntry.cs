using System;

namespace FenUISharp.States;

public struct StateEntry<T>
{
	public bool IsStatic;

	public Func<T> Value;

	public uint Priority;
}
