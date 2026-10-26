using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace FenUISharp.AnimatedVectors;

internal class FAVAnimator
{
	private static Dictionary<string, float> DefaultAttributeValues = new Dictionary<string, float>
	{
		["anchor-x"] = 0.5f,
		["anchor-y"] = 0.5f,
		["scale-x"] = 1f,
		["scale-y"] = 1f,
		["translate-x"] = 0f,
		["translate-y"] = 0f,
		["rotate"] = 0f,
		["opacity"] = 1f,
		["blur-radius"] = 0f,
		["stroke-trace"] = 1f
	};

	private List<(int, AnimatorComponent anim, Action<bool> resetVals)> animators = new List<(int, AnimatorComponent, Action<bool>)>();

	public void StopAnimation()
	{
		animators.ToList().ForEach(delegate((int, AnimatorComponent anim, Action<bool> resetVals) x)
		{
			x.resetVals(obj: true);
			x.anim.Break();
			x.anim.Dispose();
		});
		animators = new List<(int, AnimatorComponent, Action<bool>)>();
	}

	public void PlayAnimation(FAVDisplay display, AnimatedVector animatedVector, string id, Action? onComplete = null)
	{
		int activeAnimators = 0;
		foreach (var animation2 in animatedVector.Animations)
		{
			if (animation2.id != id)
			{
				continue;
			}
			AVAnimation animation = animation2.animation;
			Func<float, float> AnimationEasing = animation.CreateEasing();
			AnimatorComponent animComponent = new AnimatorComponent(display, (float x) => x);
			animComponent.Duration = animation.Duration + animation.ExtendDuration;
			AnimatorComponent animatorComponent = animComponent;
			animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
			{
				Func<float> func = () => (animComponent.Duration - animation.ExtendDuration <= 0f) ? 1f : Math.Clamp(animComponent.TimePassed / (animComponent.Duration - animation.ExtendDuration), 0f, 1f);
				x = (animation.PerKeyframeEase ? func() : AnimationEasing(func()));
				(AVKeyframe, AVKeyframe) tuple = FindInterpolationNeighbors(animation.Keyframes, x);
				float num2 = RMath.Remap(x, tuple.Item1?.time ?? 0f, tuple.Item2?.time ?? 0f, 0f, 1f);
				List<(string, object)> fromAttrs = tuple.Item1?.attributes;
				List<(string, object)> toAttrs = tuple.Item2?.attributes;
				float t = (animation.PerKeyframeEase ? AnimationEasing(num2) : num2);
				float num3 = InterpolateAttribute(fromAttrs, toAttrs, "anchor-x", t);
				float num4 = InterpolateAttribute(fromAttrs, toAttrs, "anchor-y", t);
				float num5 = InterpolateAttribute(fromAttrs, toAttrs, "scale-x", t);
				float num6 = InterpolateAttribute(fromAttrs, toAttrs, "scale-y", t);
				float num7 = InterpolateAttribute(fromAttrs, toAttrs, "translate-x", t);
				float num8 = InterpolateAttribute(fromAttrs, toAttrs, "translate-y", t);
				float num9 = InterpolateAttribute(fromAttrs, toAttrs, "rotate", t);
				float num10 = RMath.Clamp(InterpolateAttribute(fromAttrs, toAttrs, "opacity", t), 0f, 1f);
				float num11 = MathF.Abs(InterpolateAttribute(fromAttrs, toAttrs, "blur-radius", t));
				float num12 = RMath.Clamp(InterpolateAttribute(fromAttrs, toAttrs, "stroke-trace", t), 0f, 1f);
				for (int num13 = 0; num13 < animatedVector.Paths.Count; num13++)
				{
					if (animation.AffectedPathIDs.Contains(num13))
					{
						_ = animatedVector.Paths[num13];
						display.GetOrCreatePathOverride(num13, out AVPathAnimationOverride ovrd);
						if (num3 != DefaultAttributeValues["anchor-x"])
						{
							ovrd.Anchor.x = num3;
						}
						if (num4 != DefaultAttributeValues["anchor-y"])
						{
							ovrd.Anchor.y = num4;
						}
						if (num5 != DefaultAttributeValues["scale-x"])
						{
							ovrd.Scale.x = num5;
						}
						if (num6 != DefaultAttributeValues["scale-y"])
						{
							ovrd.Scale.y = num6;
						}
						if (num7 != DefaultAttributeValues["translate-x"])
						{
							ovrd.Translation.x = num7;
						}
						if (num8 != DefaultAttributeValues["translate-y"])
						{
							ovrd.Translation.y = num8;
						}
						if (num9 != DefaultAttributeValues["rotate"])
						{
							ovrd.Rotation = num9;
						}
						if (num10 != DefaultAttributeValues["opacity"])
						{
							ovrd.Opacity = num10;
						}
						if (num11 != DefaultAttributeValues["blur-radius"])
						{
							ovrd.BlurRadius = num11;
						}
						if (num12 != DefaultAttributeValues["stroke-trace"])
						{
							ovrd.StrokeTrace = num12;
						}
						if (animation.UseObjectAnchor)
						{
							ovrd.UseObjectAnchor = animation.UseObjectAnchor;
						}
						if (animation.UseObjectSizeTranslation)
						{
							ovrd.UseObjectSizeTranslation = animation.UseObjectSizeTranslation;
						}
					}
				}
				display.Invalidate(UIObject.Invalidation.SurfaceDirty);
			});
			Action<bool> resetValues = delegate(bool isEarlyEnding)
			{
				if (!animation.DontResetValues)
				{
					for (int i = 0; i < animatedVector.Paths.Count; i++)
					{
						if (animation.AffectedPathIDs.Contains(i))
						{
							_ = animatedVector.Paths[i];
							display.GetOrCreatePathOverride(i, out AVPathAnimationOverride ovrd);
							ovrd.Anchor = new Vector2(DefaultAttributeValues["anchor-x"], DefaultAttributeValues["anchor-y"]);
							ovrd.Translation = new Vector2(DefaultAttributeValues["translate-x"], DefaultAttributeValues["translate-y"]);
							ovrd.Scale = new Vector2(DefaultAttributeValues["scale-x"], DefaultAttributeValues["scale-y"]);
							ovrd.Rotation = DefaultAttributeValues["rotate"];
							ovrd.Opacity = DefaultAttributeValues["opacity"];
							ovrd.BlurRadius = DefaultAttributeValues["blur-radius"];
							ovrd.StrokeTrace = DefaultAttributeValues["stroke-trace"];
						}
					}
				}
				if (!isEarlyEnding || AnimationEasing(1f) > 0.95f)
				{
					AnimationEasing = animation.CreateEasing();
				}
			};
			AnimatorComponent animatorComponent2 = animComponent;
			animatorComponent2.OnComplete = (Action)Delegate.Combine(animatorComponent2.OnComplete, (Action)delegate
			{
				int num2 = activeAnimators;
				activeAnimators = num2 - 1;
				if (activeAnimators <= 0)
				{
					animators = new List<(int, AnimatorComponent, Action<bool>)>();
					onComplete?.Invoke();
				}
				resetValues(obj: false);
				animComponent.Dispose();
			});
			animComponent.Start();
			animators.Add((activeAnimators, animComponent, resetValues));
			int num = activeAnimators;
			activeAnimators = num + 1;
		}
	}

	private float GetDefault(string id)
	{
		if (!DefaultAttributeValues.TryGetValue(id, out var value))
		{
			return 0f;
		}
		return value;
	}

	private float InterpolateAttribute(List<(string id, object value)>? fromAttrs, List<(string id, object value)>? toAttrs, string id, float t)
	{
		return RMath.Lerp((fromAttrs?.LastOrDefault<(string, object)>(((string id, object value) a) => a.id == id).Item2 as float?) ?? GetDefault(id), (toAttrs?.LastOrDefault<(string, object)>(((string id, object value) a) => a.id == id).Item2 as float?) ?? GetDefault(id), t);
	}

	private (AVKeyframe? from, AVKeyframe? to) FindInterpolationNeighbors(List<AVKeyframe> keyframes, float currentTime)
	{
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		AVKeyframe aVKeyframe = null;
		AVKeyframe aVKeyframe2 = null;
		AVKeyframe aVKeyframe3 = null;
		foreach (AVKeyframe keyframe in keyframes)
		{
			if (keyframe.time <= currentTime && keyframe.time >= num)
			{
				aVKeyframe = aVKeyframe2;
				num = keyframe.time;
				aVKeyframe2 = keyframe;
			}
			else if (keyframe.time > currentTime && keyframe.time < num2)
			{
				num2 = keyframe.time;
				aVKeyframe3 = keyframe;
			}
		}
		if (aVKeyframe3 == null && aVKeyframe2 != null && keyframes.IndexOf(aVKeyframe2) == keyframes.Count - 1)
		{
			aVKeyframe3 = aVKeyframe2;
			aVKeyframe2 = aVKeyframe;
		}
		return (from: aVKeyframe2, to: aVKeyframe3);
	}
}
