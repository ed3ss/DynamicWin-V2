using System;
using System.Collections.Generic;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public class TypewriterLayoutProcessor : LayoutProcessor
{
	private class TypewriterGlyphState
	{
		public float StartTime;

		public float BounceOffset;

		public float RotationOffset;

		public float ScaleMultiplier;

		public bool HasAppeared;

		public SKPoint OriginalPosition;
	}

	private List<Glyph>? currentLayout;

	private List<TypewriterGlyphState>? glyphStates;

	private AnimatorComponent typewriterAnimator;

	private Random random = new Random();

	public bool LowerQualityOnAnimate = true;

	public float Duration { get; init; } = 2f;

	public float CharacterDelay { get; init; } = 0.08f;

	public float MechanicalBounce { get; init; } = 3f;

	public float RotationVariance { get; init; } = 2f;

	public float ScaleImpact { get; init; } = 0.15f;

	public float TypewriterSound { get; init; } = 0.5f;

	public bool RandomizeTimings { get; init; } = true;

	public TypewriterLayoutProcessor(FText parent, TextLayout innerLayout)
		: base(parent, innerLayout)
	{
		parent.OnModelChanged = (Action)Delegate.Combine(parent.OnModelChanged, new Action(OnModelChanged));
		typewriterAnimator = new AnimatorComponent(parent, (float t) => t);
		typewriterAnimator.Duration = Duration;
		AnimatorComponent animatorComponent = typewriterAnimator;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
		AnimatorComponent animatorComponent2 = typewriterAnimator;
		animatorComponent2.OnComplete = (Action)Delegate.Combine(animatorComponent2.OnComplete, new Action(OnAnimComplete));
	}

	private void OnModelChanged()
	{
		currentLayout = null;
		glyphStates = null;
		typewriterAnimator.Restart();
		if (LowerQualityOnAnimate)
		{
			base.Owner.Quality.SetStaticState(0.9f);
		}
	}

	private void OnAnimValueUpdated(float time)
	{
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
	}

	private void OnAnimComplete()
	{
		base.Owner.Quality.SetStaticState(1f);
	}

	public override List<Glyph> ProcessModel(TextModel model, SKRect bounds)
	{
		currentLayout = base.ProcessModel(model, bounds);
		if (currentLayout == null || currentLayout.Count == 0)
		{
			return currentLayout ?? new List<Glyph>();
		}
		if (glyphStates == null || glyphStates.Count != currentLayout.Count)
		{
			InitializeGlyphStates();
		}
		if (typewriterAnimator.IsRunning)
		{
			ProcessTypewriterAnimation();
		}
		return currentLayout;
	}

	private void InitializeGlyphStates()
	{
		if (currentLayout == null)
		{
			return;
		}
		glyphStates = new List<TypewriterGlyphState>();
		for (int i = 0; i < currentLayout.Count; i++)
		{
			Glyph glyph = currentLayout[i];
			TypewriterGlyphState typewriterGlyphState = new TypewriterGlyphState
			{
				OriginalPosition = glyph.Position,
				HasAppeared = false
			};
			float num = (float)i * CharacterDelay;
			if (RandomizeTimings)
			{
				num += (float)(random.NextDouble() - 0.5) * CharacterDelay * 0.3f;
			}
			typewriterGlyphState.StartTime = Math.Max(0f, num / Duration);
			typewriterGlyphState.BounceOffset = (float)(random.NextDouble() - 0.5) * MechanicalBounce * 0.5f;
			typewriterGlyphState.RotationOffset = (float)(random.NextDouble() - 0.5) * RotationVariance;
			typewriterGlyphState.ScaleMultiplier = 1f + (float)(random.NextDouble() - 0.5) * 0.1f;
			glyphStates.Add(typewriterGlyphState);
		}
	}

	private void ProcessTypewriterAnimation()
	{
		if (currentLayout == null || glyphStates == null)
		{
			return;
		}
		float time = typewriterAnimator.Time;
		for (int i = 0; i < currentLayout.Count; i++)
		{
			Glyph glyph = currentLayout[i];
			TypewriterGlyphState typewriterGlyphState = glyphStates[i];
			if (time >= typewriterGlyphState.StartTime)
			{
				float value = (time - typewriterGlyphState.StartTime) / (CharacterDelay / Duration);
				value = RMath.Clamp(value, 0f, 1f);
				if (!typewriterGlyphState.HasAppeared)
				{
					typewriterGlyphState.HasAppeared = true;
				}
				ApplyTypewriterEffects(glyph, typewriterGlyphState, value);
			}
			else
			{
				glyph.Style.Opacity = 0f;
			}
		}
	}

	private void ApplyTypewriterEffects(Glyph glyph, TypewriterGlyphState state, float progress)
	{
		Func<float, float> func = Easing.EaseOutBounce;
		Func<float, float> func2 = Easing.EaseOutBack;
		Func<float, float> func3 = Easing.EaseOutQuart;
		glyph.Style.Opacity = func3(progress);
		float num = func(progress);
		float num2 = (1f - num) * (MechanicalBounce + state.BounceOffset);
		glyph.Position = new SKPoint(state.OriginalPosition.X, state.OriginalPosition.Y - num2);
		float num3 = func2(progress);
		float num4 = 1f + (1f - num3) * ScaleImpact * state.ScaleMultiplier;
		glyph.Scale = new SKSize(num4, num4);
		if (progress < 0.3f)
		{
			float x = progress / 0.3f;
			Easing.EaseOutCubic(x);
			_ = state.RotationOffset;
		}
		if (progress < 0.2f)
		{
			float x2 = progress / 0.2f;
			glyph.Style.BlurRadius = (1f - Easing.EaseOutQuad(x2)) * TypewriterSound;
		}
		else
		{
			glyph.Style.BlurRadius = 0f;
		}
		if (progress < 0.1f)
		{
			float x3 = progress / 0.1f;
			Easing.EaseOutQuad(x3);
		}
	}
}
