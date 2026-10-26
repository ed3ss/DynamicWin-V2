using System;
using System.Collections.Generic;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects.Buttons;

public class FRoundToggle : SelectableButton, IStateListener
{
	private const int WIDTH = 50;

	private const int HEIGHT = 25;

	protected AnimatorComponent toggleAnimator;

	private float _width = 25f;

	private float _lastWidth = 25f;

	private float _animTime;

	private float _lastAnimTime;

	public State<SKColor> KnobColor { get; set; }

	public Spring AnimationSpring { get; set; }

	public FRoundToggle(Func<Vector2>? position = null)
		: base(null, null, position, () => new Vector2(50f, 25f))
	{
		KnobColor = new State<SKColor>(() => SKColors.White, this, this);
		toggleAnimator = new AnimatorComponent(this, Easing.EaseOutBack);
		toggleAnimator.Duration = 0.5f;
		AnimatorComponent obj = toggleAnimator;
		obj.OnValueUpdate = (Action<float>)Delegate.Combine(obj.OnValueUpdate, new Action<float>(AnimatorValueUpdate));
		base.Padding.SetStaticState(5);
		AnimationSpring = new Spring(2f, 1.75f);
		base.Transform.Size.SetStaticState(new Vector2(50f, 25f));
		base.Transform.LocalPosition.SetResponsiveState(position ?? ((Func<Vector2>)(() => new Vector2(0f, 0f))));
		base.OnSelectionChangedSilent = (Action<bool, SelectableButton>)Delegate.Combine(base.OnSelectionChangedSilent, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			AnimationSpring.ResetVector(new Vector2(x ? 1 : 0, 0f));
		});
		base.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(base.OnSelectionChanged, (Action<bool, SelectableButton>)delegate
		{
			toggleAnimator.Restart();
		});
		base.OnUserSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(base.OnUserSelectionChanged, (Action<bool, SelectableButton>)delegate
		{
			toggleAnimator.Restart();
		});
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseExit = (Action)Delegate.Combine(interactiveSurface.OnMouseExit, new Action(MouseExit));
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: true);
		base.CornerRadius.SetResponsiveState(() => base.Layout.ClampSize(base.Transform.Size.CachedValue).y / 2f);
	}

	private void AnimatorValueUpdate(float t)
	{
		UpdateColors();
		Invalidate(Invalidation.SurfaceDirty);
	}

	protected override void Update()
	{
		base.Update();
		float x = (base.IsSelected ? 1 : 0);
		_animTime = (float)(Math.Round(AnimationSpring.Update(FContext.DeltaTime, new Vector2(x, 0f)).x * 100f) / 100.0);
		_width = RMath.Lerp(_width, base.InteractiveSurface.IsMouseDown ? 30f : 25f, FContext.DeltaTime * 5f);
		if (_lastAnimTime != _animTime)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
		_lastAnimTime = _animTime;
		_lastWidth = _width;
	}

	protected override void MouseExit()
	{
		base.MouseExit();
		base.InteractiveSurface.IsMouseDown = false;
	}

	public override void Render(SKCanvas canvas)
	{
		using SKPaint paint = GetRenderPaint();
		SKRect localBounds = base.Shape.LocalBounds;
		Func<SKColor> prop = base.RenderMaterial.CachedValue.GetProp<Func<SKColor>>("BaseColor", null);
		Func<SKColor> prop2 = base.RenderMaterial.CachedValue.GetProp<Func<SKColor>>("BorderColor", null);
		float t = RMath.Clamp(1f - _animTime, 0f, 1f);
		SKColor currentBackground = RMath.Lerp(base.EnabledFillColor.CachedValue, prop?.Invoke() ?? FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Secondary, t);
		SKColor currentBorder = RMath.Lerp(base.EnabledFillColor.CachedValue.AddMix(new SKColor(25, 25, 25)), prop2?.Invoke() ?? FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.SecondaryBorder, t);
		using SKRoundRect rect = new SKRoundRect(localBounds, base.CornerRadius.CachedValue);
		base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object>
		{
			["BaseColor"] = (Func<SKColor>)(() => currentBackground),
			["BorderColor"] = (Func<SKColor>)(() => currentBorder)
		}).DrawWithMaterial(canvas, rect, this, paint);
		float left = RMath.Lerp(localBounds.Left, localBounds.Right - _width, _animTime);
		float right = RMath.Lerp(localBounds.Left + _width, localBounds.Right, _animTime);
		SKRect rect2 = new SKRect(left, localBounds.Top, right, localBounds.Bottom);
		rect2.Inflate(-2f, -2f);
		using SKRoundRect rect3 = new SKRoundRect(rect2, 20f);
		base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object>
		{
			["BaseColor"] = (Func<SKColor>)(() => KnobColor.CachedValue),
			["BorderColor"] = (Func<SKColor>)(() => SKColors.Transparent)
		}).DrawWithMaterial(canvas, rect3, this, paint);
	}

	public override void AfterRender(SKCanvas canvas)
	{
		using SKRoundRect rect = new SKRoundRect(base.Shape.LocalBounds, base.CornerRadius.CachedValue);
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Color = currentHoverMix;
		canvas.DrawRoundRect(rect, sKPaint);
	}
}
