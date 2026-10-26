using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FLargeSelectButton : SelectableButton
{
	private float borderThickness;

	public FDisplayableType Display { get; protected set; }

	public FText Label { get; protected set; }

	public State<float> SelectedBorderThickness { get; init; }

	public State<SKColor> BaseColor { get; init; }

	public FLargeSelectButton(FDisplayableType display, FText label, Action? onClick = null, Action<bool, SelectableButton>? onSelectionChanged = null, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(onClick, onSelectionChanged, position, size ?? ((Func<Vector2>)(() => new Vector2(80f, 60f))))
	{
		FLargeSelectButton fLargeSelectButton = this;
		base.RenderMaterial.SetResponsiveState(FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.PanelMaterial);
		base.CornerRadius.SetStaticState(20f);
		BaseColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.Multiply(0.9f).Saturate(0.35f), this, this);
		SelectedBorderThickness = new State<float>(() => 3f, this, this);
		base.ImageEffects.SelfOpacity.SetStaticState(0.38f);
		Display = display;
		display.SetParent(this);
		display.Transform.Size.SetStaticState(new Vector2(25f, 25f));
		Label = label;
		label.SetParent(this);
		label.Transform.Size.SetResponsiveState(delegate
		{
			SKRect boundingRect = label.LayoutModel.GetBoundingRect(label.Model, fLargeSelectButton.Shape.LocalBounds);
			return new Vector2(boundingRect.Width, boundingRect.Height);
		});
		label.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		label.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0f));
		label.Transform.LocalPosition.SetStaticState(new Vector2(0f, 5f));
		FText fText = label;
		TextModel model = label.Model;
		bool? bold = true;
		TextAlign align = new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Middle,
			VerticalAlign = TextAlign.AlignType.Start
		};
		fText.Model = TextModelFactory.CopyBasic(model, null, bold, null, null, null, null, align);
		label.ImageEffects.Opacity.SetResponsiveState(() => (!fLargeSelectButton.IsSelected) ? 0.6f : 0.9f);
	}

	protected override void Update()
	{
		base.Update();
		float other = borderThickness;
		borderThickness = RMath.Lerp(borderThickness, base.IsSelected ? SelectedBorderThickness.CachedValue : 0f, FContext.DeltaTime * 10f);
		if (!RMath.Approximately(borderThickness, other))
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		using (SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, base.CornerRadius.CachedValue))
		{
			base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object> { ["BaseColor"] = (Func<SKColor>)(() => BaseColor.CachedValue) }).DrawWithMaterial(canvas, path, this);
		}
		if (!(borderThickness > 0.1f))
		{
			return;
		}
		SKRect localBounds = base.Shape.LocalBounds;
		localBounds.Inflate(6f, 6f);
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Color = base.EnabledFillColor.CachedValue;
		sKPaint.IsStroke = true;
		sKPaint.StrokeWidth = borderThickness;
		using SKPath path2 = SKSquircle.CreateSquircle(localBounds, base.CornerRadius.CachedValue * 1.5f);
		canvas.DrawPath(path2, sKPaint);
	}
}
