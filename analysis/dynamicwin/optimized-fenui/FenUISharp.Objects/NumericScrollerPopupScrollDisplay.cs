using System;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

internal class NumericScrollerPopupScrollDisplay : UIObject, IStateListener
{
	private float smoothedValue;

	private FDisplayButton leftArrow;

	private FDisplayButton rightArrow;

	private float? startValue;

	public State<float> ReferenceValue { get; init; }

	public State<float> StepValue { get; init; }

	public Spring ValueSpring { get; set; }

	public int Lines { get; set; } = 7;

	public float LineThickness { get; set; } = 2f;

	public NumericScrollerPopupScrollDisplay(Func<float> value, Func<float> step, Action? leftArrowClick = null, Action? rightArrowClick = null)
	{
		base.Layout.StretchHorizontal.SetStaticState(value: true);
		base.Layout.StretchVertical.SetStaticState(value: true);
		ValueSpring = new Spring(2.25f, 2f);
		ReferenceValue = new State<float>(value, this, this);
		StepValue = new State<float>(step, this, this);
		leftArrow = new FDisplayButton(new FImage(() => Resources.GetImage("fenui-builtin-arrow-left")), leftArrowClick, null, () => new Vector2(13.333333f, 20f));
		leftArrow.Transform.LocalPosition.SetStaticState(new Vector2(3.5f, 0f));
		leftArrow.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		leftArrow.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		leftArrow.SetParent(this);
		rightArrow = new FDisplayButton(new FImage(() => Resources.GetImage("fenui-builtin-arrow-right")), rightArrowClick, null, () => new Vector2(13.333333f, 20f));
		rightArrow.Transform.LocalPosition.SetStaticState(new Vector2(-3.5f, 0f));
		rightArrow.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		rightArrow.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		rightArrow.SetParent(this);
		rightArrow.Display.TintColor.SetResponsiveState(() => (!rightArrow.InteractiveSurface.IsMouseHovering) ? SKColors.Transparent : FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface);
		leftArrow.Display.TintColor.SetResponsiveState(() => (!leftArrow.InteractiveSurface.IsMouseHovering) ? SKColors.Transparent : FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface);
		InteractiveSurface interactiveSurface = leftArrow.InteractiveSurface;
		interactiveSurface.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface.OnMouseEnter, (Action)delegate
		{
			Invalidate(Invalidation.SurfaceDirty);
		});
		InteractiveSurface interactiveSurface2 = leftArrow.InteractiveSurface;
		interactiveSurface2.OnMouseExit = (Action)Delegate.Combine(interactiveSurface2.OnMouseExit, (Action)delegate
		{
			Invalidate(Invalidation.SurfaceDirty);
		});
		InteractiveSurface interactiveSurface3 = rightArrow.InteractiveSurface;
		interactiveSurface3.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface3.OnMouseEnter, (Action)delegate
		{
			Invalidate(Invalidation.SurfaceDirty);
		});
		InteractiveSurface interactiveSurface4 = rightArrow.InteractiveSurface;
		interactiveSurface4.OnMouseExit = (Action)Delegate.Combine(interactiveSurface4.OnMouseExit, (Action)delegate
		{
			Invalidate(Invalidation.SurfaceDirty);
		});
		leftArrow.RenderMaterial.Value = () => new InteractableDefaultMaterial
		{
			BaseColor = () => SKColors.Transparent,
			BorderColor = () => SKColors.Transparent,
			ShadowColor = () => SKColors.Transparent,
			HightlightColor = () => SKColors.Transparent
		};
		rightArrow.RenderMaterial.Value = () => new InteractableDefaultMaterial
		{
			BaseColor = () => SKColors.Transparent,
			BorderColor = () => SKColors.Transparent,
			ShadowColor = () => SKColors.Transparent,
			HightlightColor = () => SKColors.Transparent
		};
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
	}

	protected override void Update()
	{
		base.Update();
		if (!startValue.HasValue)
		{
			ValueSpring.ResetVector(new Vector2(ReferenceValue.CachedValue / StepValue.CachedValue, 0f));
			smoothedValue = ValueSpring.Update(FContext.DeltaTime, new Vector2(ReferenceValue.CachedValue / StepValue.CachedValue, 0f)).x;
			startValue = ReferenceValue.CachedValue / StepValue.CachedValue;
		}
		float num = smoothedValue;
		smoothedValue = ValueSpring.Update(FContext.DeltaTime, new Vector2(ReferenceValue.CachedValue / StepValue.CachedValue, 0f)).x;
		if (num != smoothedValue)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		int count = canvas.SaveLayer();
		SKRect localBounds = base.Shape.LocalBounds;
		canvas.ClipRect(localBounds);
		using SKPaint sKPaint = GetRenderPaint();
		for (int i = 0; i < Lines + 2; i++)
		{
			float num = localBounds.Left + localBounds.Width / (float)Lines * ((float)i + (0f - smoothedValue) % 1f - 1f + 0.5f) - LineThickness / 2f;
			float num2 = Math.Clamp(1f - GetDistance(localBounds.MidX, num) / 10f, 0f, 1f);
			float num3 = localBounds.Height / 2f + num2 * 10f;
			sKPaint.Color = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha((byte)(num2 * 128f + 127f));
			float y = localBounds.MidY - (num3 - num2 * 10f) / 2f + 1f;
			using SKRoundRect rect = new SKRoundRect(SKRect.Create(new SKPoint(num, y), new SKSize(LineThickness, num3)), 5f);
			canvas.DrawRoundRect(rect, sKPaint);
		}
		sKPaint.Color = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface;
		float x = RMath.Clamp(localBounds.MidX + (startValue.GetValueOrDefault() - smoothedValue) * localBounds.Width / (float)Lines, localBounds.Left + 7.5f, localBounds.Right - 7.5f);
		canvas.DrawCircle(new SKPoint(x, localBounds.Top + 3f), 2f, sKPaint);
		using SKPaint sKPaint2 = GetRenderPaint();
		using SKShader shader = SKShader.CreateLinearGradient(new SKPoint(localBounds.Left + (float)(leftArrow.InteractiveSurface.IsMouseHovering ? 20 : 0), localBounds.MidY), new SKPoint(localBounds.Right - (float)(rightArrow.InteractiveSurface.IsMouseHovering ? 20 : 0), localBounds.MidY), new SKColor[3]
		{
			SKColors.Transparent,
			SKColors.White,
			SKColors.Transparent
		}, new float[3] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp);
		sKPaint2.BlendMode = SKBlendMode.DstIn;
		sKPaint2.Shader = shader;
		canvas?.DrawRect(base.Shape.LocalBounds, sKPaint2);
		canvas?.RestoreToCount(count);
	}

	private float GetDistance(float one, float two)
	{
		return MathF.Abs(two - one);
	}
}
