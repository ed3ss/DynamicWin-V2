using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public class BlurLayoutProcessor : LayoutProcessor
{
	private List<Glyph>? oldLayout;

	private List<SKPoint>? oldLayoutPositions;

	private List<Glyph>? newLayout;

	private AnimatorComponent animatorOut;

	private AnimatorComponent animatorIn;

	public bool LowerQualityOnAnimate = true;

	public float Duration { get; init; } = 0.7f;

	public float BlurRadius { get; init; } = 4f;

	public float GlyphOffset { get; init; } = 5f;

	public float FadeLength { get; init; } = 0.5f;

	public BlurLayoutProcessor(FText parent, TextLayout innerLayout)
		: base(parent, innerLayout)
	{
		parent.OnModelChanged = (Action)Delegate.Combine(parent.OnModelChanged, (Action)delegate
		{
			if (newLayout != null)
			{
				animatorIn?.Break();
				animatorOut?.Break();
				oldLayout = newLayout.ToList();
				oldLayoutPositions = new List<SKPoint>(newLayout.Count);
				foreach (Glyph item in newLayout)
				{
					oldLayoutPositions.Add(item.Position);
				}
				newLayout = null;
				animatorOut?.Restart();
				animatorIn?.Break();
				if (LowerQualityOnAnimate)
				{
					base.Owner.Quality.SetStaticState(0.85f);
				}
			}
		});
		animatorOut = new AnimatorComponent(parent, (float t) => t);
		animatorOut.Duration = Duration;
		AnimatorComponent animatorComponent = animatorOut;
		animatorComponent.OnComplete = (Action)Delegate.Combine(animatorComponent.OnComplete, new Action(OnAnimCompleteOut));
		AnimatorComponent animatorComponent2 = animatorOut;
		animatorComponent2.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent2.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
		animatorIn = new AnimatorComponent(parent, (float t) => t);
		animatorIn.Duration = Duration;
		AnimatorComponent animatorComponent3 = animatorIn;
		animatorComponent3.OnComplete = (Action)Delegate.Combine(animatorComponent3.OnComplete, new Action(OnAnimCompleteIn));
		AnimatorComponent animatorComponent4 = animatorIn;
		animatorComponent4.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent4.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
		AnimatorComponent animatorComponent5 = animatorIn;
		animatorComponent5.OnComplete = (Action)Delegate.Combine(animatorComponent5.OnComplete, (Action)delegate
		{
			base.Owner.Quality.SetStaticState(1f);
		});
	}

	private void OnAnimCompleteOut()
	{
		animatorIn.Restart();
	}

	private void OnAnimCompleteIn()
	{
		oldLayout = null;
	}

	private void OnAnimValueUpdated(float time)
	{
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
	}

	public override List<Glyph> ProcessModel(TextModel model, SKRect bounds)
	{
		newLayout = base.ProcessModel(model, bounds);
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		if (oldLayout != null && newLayout != null)
		{
			float fadeLength = FadeLength;
			int num = Math.Max(oldLayout.Count, newLayout.Count);
			float num2 = (animatorIn.IsRunning ? animatorIn.Time : animatorOut.Time) * (1f + fadeLength);
			Func<float, float> func = Easing.EaseOutQuint;
			Func<float, float> func2 = Easing.EaseInQuint;
			for (int i = 0; i < num; i++)
			{
				float num3 = ((num > 1) ? ((float)i / (float)(num - 1)) : 0f);
				float value = (num2 - num3) / fadeLength;
				value = RMath.Clamp(value, 0f, 1f);
				value = (animatorIn.IsRunning ? Easing.EaseInCubic(value) : Easing.EaseOutCubic(value));
				if (i < oldLayout.Count && animatorOut.IsRunning)
				{
					value = 1f - value;
					float num4 = func2(1f - value);
					float num5 = func(value);
					if (oldLayoutPositions == null)
					{
						continue;
					}
					oldLayout[i].Position = new SKPoint(oldLayoutPositions[i].X, oldLayoutPositions[i].Y - num4 * GlyphOffset);
					oldLayout[i].Style.BlurRadius = num4 * BlurRadius;
					oldLayout[i].Style.Opacity = RMath.Clamp(num5 * 2f, 0f, 1f);
				}
				if (i < newLayout.Count && newLayout[i] != null && animatorIn.IsRunning)
				{
					float num6 = func2(1f - value);
					float num7 = func(value);
					newLayout[i].Position = new SKPoint(newLayout[i].Position.X, newLayout[i].Position.Y - num6 * (0f - GlyphOffset));
					newLayout[i].Style.BlurRadius = num6 * BlurRadius;
					newLayout[i].Style.Opacity = RMath.Clamp(num7 * 2f, 0f, 1f);
				}
				else if (i < newLayout.Count && !animatorIn.IsRunning)
				{
					newLayout[i].Style.Opacity = 0f;
				}
			}
			List<Glyph>? list = oldLayout;
			List<Glyph> list2 = newLayout;
			int num8 = list.Count + list2.Count;
			List<Glyph> list3 = new List<Glyph>(num8);
			CollectionsMarshal.SetCount(list3, num8);
			Span<Glyph> span = CollectionsMarshal.AsSpan(list3);
			int num9 = 0;
			Span<Glyph> span2 = CollectionsMarshal.AsSpan(list);
			span2.CopyTo(span.Slice(num9, span2.Length));
			num9 += span2.Length;
			Span<Glyph> span3 = CollectionsMarshal.AsSpan(list2);
			span3.CopyTo(span.Slice(num9, span3.Length));
			num9 += span3.Length;
			return list3;
		}
		return newLayout ?? base.ProcessModel(model, bounds);
	}
}
