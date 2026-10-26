using System;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FDisplayableType : FPanel
{
	public State<SKColor> TintColor { get; private init; }

	public State<SKBlendMode> TintBlendMode { get; private init; }

	public FDisplayableType(Func<Vector2>? position = null, Func<Vector2>? size = null, bool dynamicColor = false, float? cornerRadius = null, Func<SKColor>? color = null)
		: base(position, size, cornerRadius, color)
	{
		TintBlendMode = new State<SKBlendMode>(() => SKBlendMode.Modulate, this, this);
		TintColor = new State<SKColor>(() => (!dynamicColor) ? SKColors.White : FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, this, this);
	}
}
