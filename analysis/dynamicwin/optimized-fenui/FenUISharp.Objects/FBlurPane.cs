using System;
using System.Collections.Generic;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FBlurPane : FPanel, IStateListener
{
	public bool HighQualityBlur { get; set; } = true;

	public State<Material> BlurMaterial { get; init; }

	public FBlurPane(Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(position, size)
	{
		BlurMaterial = new State<Material>(() => new BlurMaterial(() => base.Composition.GrabBehindPlusBuffer(base.Shape.GlobalBounds, HighQualityBlur ? 0.3f : 0.02f))
		{
			BlurRadius = () => HighQualityBlur ? 15 : 5
		}, this, this);
		base.RenderMaterial.Value = () => new MaterialCompose(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.PanelMaterial().WithOverride(new Dictionary<string, object> { ["BorderColor"] = (Func<SKColor>)(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(50)) }), () => BlurMaterial.CachedValue);
	}

	public override void OnInternalStateChanged<T>(T value)
	{
		base.OnInternalStateChanged(value);
		Invalidate(Invalidation.SurfaceDirty);
	}

	protected override void Update()
	{
		base.Update();
		if (FContext.GetCurrentWindow().Surface.IsNextFrameRendering())
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint paint = GetRenderPaint();
		using SKPath path = GetPanelPath();
		base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, path, this, paint);
	}
}
