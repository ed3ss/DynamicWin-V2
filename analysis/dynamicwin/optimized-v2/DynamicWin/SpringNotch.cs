using System;
using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace DynamicWin;

internal class SpringNotch : UIObject
{
	public bool IsNotchMode = true;

	public float InsideMargin = 20f;

	public float HorizontalMarginMultiplier = 1.25f;

	protected float IslandTopOffset = 5f;

	protected float NotchOutwardCornerRadius = 5.5f;

	protected float NotchInwardCornerRadius = 20f;

	protected float NotchInwardCornerRadiusDivisor = 1f;

	private float currentNotchInwardCornerRadius = 20f;

	public State<bool> NotchActive;

	public AnimatorComponent disableAnim;

	private Vector2 _springValues = new Vector2(2.7f, 1.6f);

	private bool globalInsideFlag;

	private float dragLocalXOffset;

	private Vector2 mousePositionBeforeDrag;

	private Spring dragStretchSpring = new Spring(1f, 2f);

	private Spring dragOffsetSpring = new Spring(1f, 2f);

	private float currentDropShadow = 1f;

	public bool isDemoNotch;

	public Spring Spring { get; set; }

	public Vector2 CurrentSize { get; set; } = new Vector2(0f, 0f);

	public Vector2 ContractedSize { get; set; } = new Vector2(35f, 30f);

	public float CornerRadius { get; set; } = 15f;

	public float MarginDivisor { get; set; } = 1f;

	public SpringNotch()
		: base(() => new Vector2(0f, 0f), () => Vector2.Zero)
	{
		disableAnim = new AnimatorComponent(this, Easing.EaseOutCubic);
		disableAnim.Duration = 0.35f;
		AnimatorComponent animatorComponent = disableAnim;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
		{
			base.ImageEffects.BlurRadius.SetStaticState(x * 10f);
			base.ImageEffects.Opacity.SetStaticState(1f - x);
		});
		Action animComplete = delegate
		{
			base.Enabled.SetStaticState(value: false);
			base.ImageEffects.BlurRadius.SetStaticState(0f);
			base.ImageEffects.Opacity.SetStaticState(1f);
		};
		Func<List<StateEntry<Vector2>>, StateEntry<Vector2>> resolver = delegate(List<StateEntry<Vector2>> entries)
		{
			Vector2 addition = Vector2.Zero;
			entries.ForEach(delegate(StateEntry<Vector2> x)
			{
				addition += x.Value();
			});
			return new StateEntry<Vector2>
			{
				Priority = 1u,
				Value = () => addition
			};
		};
		base.Transform.LocalPosition.SetResolver(resolver);
		NotchActive = new State<bool>(() => true, this, delegate
		{
			FContext.GetCurrentDispatcher().Invoke(delegate
			{
				State<bool> notchActive = NotchActive;
				if (notchActive != null && !notchActive.CachedValue)
				{
					disableAnim.Inverse = false;
					disableAnim.OnComplete = animComplete;
					disableAnim.Restart();
				}
				else
				{
					disableAnim.Inverse = true;
					disableAnim.OnComplete = null;
					disableAnim.Restart();
				}
				base.Enabled.SetStaticState(value: true);
				base.ImageEffects.BlurRadius.SetStaticState(0f);
				base.ImageEffects.Opacity.SetStaticState(1f);
			});
		});
		base.Layout.Alignment.SetStaticState(new Vector2(0.5f, 0f));
		base.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0f));
		base.Transform.Anchor.SetStaticState(new Vector2(0.5f, 0f));
		Spring = new Spring(2.7f, 1.6f);
		Spring.ResetVector(CurrentSize);
		base.Padding.SetStaticState(50);
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseMove = (Action<Vector2>)Delegate.Combine(interactiveSurface.OnMouseMove, new Action<Vector2>(MouseMove));
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: false);
		base.InteractiveSurface.ExtendInteractionRadius.SetStaticState(5);
		base.InteractiveSurface.IgnoreChildInteractions.SetStaticState(value: true);
		Settings.SetListenerForSettingChange("notch.spring-finetune", OptionChanged);
		Settings.SetListenerForSettingChange("notch.speed-finetune", OptionChanged);
		OptionChanged();
	}

	private void OptionChanged()
	{
		float setting = Settings.GetSetting("notch.spring-finetune", 1.6f);
		float setting2 = Settings.GetSetting("notch.speed-finetune", 2f);
		_springValues = new Vector2(setting2, setting);
		Invalidate(Invalidation.SurfaceDirty);
	}

	protected void MouseMove(Vector2 pos)
	{
		SKRect globalInteractionRect = base.InteractiveSurface.GetGlobalInteractionRect();
		SKRect globalInteractionRect2 = base.InteractiveSurface.GetGlobalInteractionRect();
		globalInteractionRect2.Inflate(2.5f, 2.5f);
		if (!globalInsideFlag && globalInteractionRect.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)))
		{
			globalInsideFlag = true;
			CurrentSize = ContractedSize * 1.4f;
		}
		if (!globalInteractionRect2.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)))
		{
			globalInsideFlag = false;
		}
	}

	protected override void EarlyUpdate()
	{
		base.EarlyUpdate();
		base.Transform.Scale.SetProcessor((Vector2 x) => dragStretchSpring.Update(FContext.DeltaTime, x * 1000f) / 1000f);
		if (base.InteractiveSurface.IsMouseDown)
		{
			Vector2 vector = FContext.GetCurrentWindow().ClientMousePosition - mousePositionBeforeDrag;
			dragLocalXOffset = dragOffsetSpring.Update(FContext.DeltaTime, vector / 125f).x;
			vector /= 7500f;
			vector.x = MathF.Abs(vector.x);
			vector.y *= 1.5f;
			base.Transform.Scale.SetStaticState(Vector2.Clamp(Vector2.One + vector, Vector2.One * 0.75f, Vector2.One * 1.35f));
		}
		else
		{
			dragLocalXOffset = dragOffsetSpring.Update(FContext.DeltaTime, Vector2.Zero).x;
			mousePositionBeforeDrag = FContext.GetCurrentWindow().ClientMousePosition;
			base.Transform.Scale.SetStaticState(Vector2.One);
		}
		base.Transform.LocalPosition.SetStaticState(new Vector2(dragLocalXOffset, 0f), 10u);
		if (isDemoNotch)
		{
			base.Padding.SetStaticState(80);
		}
		Spring.SetValues(_springValues.x, globalInsideFlag ? _springValues.y : Math.Clamp(_springValues.y / 1.4f, 0f, 2f));
		float notchInwardCornerRadius = NotchInwardCornerRadius;
		currentNotchInwardCornerRadius = RMath.Lerp(currentNotchInwardCornerRadius, notchInwardCornerRadius, FContext.DeltaTime * 10f);
		base.Transform.LocalPosition.SetStaticState(new Vector2(base.Transform.LocalPosition.CachedValue.x, IsNotchMode ? 0f : IslandTopOffset));
		Vector2 cachedValue = base.Transform.Size.CachedValue;
		base.Transform.Size.SetStaticState(Spring.Update(FContext.DeltaTime, CurrentSize));
		CurrentSize = ContractedSize;
		if (globalInsideFlag)
		{
			CurrentSize *= 1.05f;
		}
		Vector2 contractedSize = ContractedSize;
		SKRect currentRegion = SKRect.Create((0f - contractedSize.x) / 2f + base.Transform.LocalPosition.CachedValue.x, 0f + base.Transform.LocalPosition.CachedValue.y, contractedSize.x, contractedSize.y);
		int num = 17;
		currentRegion.Inflate(num, num);
		currentRegion = base.Transform.LocalToGlobal(currentRegion);
		FContext.GetCurrentWindow().Shape.AddOrUpdateWindowRegion(this, () => currentRegion);
		if (Math.Round(cachedValue.Magnitude * 10f) / 10.0 != Math.Round(base.Transform.Size.CachedValue.Magnitude * 10f) / 10.0)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		FContext.GetCurrentWindow().Shape.DissolveWindowRegion(this);
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint sKPaint = GetRenderPaint();
		canvas.Translate(0f, -1f);
		sKPaint.Color = SKColors.Black;
		SKRect localBounds = base.Shape.LocalBounds;
		localBounds.Inflate(75f, 75f);
		localBounds.Offset(0f, 75f);
		canvas.ClipRect(localBounds);
		if (isDemoNotch)
		{
			SKPaint sKPaint2 = sKPaint.Clone();
			sKPaint2.Color = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.WithAlpha(25);
			sKPaint2.IsStroke = true;
			sKPaint2.StrokeWidth = 2f;
			sKPaint2.PathEffect = SKPathEffect.CreateDash(new float[2] { 7f, 7f }, 0f);
			using (SKRoundRect rect = new SKRoundRect(base.Transform.GlobalToDrawLocal(base.InteractiveSurface.GetGlobalInteractionRect()), base.InteractiveSurface.ExtendInteractionRadius.CachedValue))
			{
				canvas.DrawRoundRect(rect, sKPaint2);
			}
			sKPaint2.Dispose();
		}
		currentDropShadow = RMath.Lerp(currentDropShadow, GetDropShadowValue(), 8f * FContext.DeltaTime);
		using (SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 3f, currentDropShadow, currentDropShadow, SKColors.Black.WithAlpha(100)))
		{
			sKPaint.ImageFilter = imageFilter;
		}
		if (IsNotchMode)
		{
			using SKPath path = OutwardsCornerRoundingPath(base.Shape.LocalBounds.Top, base.Shape.LocalBounds.Left, base.Shape.LocalBounds.Width, base.Shape.LocalBounds.Height);
			canvas.DrawPath(path, sKPaint);
		}
		SKRect localBounds2 = base.Shape.LocalBounds;
		if (IsNotchMode)
		{
			localBounds2.Inflate(0f, currentNotchInwardCornerRadius);
			localBounds2.Offset(0f, 0f - currentNotchInwardCornerRadius);
		}
		using SKPath path2 = SKSquircle.CreateSquircle(localBounds2, currentNotchInwardCornerRadius, 0.6f);
		canvas.DrawPath(path2, sKPaint);
	}

	private float GetDropShadowValue()
	{
		return RMath.Remap(globalInsideFlag ? 0.25f : 0f, 0f, 1f, 10f, 20f);
	}

	protected SKPath OutwardsCornerRoundingPath(float top, float left, float width, float height)
	{
		SKPath sKPath = new SKPath();
		float notchOutwardCornerRadius = NotchOutwardCornerRadius;
		float num = Math.Min(NotchOutwardCornerRadius, height / 2f);
		float num2 = 0f;
		float num3 = 8f;
		sKPath.MoveTo(left, top + num);
		sKPath.CubicTo(new SKPoint(left, top + num), new SKPoint(left - notchOutwardCornerRadius / num3, top + num2), new SKPoint(left - notchOutwardCornerRadius, top));
		sKPath.LineTo(left + width + notchOutwardCornerRadius, top);
		sKPath.CubicTo(new SKPoint(left + width + notchOutwardCornerRadius, top), new SKPoint(left + width + notchOutwardCornerRadius / num3, top + num2), new SKPoint(left + width, top + num));
		sKPath.LineTo(left, top + num);
		sKPath.Close();
		return sKPath;
	}
}
