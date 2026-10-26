using System;
using FenUISharp.Objects;

namespace DynamicWin;

public struct ActivitySpring
{
	public Type RelatedActivity;

	public Func<UIObject> Content;

	public Func<bool> Condition;
}
