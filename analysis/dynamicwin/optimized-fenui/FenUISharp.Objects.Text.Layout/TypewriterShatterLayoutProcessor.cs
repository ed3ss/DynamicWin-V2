using System;
using System.Collections.Generic;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public class TypewriterShatterLayoutProcessor : LayoutProcessor
{
	private struct ShatterFragment
	{
		public char Character;

		public SKPoint InitialPosition;

		public SKPoint Velocity;

		public float RotationVelocity;

		public float InitialRotation;

		public TextStyle Style;

		public SKSize Size;

		public SKPoint Anchor;

		public SKSize Scale;
	}

	private List<Glyph>? oldLayout;

	private List<ShatterFragment>? shatterFragments;

	private List<Glyph>? newLayout;

	private Random random = new Random();

	private AnimatorComponent shatterAnimator;

	private AnimatorComponent typewriterAnimator;

	public float ShatterDuration { get; init; } = 0.8f;

	public float TypewriterDuration { get; init; } = 0.6f;

	public float TypewriterDelay { get; init; } = 0.2f;

	public int ShatterFragmentCount { get; init; } = 3;

	public float ShatterVelocityMultiplier { get; init; } = 150f;

	public float ShatterRotationSpeed { get; init; } = 720f;

	public float TypewriterCharDelay { get; init; } = 0.05f;

	public bool LowerQualityOnAnimate { get; init; } = true;

	public TypewriterShatterLayoutProcessor(FText parent, TextLayout innerLayout)
		: base(parent, innerLayout)
	{
		parent.OnModelChanged = (Action)Delegate.Combine(parent.OnModelChanged, (Action)delegate
		{
			oldLayout = newLayout;
			newLayout = null;
			CreateShatterFragments();
			shatterAnimator?.Restart();
			typewriterAnimator?.Break();
			if (LowerQualityOnAnimate)
			{
				base.Owner.Quality.SetStaticState(0.8f);
			}
		});
		shatterAnimator = new AnimatorComponent(parent, (float t) => t);
		shatterAnimator.Duration = ShatterDuration;
		AnimatorComponent animatorComponent = shatterAnimator;
		animatorComponent.OnComplete = (Action)Delegate.Combine(animatorComponent.OnComplete, new Action(OnShatterComplete));
		AnimatorComponent animatorComponent2 = shatterAnimator;
		animatorComponent2.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent2.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
		typewriterAnimator = new AnimatorComponent(parent, (float t) => t);
		typewriterAnimator.Duration = TypewriterDuration;
		AnimatorComponent animatorComponent3 = typewriterAnimator;
		animatorComponent3.OnComplete = (Action)Delegate.Combine(animatorComponent3.OnComplete, new Action(OnTypewriterComplete));
		AnimatorComponent animatorComponent4 = typewriterAnimator;
		animatorComponent4.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent4.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
	}

	private void CreateShatterFragments()
	{
		if (oldLayout == null)
		{
			return;
		}
		shatterFragments = new List<ShatterFragment>();
		foreach (Glyph item2 in oldLayout)
		{
			for (int i = 0; i < ShatterFragmentCount; i++)
			{
				ShatterFragment item = new ShatterFragment
				{
					Character = item2.Character,
					InitialPosition = item2.Position,
					Velocity = new SKPoint((float)(random.NextDouble() - 0.5) * ShatterVelocityMultiplier, (float)(random.NextDouble() - 0.8) * ShatterVelocityMultiplier),
					RotationVelocity = (float)(random.NextDouble() - 0.5) * ShatterRotationSpeed,
					InitialRotation = (float)random.NextDouble() * 360f,
					Style = new TextStyle(item2.Style),
					Size = item2.Size,
					Anchor = item2.Anchor,
					Scale = item2.Scale
				};
				shatterFragments.Add(item);
			}
		}
	}

	private void OnShatterComplete()
	{
		typewriterAnimator.Restart();
	}

	private void OnTypewriterComplete()
	{
		oldLayout = null;
		shatterFragments = null;
		base.Owner.Quality.SetStaticState(1f);
	}

	private void OnAnimValueUpdated(float time)
	{
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
	}

	public override List<Glyph> ProcessModel(TextModel model, SKRect bounds)
	{
		newLayout = base.ProcessModel(model, bounds);
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		List<Glyph> list = new List<Glyph>();
		if (shatterFragments != null && shatterAnimator.IsRunning)
		{
			float num = Easing.EaseInQuart(shatterAnimator.Time);
			foreach (ShatterFragment shatterFragment in shatterFragments)
			{
				Glyph glyph = new Glyph(shatterFragment.Character, new SKPoint(shatterFragment.InitialPosition.X + shatterFragment.Velocity.X * num, shatterFragment.InitialPosition.Y + shatterFragment.Velocity.Y * num + 150f * num * num), shatterFragment.Scale, shatterFragment.Anchor, new TextStyle(shatterFragment.Style), shatterFragment.Size);
				glyph.Style.Opacity = RMath.Clamp((1f - num) * 2f, 0f, 1f);
				glyph.Scale = new SKSize(shatterFragment.Scale.Width * (1f - num * 0.5f), shatterFragment.Scale.Height * (1f - num * 0.5f));
				list.Add(glyph);
			}
		}
		if (newLayout != null && typewriterAnimator.IsRunning)
		{
			float time = typewriterAnimator.Time;
			for (int i = 0; i < newLayout.Count; i++)
			{
				Glyph glyph2 = newLayout[i];
				float num2 = (float)i * TypewriterCharDelay;
				float num3 = num2 + TypewriterCharDelay * 2f;
				if (time >= num2)
				{
					Glyph glyph3 = new Glyph(glyph2.Character, glyph2.Position, glyph2.Scale, glyph2.Anchor, new TextStyle(glyph2.Style), glyph2.Size);
					if (time < num3)
					{
						float value = (time - num2) / (num3 - num2);
						value = RMath.Clamp(value, 0f, 1f);
						float num4 = Easing.EaseOutElastic(value);
						glyph3.Scale = new SKSize(glyph2.Scale.Width * (0.3f + 0.7f * num4), glyph2.Scale.Height * (0.3f + 0.7f * num4));
						glyph3.Style.BlurRadius = (1f - value) * 2f;
						glyph3.Style.Opacity = Easing.EaseOutCubic(value);
						glyph3.Position = new SKPoint(glyph2.Position.X, glyph2.Position.Y - (1f - value) * 3f);
					}
					list.Add(glyph3);
				}
			}
		}
		else if (newLayout != null && !typewriterAnimator.IsRunning && !shatterAnimator.IsRunning)
		{
			list.AddRange(newLayout);
		}
		return list;
	}
}
