using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Rendering;

public class TextRenderer
{
	protected FText Parent { get; init; }

	public TextRenderer(FText parent)
	{
		Parent = parent;
	}

	public virtual void DrawText(SKCanvas canvas, TextModel model, List<Glyph> glyphs, SKRect localBounds, SKPaint paint)
	{
		for (int i = 0; i < glyphs.Count; i++)
		{
			if (!RMath.IsRectPartiallyInside(localBounds, glyphs[i].Bounds))
			{
				continue;
			}
			int count = canvas.Save();
			using SKPaint sKPaint = paint.Clone();
			using SKPaint sKPaint2 = paint.Clone();
			Glyph glyph = glyphs[i];
			sKPaint2.Color = glyph.Style.BackgroundColor();
			sKPaint.Color = glyph.Style.Color();
			canvas.Scale(glyph.Scale.Width, glyph.Scale.Height, glyph.Position.X + glyph.Bounds.Width * glyph.Anchor.X / 2f, glyph.Position.Y + (0f - glyph.Bounds.Height) * glyph.Anchor.Y / 2f);
			SKColor color = sKPaint2.Color;
			if (color.Alpha != 0 && (i == 0 || glyphs[i - 1].Style.BackgroundColor() != color))
			{
				SKRect bounds = glyph.Bounds;
				bounds.Offset(0f, bounds.Height / 2f);
				SKRect rect = bounds;
				int num = 0;
				Glyph glyph2 = glyph;
				do
				{
					SKRect bounds2 = glyph2.Bounds;
					bounds2.Offset(0f, bounds2.Height / 2f);
					rect = new SKRect(MathF.Min(rect.Left, glyph2.Bounds.Left), MathF.Min(rect.Top, glyph2.Bounds.Top), MathF.Max(rect.Right, glyph2.Bounds.Right), MathF.Max(rect.Bottom, glyph2.Bounds.Bottom));
					num++;
					if (i + num >= glyphs.Count)
					{
						break;
					}
					glyph2 = glyphs[i + num];
				}
				while (glyph2.Style.BackgroundColor() == color);
				using SKRoundRect rect2 = new SKRoundRect(rect, 5f);
				canvas.DrawRoundRect(rect2, sKPaint2);
			}
			using (SKImageFilter sKImageFilter = SKImageFilter.CreateBlur(glyph.Style.BlurRadius, glyph.Style.BlurRadius))
			{
				using SKFont font = CreateFont(model.Typeface, glyph.Style);
				if (glyph.Style.BlurRadius > 0f)
				{
					sKPaint.ImageFilter = sKImageFilter;
				}
				if (glyph.Style.Opacity < 1f && glyph.Style.Opacity >= 0f)
				{
					float[] obj = new float[20]
					{
						1f, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f,
						0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f
					};
					obj[18] = glyph.Style.Opacity;
					using SKColorFilter cf = SKColorFilter.CreateColorMatrix(obj);
					using SKImageFilter sKImageFilter2 = SKImageFilter.CreateColorFilter(cf);
					if (sKImageFilter != null)
					{
						using SKImageFilter imageFilter = SKImageFilter.CreateCompose(sKImageFilter2, sKImageFilter);
						sKPaint.ImageFilter = imageFilter;
					}
					else
					{
						sKPaint.ImageFilter = sKImageFilter2;
					}
				}
				SKPoint position = glyph.Position;
				canvas.DrawText(glyph.Character.ToString(), position, SKTextAlign.Center, font, sKPaint);
			}
			if (glyph.Style.Underlined)
			{
				DrawUnderline(canvas, glyph, sKPaint);
			}
			canvas.RestoreToCount(count);
		}
	}

	public virtual void DrawUnderline(SKCanvas canvas, Glyph glyph, SKPaint paint)
	{
		if (!char.IsWhiteSpace(glyph.Character))
		{
			int num = 2;
			canvas.DrawLine(new SKPoint(glyph.Bounds.Left, glyph.Bounds.Bottom + (float)num), new SKPoint(glyph.Bounds.Right, glyph.Bounds.Bottom + (float)num), paint);
		}
	}

	public static SKFont CreateFont(FTypeface typeface, TextStyle style)
	{
		return new SKFont(typeface.CreateSKTypeface(style.Weight, style.Slant), style.FontSize)
		{
			Subpixel = true,
			ForceAutoHinting = true,
			Edging = SKFontEdging.SubpixelAntialias
		};
	}
}
