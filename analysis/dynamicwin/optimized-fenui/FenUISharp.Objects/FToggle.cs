using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FToggle : SelectableButton, IStateListener
{
	public State<SKColor> CheckColor { get; set; }

	public FToggle(Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(null, null, position, size ?? ((Func<Vector2>)(() => new Vector2(20f, 20f))))
	{
		CheckColor = new State<SKColor>(() => SKColors.White, this, this);
		base.Padding.SetStaticState(2);
		base.Transform.Size.SetStaticState(new Vector2(20f, 20f));
		FImage fImage = new FImage(() => Resources.GetImage("fenui-builtin-check"));
		fImage.Transform.Size.SetResponsiveState(() => new Vector2(base.Layout.ClampSize(base.Transform.Size.CachedValue).x - 4f, base.Layout.ClampSize(base.Transform.Size.CachedValue).y - 4f));
		fImage.Transform.LocalPosition.SetStaticState(new Vector2(0.5f, 1f));
		fImage.Enabled.SetResponsiveState(() => base.IsSelected);
		fImage.SetParent(this);
	}
}
