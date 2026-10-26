using System;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FImage : FDisplayableType, IStateListener
{
	public enum ImageScaleMode
	{
		Stretch,
		Fit,
		Contain
	}

	public State<SKImage> Image { get; private init; }

	public State<ImageScaleMode> ScaleMode { get; private init; }

	public FImage(Func<SKImage> image, bool drawBackground = false, bool dynamicColor = false, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(position, size)
	{
		_drawBasePanel = drawBackground;
		Image = new State<SKImage>(image, this, this);
		ScaleMode = new State<ImageScaleMode>(() => ImageScaleMode.Fit, this, this);
		base.CornerRadius.SetResponsiveState(() => base.Layout.ClampSize(base.Transform.Size.CachedValue).y / 1.5f);
		base.Padding.Value = () => 10;
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		if (Image.CachedValue == null)
		{
			return;
		}
		using SKPaint sKPaint = GetRenderPaint();
		SKRect localBounds = base.Shape.LocalBounds;
		using SKPath path = GetPanelPath(localBounds);
		canvas.ClipPath(path, SKClipOperation.Intersect, antialias: true);
		SKRect? sKRect = null;
		switch (ScaleMode.CachedValue)
		{
		case ImageScaleMode.Stretch:
			sKRect = base.Shape.LocalBounds;
			break;
		case ImageScaleMode.Contain:
		{
			float num6 = Math.Min(base.Shape.LocalBounds.Width / (float)Image.CachedValue.Width, base.Shape.LocalBounds.Height / (float)Image.CachedValue.Height);
			float num7 = (float)Image.CachedValue.Width * num6;
			float num8 = (float)Image.CachedValue.Height * num6;
			float x = base.Shape.LocalBounds.Left + (base.Shape.LocalBounds.Width - num7) / 2f;
			float y = base.Shape.LocalBounds.Top + (base.Shape.LocalBounds.Height - num8) / 2f;
			sKRect = SKRect.Create(x, y, num7, num8);
			break;
		}
		case ImageScaleMode.Fit:
		{
			float num = Math.Max(base.Shape.LocalBounds.Width / (float)Image.CachedValue.Width, base.Shape.LocalBounds.Height / (float)Image.CachedValue.Height);
			float num2 = (float)Image.CachedValue.Width * num;
			float num3 = (float)Image.CachedValue.Height * num;
			float num4 = base.Shape.LocalBounds.Left + (base.Shape.LocalBounds.Width - num2) / 2f;
			float num5 = base.Shape.LocalBounds.Top + (base.Shape.LocalBounds.Height - num3) / 2f;
			sKRect = new SKRect(num4, num5, num4 + num2, num5 + num3);
			break;
		}
		}
		using (SKColorFilter colorFilter = SKColorFilter.CreateBlendMode(base.TintColor.CachedValue, base.TintBlendMode.CachedValue))
		{
			sKPaint.ColorFilter = colorFilter;
		}
		canvas.DrawImage(Image.CachedValue, sKRect ?? base.Shape.LocalBounds, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), sKPaint);
	}
}
