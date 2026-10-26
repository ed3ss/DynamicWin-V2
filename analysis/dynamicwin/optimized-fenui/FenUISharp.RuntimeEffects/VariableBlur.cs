using System;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.RuntimeEffects;

public static class VariableBlur
{
	public static void ApplyBlur(SKSurface surface, SKRect bounds, int pad, Vector2 direction, float maxBlur = 5f, Func<float, float>? progressiveFunc = null)
	{
		using SKImage image = surface.Snapshot();
		using SKPaint paint = new SKPaint
		{
			BlendMode = SKBlendMode.Clear
		};
		SKRect sKRect = bounds;
		sKRect.Inflate(5f, 5f);
		surface.Canvas.DrawRect(bounds, paint);
		int num = 1;
		Func<float, float> func = progressiveFunc ?? ((Func<float, float>)((float x) => MathF.Pow(x, 2f)));
		for (int num2 = 0; (float)num2 < bounds.Height; num2 += num)
		{
			surface.Canvas.Save();
			float num3 = (float)num2 / bounds.Height;
			float num4 = 0f;
			num4 = ((direction.y != 1f) ? (func(num3) * maxBlur) : (func(1f - num3) * maxBlur));
			using SKImageFilter imageFilter = SKImageFilter.CreateBlur(num4, num4 / 2f);
			using SKPaint paint2 = new SKPaint
			{
				ImageFilter = imageFilter
			};
			using SKPaint sKPaint = new SKPaint();
			sKPaint.BlendMode = SKBlendMode.DstIn;
			SKRect sKRect2 = SKRect.Create(0f, (float)num2 + bounds.Top, bounds.Width, num);
			SKRect rect = sKRect2;
			surface.Canvas.ClipRect(rect);
			surface.Canvas.DrawImage(image, -pad, -pad, paint2);
			surface.Canvas.DrawRect(sKRect2, sKPaint);
			surface.Canvas.Restore();
		}
	}
}
