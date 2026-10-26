using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using SkiaSharp;

namespace FenUISharp.Materials;

public class InteractableDefaultMaterial : Material
{
	public Func<SKColor> BaseColor
	{
		get
		{
			return GetProp<Func<SKColor>>("BaseColor", () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Secondary);
		}
		set
		{
			SetProp("BaseColor", value);
		}
	}

	public Func<SKColor> ShadowColor
	{
		get
		{
			return GetProp<Func<SKColor>>("ShadowColor", () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Shadow);
		}
		set
		{
			SetProp("ShadowColor", value);
		}
	}

	public Func<SKColor> BorderColor
	{
		get
		{
			return GetProp<Func<SKColor>>("BorderColor", () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.SecondaryBorder);
		}
		set
		{
			SetProp("BorderColor", value);
		}
	}

	public Func<SKColor> HightlightColor
	{
		get
		{
			return GetProp<Func<SKColor>>("HightlightColor", () => BaseColor().AddMix(new SKColor(25, 25, 25)).MultiplyMix(new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 100)));
		}
		set
		{
			SetProp("HightlightColor", value);
		}
	}

	public Func<float> DropShadowRadius
	{
		get
		{
			return GetProp<Func<float>>("DropShadowRadius", () => 2f);
		}
		set
		{
			SetProp("DropShadowRadius", value);
		}
	}

	protected override void Draw(SKCanvas targetCanvas, SKPath path, UIObject caller, SKPaint paint)
	{
		SKRect bounds = path.Bounds;
		targetCanvas.Translate(0.5f, 0.5f);
		paint.IsAntialias = true;
		paint.Style = SKPaintStyle.Fill;
		paint.Color = BaseColor();
		float num = DropShadowRadius();
		using (SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 2f, num, num, ShadowColor()))
		{
			paint.ImageFilter = imageFilter;
		}
		targetCanvas.DrawPath(path, paint);
		paint.IsAntialias = true;
		paint.Shader = SKShader.CreateLinearGradient(new SKPoint(bounds.Left, bounds.Top), new SKPoint(bounds.Left, bounds.Top + 4f), new SKColor[2]
		{
			HightlightColor(),
			SKColors.Transparent
		}, new float[2] { 0f, 0.4f }, SKShaderTileMode.Clamp);
		targetCanvas.DrawPath(path, paint);
		paint.IsStroke = true;
		paint.Color = BorderColor();
		paint.StrokeWidth = 1f;
		targetCanvas.DrawPath(path, paint);
		paint.IsAntialias = true;
		paint.Shader = SKShader.CreateLinearGradient(new SKPoint(bounds.Left, bounds.Bottom), new SKPoint(bounds.Left, bounds.Top + 4f), new SKColor[2]
		{
			FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Shadow,
			SKColors.Transparent
		}, new float[2] { 0f, 1f }, SKShaderTileMode.Clamp);
		targetCanvas.DrawPath(path, paint);
		targetCanvas.Translate(-0.5f, -0.5f);
	}
}
