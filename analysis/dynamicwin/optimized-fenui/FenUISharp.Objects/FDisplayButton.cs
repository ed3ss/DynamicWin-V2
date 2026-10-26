using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.States;

namespace FenUISharp.Objects;

public class FDisplayButton : Button, IStateListener
{
	private float minWidth;

	private float maxWidth;

	public FDisplayableType Display { get; protected set; }

	public FDisplayButton(FDisplayableType display, Action? onClick = null, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(onClick, position, size)
	{
		base.OnClick = onClick;
		maxWidth = RMath.Clamp(maxWidth, minWidth, float.MaxValue);
		minWidth = RMath.Clamp(minWidth, 0f, maxWidth);
		Display = display;
		Display.SetParent(this);
		Display.Layout.StretchHorizontal.SetStaticState(value: true);
		Display.Layout.StretchVertical.SetStaticState(value: true);
		base.Padding.SetStaticState(10);
	}
}
