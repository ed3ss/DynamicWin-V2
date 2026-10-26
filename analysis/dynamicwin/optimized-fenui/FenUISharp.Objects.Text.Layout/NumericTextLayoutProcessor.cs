using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public class NumericTextLayoutProcessor : LayoutProcessor
{
	private enum GlyphTransitionType
	{
		Unchanged,
		Moving,
		Departing,
		Arriving
	}

	private List<Glyph>? oldLayout;

	private List<Glyph>? newLayout;

	private Dictionary<char, SKPoint> oldGlyphPositions;

	private Dictionary<char, int> oldGlyphCounts;

	private Dictionary<char, int> newGlyphCounts;

	private AnimatorComponent mainAnimator;

	private Dictionary<char, Spring> positionSprings;

	private Dictionary<char, Spring> scaleSpringsDeparting;

	private Dictionary<char, Spring> scaleStringArriving;

	private int totalGlyphs;

	private float totalDelay;

	public float Duration { get; init; } = 0.6f;

	public float DurationLengthAddition { get; init; } = 0.1f;

	public float BlurRadius { get; init; } = 10f;

	public float VerticalOffset { get; init; } = -10f;

	public float SpringSpeed { get; init; } = 3f;

	public float SpringSpringiness { get; init; } = 2f;

	public float ScaleDownAmount { get; init; } = 0.4f;

	public float GlyphDelay { get; init; } = 0.05f;

	public Func<float, float> VerticalPositionEasing { get; init; } = Easing.CombineInOut(Easing.EaseInBackDramatic, Easing.EaseOutBackDramatic);

	public Func<float, float> SlidePositionEasing { get; init; } = Easing.EaseOutExpo;

	public Func<float, float> FadeEasing { get; init; } = Easing.EaseInCubic;

	public bool LowerQualityOnAnimate { get; init; }

	public NumericTextLayoutProcessor(FText parent, TextLayout innerLayout)
		: base(parent, innerLayout)
	{
		oldGlyphPositions = new Dictionary<char, SKPoint>();
		oldGlyphCounts = new Dictionary<char, int>();
		newGlyphCounts = new Dictionary<char, int>();
		positionSprings = new Dictionary<char, Spring>();
		scaleSpringsDeparting = new Dictionary<char, Spring>();
		scaleStringArriving = new Dictionary<char, Spring>();
		parent.OnModelChanged = (Action)Delegate.Combine(parent.OnModelChanged, new Action(OnModelChanged));
		mainAnimator = new AnimatorComponent(parent, (float t) => t);
		AnimatorComponent animatorComponent = mainAnimator;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
		AnimatorComponent animatorComponent2 = mainAnimator;
		animatorComponent2.OnComplete = (Action)Delegate.Combine(animatorComponent2.OnComplete, new Action(OnAnimComplete));
	}

	private void OnModelChanged()
	{
		if (newLayout != null)
		{
			float num = 1f;
			int count = newLayout.Count;
			float val = GlyphDelay * (float)Math.Max(0, count - 1) + num;
			mainAnimator.Duration = Math.Max(Duration, val);
			oldLayout = new List<Glyph>(newLayout.Select((Glyph g) => new Glyph(g.Character, g.Position, g.Scale, g.Anchor, new TextStyle(g.Style), g.Size)));
			oldGlyphPositions.Clear();
			oldGlyphCounts.Clear();
			foreach (Glyph item in oldLayout)
			{
				oldGlyphPositions[item.Character] = item.Position;
				oldGlyphCounts[item.Character] = oldGlyphCounts.GetValueOrDefault(item.Character, 0) + 1;
			}
		}
		newLayout = null;
		positionSprings.Clear();
		scaleSpringsDeparting.Clear();
		scaleStringArriving.Clear();
		mainAnimator.Restart();
		if (LowerQualityOnAnimate)
		{
			base.Owner.Quality.SetStaticState(0.85f);
		}
	}

	private void OnAnimValueUpdated(float time)
	{
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
	}

	private void OnAnimComplete()
	{
		oldLayout = null;
		base.Owner.Quality.SetStaticState(1f);
	}

	private float GetGlyphTime(float animationTime, int index)
	{
		if (totalGlyphs <= 1 || GlyphDelay <= 0f)
		{
			return animationTime;
		}
		float num = (float)index * GlyphDelay;
		float num2 = (float)(totalGlyphs - 1) * GlyphDelay;
		if (num2 > 1f)
		{
			float num3 = 1f / num2;
			num = (float)index * GlyphDelay * num3;
		}
		return RMath.Clamp((animationTime - num) / (1f - num), 0f, 1f);
	}

	private GlyphTransitionType GetTransitionType(Glyph oldGlyph, Glyph newGlyph, int oldIndex, int newIndex)
	{
		if (oldGlyph == null && newGlyph != null)
		{
			return GlyphTransitionType.Arriving;
		}
		if (oldGlyph != null && newGlyph == null)
		{
			return GlyphTransitionType.Departing;
		}
		if (oldGlyph != null && newGlyph != null)
		{
			if (oldGlyph.Character == newGlyph.Character && oldIndex == newIndex)
			{
				return GlyphTransitionType.Unchanged;
			}
			return GlyphTransitionType.Moving;
		}
		return GlyphTransitionType.Unchanged;
	}

	public override List<Glyph> ProcessModel(TextModel model, SKRect bounds)
	{
		newLayout = base.ProcessModel(model, bounds);
		totalGlyphs = newLayout.Count;
		totalDelay = GlyphDelay * (float)totalGlyphs;
		if (oldLayout == null)
		{
			return newLayout;
		}
		newGlyphCounts.Clear();
		foreach (Glyph item in newLayout)
		{
			newGlyphCounts[item.Character] = newGlyphCounts.GetValueOrDefault(item.Character, 0) + 1;
		}
		float animationTime = Easing.EaseOutCubic(mainAnimator.Time);
		List<Glyph> list = new List<Glyph>();
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < oldLayout.Count; i++)
		{
			Glyph glyph = oldLayout[i];
			Glyph newGlyph = null;
			int newIndex = -1;
			for (int j = 0; j < newLayout.Count; j++)
			{
				if (newLayout[j].Character == glyph.Character && !hashSet.Contains(j))
				{
					newGlyph = newLayout[j];
					newIndex = j;
					hashSet.Add(j);
					break;
				}
			}
			GlyphTransitionType transitionType = GetTransitionType(glyph, newGlyph, i, newIndex);
			float glyphTime = GetGlyphTime(animationTime, i);
			Glyph glyph2 = ProcessGlyph(glyph, newGlyph, transitionType, glyphTime, i);
			if (glyph2 != null)
			{
				list.Add(glyph2);
			}
		}
		for (int k = 0; k < newLayout.Count; k++)
		{
			if (!hashSet.Contains(k))
			{
				Glyph newGlyph2 = newLayout[k];
				float glyphTime2 = GetGlyphTime(animationTime, k);
				Glyph glyph3 = ProcessGlyph(null, newGlyph2, GlyphTransitionType.Arriving, glyphTime2, k);
				if (glyph3 != null)
				{
					list.Add(glyph3);
				}
			}
		}
		return list;
	}

	private Glyph? ProcessGlyph(Glyph? oldGlyph, Glyph? newGlyph, GlyphTransitionType type, float t, int index)
	{
		switch (type)
		{
		case GlyphTransitionType.Unchanged:
			return newGlyph;
		case GlyphTransitionType.Moving:
		{
			if (oldGlyph == null || newGlyph == null)
			{
				return newGlyph;
			}
			Vector2 vector = new Vector2(oldGlyph.Position.X, oldGlyph.Position.Y);
			Vector2 to = new Vector2(newGlyph.Position.X, newGlyph.Position.Y);
			Vector2 vector2 = Vector2.Lerp(vector, to, SlidePositionEasing(t));
			return new Glyph(newGlyph.Character, new SKPoint(vector2.x, vector2.y), newGlyph.Scale, newGlyph.Anchor, new TextStyle(newGlyph.Style), newGlyph.Size);
		}
		case GlyphTransitionType.Departing:
		{
			if (oldGlyph == null)
			{
				return null;
			}
			float num4 = VerticalPositionEasing(t);
			float num5 = FadeEasing(t);
			SKPoint position2 = new SKPoint(oldGlyph.Position.X, oldGlyph.Position.Y - num4 * VerticalOffset);
			float num6 = RMath.Lerp(1f, ScaleDownAmount, num5);
			return new Glyph(oldGlyph.Character, position2, new SKSize(num6, num6), oldGlyph.Anchor, new TextStyle(oldGlyph.Style), oldGlyph.Size)
			{
				Style = 
				{
					BlurRadius = num5 * BlurRadius,
					Opacity = 1f - num5
				}
			};
		}
		case GlyphTransitionType.Arriving:
		{
			if (newGlyph == null)
			{
				return null;
			}
			float num = VerticalPositionEasing(1f - t);
			float num2 = FadeEasing(1f - t);
			SKPoint position = new SKPoint(newGlyph.Position.X, newGlyph.Position.Y + num * VerticalOffset);
			float num3 = RMath.Lerp(1f, ScaleDownAmount, num2);
			return new Glyph(newGlyph.Character, position, new SKSize(num3, num3), newGlyph.Anchor, new TextStyle(newGlyph.Style), newGlyph.Size)
			{
				Style = 
				{
					BlurRadius = num2 * BlurRadius,
					Opacity = 1f - num2
				}
			};
		}
		default:
			return newGlyph;
		}
	}
}
