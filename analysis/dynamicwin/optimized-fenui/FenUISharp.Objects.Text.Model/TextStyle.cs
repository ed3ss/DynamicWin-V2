using System;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Model;

public class TextStyle
{
	public SKFontStyleWeight Weight { get; set; } = SKFontStyleWeight.Normal;

	public SKFontStyleSlant Slant { get; set; }

	public bool Underlined { get; set; }

	public float FontSize { get; set; } = 16f;

	public Func<SKColor> Color { get; set; } = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface;

	public Func<SKColor> BackgroundColor { get; set; } = () => SKColors.Transparent;

	public float BlurRadius { get; set; }

	public float Opacity { get; set; } = 1f;

	public TextStyle()
	{
	}

	public TextStyle(TextStyle copy)
	{
		Weight = copy.Weight;
		Slant = copy.Slant;
		Underlined = copy.Underlined;
		FontSize = copy.FontSize;
		Color = copy.Color;
		BackgroundColor = copy.BackgroundColor;
		BlurRadius = copy.BlurRadius;
	}
}
