using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Behavior.RuntimeEffects;

public class ImageEffects : BehaviorComponent, IStateListener
{
	public State<float> SelfOpacity { get; init; }

	public State<float> Opacity { get; init; }

	public State<float> BlurRadius { get; init; }

	public State<float> Saturation { get; init; }

	public State<float> Brightness { get; init; }

	public State<bool> InheritFromParent { get; init; }

	public ImageEffects(UIObject owner)
		: base(owner)
	{
		SelfOpacity = new State<float>(() => 1f, base.Owner, this);
		SelfOpacity.SetResolver(StateResolverTemplates.SmallestFloatResolver);
		SelfOpacity.SetProcessor((float x) => RMath.Clamp(x, 0f, 1f));
		Opacity = new State<float>(() => 1f, base.Owner, this);
		Opacity.SetResolver(StateResolverTemplates.SmallestFloatResolver);
		Opacity.SetProcessor((float x) => RMath.Clamp(x, 0f, 1f));
		Saturation = new State<float>(() => 1f, base.Owner, this);
		Saturation.SetProcessor((float x) => RMath.Clamp(x, 0f, 4f));
		Brightness = new State<float>(() => 1f, base.Owner, this);
		Brightness.SetProcessor((float x) => RMath.Clamp(x, 0f, 2f));
		BlurRadius = new State<float>(() => 0f, base.Owner, this);
		BlurRadius.SetResolver(StateResolverTemplates.BiggestFloatResolver);
		BlurRadius.SetProcessor((float x) => Math.Abs(x));
		InheritFromParent = new State<bool>(() => true, base.Owner, this);
		base.Owner.Padding.SetResponsiveState(GetPadding, 10u);
	}

	public override void HandleEvent(BehaviorEventType type, object? data = null)
	{
		base.HandleEvent(type, data);
		UIObject owner = base.Owner;
		if (owner != null && type == BehaviorEventType.AfterRender && owner.ObjectSurface.TryGetSurface(out SKSurface surface))
		{
			ApplyImageEffects(surface, owner.Shape.SurfaceDrawRect);
		}
	}

	public void ApplyImageEffects(SKSurface surface, SKRect bounds)
	{
		if (!AreEffectsApplied())
		{
			return;
		}
		GetValues(out (float, float, float, float) values, isChild: false);
		using SKImage image = surface.Snapshot();
		using SKPaint sKPaint = new SKPaint();
		surface.Canvas.Clear(SKColors.Transparent);
		if (values.Item2 != 0f)
		{
			using SKImageFilter imageFilter = SKImageFilter.CreateBlur(values.Item2, values.Item2);
			sKPaint.ImageFilter = imageFilter;
		}
		if (values.Item3 != 1f)
		{
			float num = 1f - values.Item3;
			float num2 = 0.2126f * num;
			float num3 = 0.7152f * num;
			float num4 = 0.0722f * num;
			using SKColorFilter cf = SKColorFilter.CreateColorMatrix(new float[20]
			{
				num2 + values.Item3,
				num3,
				num4,
				0f,
				0f,
				num2,
				num3 + values.Item3,
				num4,
				0f,
				0f,
				num2,
				num3,
				num4 + values.Item3,
				0f,
				0f,
				0f,
				0f,
				0f,
				1f,
				0f
			});
			using SKImageFilter sKImageFilter = SKImageFilter.CreateColorFilter(cf);
			if (sKPaint.ImageFilter == null)
			{
				sKPaint.ImageFilter = sKImageFilter;
			}
			else
			{
				using SKImageFilter imageFilter2 = SKImageFilter.CreateCompose(sKPaint.ImageFilter, sKImageFilter);
				sKPaint.ImageFilter = imageFilter2;
			}
		}
		if (values.Item4 != 1f)
		{
			using SKColorFilter cf2 = SKColorFilter.CreateColorMatrix(new float[20]
			{
				1f * values.Item4,
				0f,
				0f,
				0f,
				0f,
				0f,
				1f * values.Item4,
				0f,
				0f,
				0f,
				0f,
				0f,
				1f * values.Item4,
				0f,
				0f,
				0f,
				0f,
				0f,
				1f,
				0f
			});
			using SKImageFilter sKImageFilter2 = SKImageFilter.CreateColorFilter(cf2);
			if (sKPaint.ImageFilter == null)
			{
				sKPaint.ImageFilter = sKImageFilter2;
			}
			else
			{
				using SKImageFilter imageFilter3 = SKImageFilter.CreateCompose(sKPaint.ImageFilter, sKImageFilter2);
				sKPaint.ImageFilter = imageFilter3;
			}
		}
		sKPaint.Color = SKColors.White.WithAlpha((byte)(values.Item1 * 255f));
		surface.Canvas.DrawImage(image, bounds, sKPaint);
	}

	public bool AreEffectsApplied()
	{
		GetValues(out (float, float, float, float) values, isChild: false);
		if (values.Item1 == 1f && values.Item2 == 0f && values.Item3 == 1f)
		{
			return values.Item4 != 1f;
		}
		return true;
	}

	public void GetValues(out (float opacity, float blurRadius, float saturation, float brightness) values, bool isChild)
	{
		UIObject owner = base.Owner;
		if (owner?.Parent == null || !InheritFromParent.CachedValue)
		{
			values = (opacity: Opacity.CachedValue * ((!isChild) ? SelfOpacity.CachedValue : 1f), blurRadius: BlurRadius.CachedValue, saturation: Saturation.CachedValue, brightness: Brightness.CachedValue);
			return;
		}
		owner.Parent.ImageEffects.GetValues(out (float, float, float, float) values2, isChild: true);
		float cachedValue = Opacity.CachedValue;
		float num;
		if (InheritFromParent.CachedValue)
		{
			(num, _, _, _) = values2;
		}
		else
		{
			num = 1f;
		}
		values = (opacity: cachedValue * num * ((!isChild) ? SelfOpacity.CachedValue : 1f), blurRadius: Math.Max(BlurRadius.CachedValue, InheritFromParent.CachedValue ? values2.Item2 : 0f), saturation: Saturation.CachedValue * (InheritFromParent.CachedValue ? values2.Item3 : 1f), brightness: Brightness.CachedValue * (InheritFromParent.CachedValue ? values2.Item4 : 1f));
	}

	public int GetPadding()
	{
		return (int)MathF.Max(BlurRadius.CachedValue, 0f);
	}

	public void OnInternalStateChanged<T>(T value)
	{
		base.Owner?.Invalidate(UIObject.Invalidation.SurfaceDirty);
		base.Owner?.Children.ForEach(delegate(UIObject x)
		{
			x.ImageEffects.OnInternalStateChanged(value);
		});
	}
}
