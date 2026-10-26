using System;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.RuntimeEffects;
using SkiaSharp;

namespace FenUISharp.Objects;

public class CachedSurface(Action<SKCanvas?> DrawAction) : IDisposable
{
	private FAdditionalSurface? _cachedSurface;

	private SKImageInfo? _cachedImageInfo;

	private SKImage? _cachedSnapshot;

	private int padding;

	private float quality = 1f;

	public bool LockInvalidation { get; set; }

	public bool TryGetSurface(out SKSurface surface)
	{
		if (_cachedSurface == null)
		{
			surface = null;
			return false;
		}
		surface = _cachedSurface.SkiaSurface;
		return true;
	}

	private SKSurface? CreateSurface()
	{
		if (!_cachedImageInfo.HasValue)
		{
			FLogger.Error("InvalidateSurface has to be called before surface is created");
			return null;
		}
		if (!FContext.IsValidContext())
		{
			throw new Exception("Invalid FenUISharp window context.");
		}
		_cachedSurface = FContext.GetCurrentWindow()?.SkiaDirectCompositionContext?.CreateAdditional(_cachedImageInfo.Value);
		_cachedSurface?.SkiaSurface.Canvas.Scale(quality, quality);
		_cachedSurface?.SkiaSurface.Canvas.Translate(padding, padding);
		return _cachedSurface?.SkiaSurface;
	}

	public SKSurface? Draw(PostProcessChain? effectChain = null)
	{
		if (_cachedSurface != null)
		{
			return _cachedSurface.SkiaSurface;
		}
		SKSurface sKSurface = CreateSurface();
		PPInfo info = new PPInfo
		{
			source = sKSurface,
			target = sKSurface,
			sourceInfo = _cachedImageInfo.GetValueOrDefault()
		};
		if (effectChain != null && sKSurface != null)
		{
			effectChain.OnBeforeRender(info);
		}
		DrawAction?.Invoke(sKSurface?.Canvas);
		if (effectChain != null && sKSurface != null)
		{
			effectChain.OnAfterRender(info);
		}
		if (effectChain != null && sKSurface != null)
		{
			effectChain.OnLateAfterRender(info);
		}
		_cachedSnapshot?.Dispose();
		_cachedSnapshot = null;
		_cachedSnapshot = sKSurface?.Snapshot();
		return sKSurface;
	}

	public void DrawFullChainToTarget(SKCanvas target, SKRect targetRect, SKPaint paint, PostProcessChain? effectChain = null)
	{
		Draw(effectChain);
		if (_cachedSnapshot != null)
		{
			target.DrawImage(_cachedSnapshot, targetRect, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
		}
	}

	public static (SKPoint scale, SKPoint offset) GetTransformationsForTargetRect(SKRect sourceRect, SKRect targetRect, bool preserveAspect = true, bool center = true)
	{
		float num = targetRect.Width / sourceRect.Width;
		float num2 = targetRect.Height / sourceRect.Height;
		if (preserveAspect)
		{
			num = (num2 = Math.Min(num, num2));
		}
		float num3 = targetRect.Left;
		float num4 = targetRect.Top;
		if (center)
		{
			num3 += (targetRect.Width - sourceRect.Width * num) / 2f;
			num4 += (targetRect.Height - sourceRect.Height * num2) / 2f;
		}
		return (scale: new SKPoint(num, num2), offset: new SKPoint(num3, num4));
	}

	public SKImage? CaptureSurfaceRegion(SKRect region, float quality = 0.5f)
	{
		if (_cachedSurface == null)
		{
			return null;
		}
		Compositor.Dump(_cachedSurface.SkiaSurface.Snapshot(), "buffer_surf_whole");
		SKImage sKImage = _cachedSurface.SkiaSurface.Snapshot(new SKRectI((int)region.Left, (int)region.Top, (int)region.Right, (int)region.Bottom));
		SKImage? sKImage2 = RMath.CreateLowResImage(sKImage, RMath.Clamp(quality, 0.01f, 1f), SkiaDirectCompositionContext.SamplingOptions);
		sKImage?.Dispose();
		Compositor.Dump(sKImage2, "buffer_cropped_scaled");
		return sKImage2;
	}

	public void InvalidateSurface(SKRect dimensions, float quality, int padding)
	{
		if (!LockInvalidation)
		{
			if (_cachedSurface != null)
			{
				_cachedSurface?.SkiaSurface?.Canvas.Dispose();
				_cachedSurface?.Dispose();
				_cachedSurface = null;
			}
			quality = RMath.Clamp(quality, 0f, 1f);
			int width = (int)((dimensions.Width + (float)(padding * 2)) * quality);
			int height = (int)((dimensions.Height + (float)(padding * 2)) * quality);
			this.padding = padding;
			this.quality = quality;
			_cachedImageInfo = new SKImageInfo(width, height);
		}
	}

	public SKImage? GetImage()
	{
		return _cachedSnapshot;
	}

	public void Dispose()
	{
		LockInvalidation = false;
		DisposeSurface();
	}

	internal void DisposeSurface()
	{
		if (!LockInvalidation)
		{
			_cachedSnapshot?.Dispose();
			_cachedSurface?.Dispose();
			_cachedSnapshot = null;
			_cachedSurface = null;
		}
	}
}
