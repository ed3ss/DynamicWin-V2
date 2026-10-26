using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FPanel : UIObject, IStateListener
{
	protected bool _drawBasePanel = true;

	public State<float> CornerRadius { get; private init; }

	public State<bool> UseSquircle { get; private init; }

	public FPanel(Func<Vector2>? position = null, Func<Vector2>? size = null, float? cornerRadius = null, Func<SKColor>? color = null)
		: base(position, size)
	{
		CornerRadius = new State<float>(() => cornerRadius ?? 35f, this, this);
		UseSquircle = new State<bool>(() => true, this, this);
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: true);
		if (color == null)
		{
			base.RenderMaterial.Value = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.PanelMaterial();
		}
		else
		{
			base.RenderMaterial.Value = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.PanelMaterial().WithOverride(new Dictionary<string, object> { ["BaseColor"] = color });
		}
		base.Padding.Value = () => 20;
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		if (_drawBasePanel)
		{
			RenderBasePanel(canvas, base.Shape.LocalBounds);
		}
	}

	protected virtual void RenderBasePanel(SKCanvas canvas, SKRect rect)
	{
		using SKPaint paint = GetRenderPaint();
		using SKPath path = GetPanelPath(rect);
		base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, path, this, paint);
	}

	public SKPath GetPanelPath(SKRect? rect = null)
	{
		SKPath sKPath = new SKPath();
		if (UseSquircle.CachedValue)
		{
			sKPath = SKSquircle.CreateSquircle(rect ?? base.Shape.LocalBounds, CornerRadius.CachedValue);
		}
		else
		{
			sKPath.AddRoundRect(rect ?? base.Shape.LocalBounds, CornerRadius.CachedValue, CornerRadius.CachedValue);
		}
		return sKPath;
	}
}
