using System;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FRadialProgressBar : FProgressBar
{
	public float Thickness { get; set; } = 5f;

	public float IndeterminateArc { get; set; } = 100f;

	public FRadialProgressBar(Func<float> value, Func<Vector2>? position = null, float radius = 100f)
		: base(value, position, radius, radius)
	{
	}

	public FRadialProgressBar(Func<Vector2>? position = null, float radius = 100f)
		: base(position, radius, radius)
	{
	}

	public override void Render(SKCanvas canvas)
	{
		SKRect localBounds = base.Shape.LocalBounds;
		_ = base.Shape.LocalBounds.Height / 2f;
		float num = RMath.Remap(_01value, 0f, 1f, 0.01f, 1f) * 360f;
		if (!base.LeftToRight)
		{
			num = 1f - num;
		}
		using (SKPaint sKPaint = GetRenderPaint())
		{
			using SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 2f, 5f, 5f, FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Shadow);
			sKPaint.IsStroke = true;
			sKPaint.StrokeCap = SKStrokeCap.Round;
			sKPaint.ImageFilter = imageFilter;
			sKPaint.StrokeWidth = Thickness + 1f;
			sKPaint.Color = base.BorderColor.CachedValue;
			canvas.DrawArc(localBounds, -90f, 360f, useCenter: false, sKPaint);
			sKPaint.ImageFilter = null;
			sKPaint.StrokeWidth = Thickness;
			sKPaint.Color = base.BackgroundColor.CachedValue;
			canvas.DrawArc(localBounds, -90f, 360f, useCenter: false, sKPaint);
			sKPaint.Color = base.FillColor.CachedValue;
			if (base.Indeterminate)
			{
				float startAngle = time * 360f;
				float indeterminateArc = IndeterminateArc;
				canvas.DrawArc(localBounds, startAngle, indeterminateArc, useCenter: false, sKPaint);
			}
			else
			{
				canvas.DrawArc(localBounds, -90f, num, useCenter: false, sKPaint);
			}
		}
		using SKPaint sKPaint2 = GetRenderPaint();
		sKPaint2.IsStroke = true;
		sKPaint2.StrokeWidth = Thickness;
		sKPaint2.StrokeCap = SKStrokeCap.Round;
		sKPaint2.Style = SKPaintStyle.Stroke;
		SKColor[] colors = new SKColor[3]
		{
			SKColors.Black.WithAlpha(65),
			SKColors.White.WithAlpha(85),
			SKColors.Black.WithAlpha(65)
		};
		float[] colorPos = new float[3] { 0f, 0.5f, 1f };
		SKPoint center = new SKPoint(localBounds.MidX, localBounds.MidY);
		sKPaint2.Shader = SKShader.CreateSweepGradient(center, colors, colorPos, SKShaderTileMode.Clamp, 0f, 360f);
		sKPaint2.BlendMode = SKBlendMode.Screen;
		if (base.Indeterminate)
		{
			float startAngle2 = time * 360f + 90f;
			float indeterminateArc2 = IndeterminateArc;
			canvas.RotateDegrees(-90f, localBounds.MidX, localBounds.MidY);
			canvas.DrawArc(localBounds, startAngle2, indeterminateArc2, useCenter: false, sKPaint2);
		}
		else
		{
			canvas.RotateDegrees(-90f, localBounds.MidX, localBounds.MidY);
			canvas.DrawArc(localBounds, 0f, num, useCenter: false, sKPaint2);
		}
	}
}
