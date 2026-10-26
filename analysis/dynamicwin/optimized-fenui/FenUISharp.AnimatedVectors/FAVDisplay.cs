using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.AnimatedVectors;

public class FAVDisplay : FDisplayableType
{
	private AnimatedVector currentAnimVector;

	private FAVAnimator? currentAnimation;

	private bool ignoreAnimSwap;

	public State<AnimatedVector> AnimatedVector { get; private set; }

	private Dictionary<int, AVPathAnimationOverride> PathOverrides { get; set; } = new Dictionary<int, AVPathAnimationOverride>();

	public void GetOrCreatePathOverride(int pathID, out AVPathAnimationOverride ovrd)
	{
		if (PathOverrides.Any<KeyValuePair<int, AVPathAnimationOverride>>((KeyValuePair<int, AVPathAnimationOverride> x) => x.Key == pathID))
		{
			ovrd = PathOverrides.Last<KeyValuePair<int, AVPathAnimationOverride>>((KeyValuePair<int, AVPathAnimationOverride> x) => x.Key == pathID).Value;
		}
		else
		{
			ovrd = new AVPathAnimationOverride();
			PathOverrides.Add(pathID, ovrd);
		}
	}

	public FAVDisplay(Func<AnimatedVector> animatedVector, bool dynamicColor = false, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(position, size, dynamicColor)
	{
		AnimatedVector = new State<AnimatedVector>(animatedVector, this, this);
		currentAnimVector = AnimatedVector.CachedValue;
		base.Padding.SetResponsiveState(() => AnimatedVector.CachedValue.ExtendBounds);
		base.RenderMaterial.SetStaticState(new EmptyDefaultMaterial
		{
			BaseColor = () => SKColors.White
		});
		AnimatedVector.Subscribe(delegate(AnimatedVector x)
		{
			if (ignoreAnimSwap)
			{
				ignoreAnimSwap = false;
			}
			else
			{
				Action action = delegate
				{
					base.ObjectSurface.LockInvalidation = true;
					FContext.GetCurrentDispatcher().InvokeLater(delegate
					{
						base.ObjectSurface.LockInvalidation = false;
					}, 1uL);
					currentAnimVector = x;
					PlayAnimation("in");
				};
				if (currentAnimVector != null)
				{
					PlayAnimation("out", action);
				}
				else
				{
					action();
				}
			}
		});
	}

	public override void Render(SKCanvas canvas)
	{
		foreach (AVPath path in currentAnimVector.Paths)
		{
			using SKPath sKPath = new SKPath(path.SKPath);
			AVPathAnimationOverride aVPathAnimationOverride;
			if (PathOverrides.Any<KeyValuePair<int, AVPathAnimationOverride>>((KeyValuePair<int, AVPathAnimationOverride> x) => x.Key == currentAnimVector.Paths.IndexOf(path)))
			{
				aVPathAnimationOverride = PathOverrides.Last<KeyValuePair<int, AVPathAnimationOverride>>((KeyValuePair<int, AVPathAnimationOverride> x) => x.Key == currentAnimVector.Paths.IndexOf(path)).Value;
			}
			else
			{
				aVPathAnimationOverride = new AVPathAnimationOverride();
				PathOverrides.Add(currentAnimVector.Paths.IndexOf(path), aVPathAnimationOverride);
			}
			using SKPaint sKPaint = new SKPaint
			{
				Color = SKColors.White.WithAlpha((byte)(aVPathAnimationOverride.Opacity * 255f))
			};
			if (aVPathAnimationOverride.BlurRadius > 0f)
			{
				using SKImageFilter imageFilter = SKImageFilter.CreateBlur(aVPathAnimationOverride.BlurRadius, aVPathAnimationOverride.BlurRadius);
				sKPaint.ImageFilter = imageFilter;
			}
			int count = canvas.SaveLayer(sKPaint);
			SKRect viewBox = currentAnimVector.ViewBox;
			float val = base.Shape.LocalBounds.Width / viewBox.Width;
			float val2 = base.Shape.LocalBounds.Height / viewBox.Height;
			float num = Math.Min(val, val2);
			sKPath.GetBounds(out var rect);
			SKMatrix first = SKMatrix.CreateScale(num * 2f, num * 2f);
			Vector2 vector = new Vector2(aVPathAnimationOverride.Anchor.x * rect.Width + rect.Left, aVPathAnimationOverride.Anchor.y * rect.Height + rect.Top);
			if (aVPathAnimationOverride.UseObjectAnchor)
			{
				vector = new Vector2(aVPathAnimationOverride.Anchor.x * base.Shape.LocalBounds.Width, aVPathAnimationOverride.Anchor.y * base.Shape.LocalBounds.Height);
				vector /= num * 2f;
			}
			first = SKMatrix.Concat(first, SKMatrix.CreateRotationDegrees(aVPathAnimationOverride.Rotation, vector.x, vector.y));
			first = SKMatrix.Concat(first, SKMatrix.CreateScale(RMath.Clamp(aVPathAnimationOverride.Scale.x, 0f, 99999f), RMath.Clamp(aVPathAnimationOverride.Scale.y, 0f, 99999f), vector.x, vector.y));
			if (aVPathAnimationOverride.UseObjectSizeTranslation)
			{
				first = SKMatrix.Concat(first, SKMatrix.CreateTranslation(aVPathAnimationOverride.Translation.x * base.Shape.LocalBounds.Width, aVPathAnimationOverride.Translation.y * base.Shape.LocalBounds.Height));
			}
			first = SKMatrix.Concat(first, SKMatrix.CreateTranslation(0f - viewBox.Left, 0f - viewBox.Top));
			if (!aVPathAnimationOverride.UseObjectSizeTranslation)
			{
				first = SKMatrix.Concat(first, SKMatrix.CreateTranslation(aVPathAnimationOverride.Translation.x * rect.Width, aVPathAnimationOverride.Translation.y * rect.Height));
			}
			sKPath.Transform(in first);
			using SKPaint sKPaint2 = GetRenderPaint();
			sKPaint2.Color = path.Fill;
			if (aVPathAnimationOverride.StrokeTrace != 1f)
			{
				float num2 = sKPath.ApproximateLength();
				float num3 = num2 * aVPathAnimationOverride.StrokeTrace;
				sKPaint2.PathEffect = SKPathEffect.CreateDash(new float[2] { num3, num2 }, 0f);
			}
			base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, sKPath, this, sKPaint2);
			sKPaint2.Color = base.TintColor.CachedValue;
			sKPaint2.BlendMode = base.TintBlendMode.CachedValue;
			base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, sKPath, this, sKPaint2);
			if (path.StrokeWidth > 0f)
			{
				sKPaint2.BlendMode = SKBlendMode.SrcOver;
				sKPaint2.IsStroke = true;
				sKPaint2.Color = path.Stroke;
				sKPaint2.StrokeWidth = path.StrokeWidth;
				sKPaint2.StrokeCap = currentAnimVector.LineCap;
				sKPaint2.StrokeJoin = currentAnimVector.LineJoin;
				base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, sKPath, this, sKPaint2);
				sKPaint2.Color = base.TintColor.CachedValue;
				sKPaint2.BlendMode = base.TintBlendMode.CachedValue;
				base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, sKPath, this, sKPaint2);
			}
			canvas.RestoreToCount(count);
		}
	}

	public void SilentSetFAV(AnimatedVector fav)
	{
		ignoreAnimSwap = true;
		currentAnimVector = fav;
		AnimatedVector.SetStaticState(fav);
		Invalidate(Invalidation.SurfaceDirty);
	}

	public void PlayAnimation(string id, Action? onComplete = null)
	{
		if (!currentAnimVector.Animations.Any<(string, AVAnimation)>(((string id, AVAnimation animation) x) => x.id == id))
		{
			onComplete?.Invoke();
			return;
		}
		if (currentAnimation != null)
		{
			currentAnimation.StopAnimation();
			currentAnimation = null;
		}
		onComplete = (Action)Delegate.Combine(onComplete, (Action)delegate
		{
			currentAnimation = null;
		});
		currentAnimation = new FAVAnimator();
		currentAnimation.PlayAnimation(this, currentAnimVector, id, onComplete);
	}
}
