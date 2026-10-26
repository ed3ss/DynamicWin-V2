using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using SkiaSharp;

namespace FenUISharp.Materials;

public class DefaultPanelMaterial : Material
{
	public Func<SKColor> BaseColor
	{
		get
		{
			return GetProp<Func<SKColor>>("BaseColor", () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface);
		}
		set
		{
			SetProp("BaseColor", value);
		}
	}

	public Func<SKColor> BorderColor
	{
		get
		{
			return GetProp<Func<SKColor>>("BorderColor", () => BaseColor().AddMix(new SKColor(50, 50, 50, 0)));
		}
		set
		{
			SetProp("BorderColor", value);
		}
	}

	public Func<SKColor> DarkBorderColor
	{
		get
		{
			return GetProp<Func<SKColor>>("DarkBorderColor", () => BorderColor().Multiply(0.1f));
		}
		set
		{
			SetProp("DarkBorderColor", value);
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

	public Func<float> DropShadowRadius
	{
		get
		{
			return GetProp<Func<float>>("DropShadowRadius", () => 6f);
		}
		set
		{
			SetProp("DropShadowRadius", value);
		}
	}

	public Func<float> BorderSize
	{
		get
		{
			return GetProp<Func<float>>("BorderSize", () => 2f);
		}
		set
		{
			SetProp("BorderSize", value);
		}
	}

	protected override void Draw(SKCanvas targetCanvas, SKPath path, UIObject caller, SKPaint paint)
	{
		if (BorderSize() % 2f == 1f)
		{
			targetCanvas.Translate(0.5f, 0.5f);
		}
		paint.Color = BaseColor();
		using (SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 2f, DropShadowRadius(), DropShadowRadius(), ShadowColor()))
		{
			paint.ImageFilter = imageFilter;
		}
		using (SKPaint sKPaint = paint.Clone())
		{
			sKPaint.IsStroke = true;
			sKPaint.StrokeCap = SKStrokeCap.Round;
			sKPaint.StrokeJoin = SKStrokeJoin.Round;
			sKPaint.ImageFilter = null;
			sKPaint.Color = DarkBorderColor();
			sKPaint.StrokeWidth = BorderSize() + 2f * (BorderSize() / 2f);
			targetCanvas.DrawPath(path, sKPaint);
			sKPaint.Color = BorderColor();
			sKPaint.StrokeWidth = BorderSize();
			targetCanvas.DrawPath(path, sKPaint);
		}
		targetCanvas.DrawPath(path, paint);
		if (BorderSize() % 2f == 1f)
		{
			targetCanvas.Translate(-0.5f, -0.5f);
		}
	}
}
