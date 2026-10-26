using System;
using System.Collections.Generic;
using FenUISharp.Objects.Text.Layout;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Model;

public class TextModelFactory
{
	public static TextModel CreateBasic(string text, float textSize = 14f, bool bold = false, bool italic = false, bool underlined = false, Func<SKColor>? textColor = null, Func<SKColor>? backgroundColor = null, TextAlign? align = null)
	{
		TextStyle style = new TextStyle
		{
			FontSize = textSize,
			Color = (textColor ?? ((Func<SKColor>)(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnBackground))),
			Weight = (bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal),
			Slant = (italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright),
			Underlined = underlined,
			BackgroundColor = (backgroundColor ?? ((Func<SKColor>)(() => SKColors.Transparent)))
		};
		TextAlign align2 = align ?? new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Middle,
			VerticalAlign = TextAlign.AlignType.Middle
		};
		return new TextModel(new List<TextSpan>
		{
			new TextSpan(text, style)
		}, align2, FTypeface.Default);
	}

	public static TextModel CopyBasic(TextModel old, float? textSize = null, bool? bold = null, bool? italic = null, bool? underlined = null, Func<SKColor>? textColor = null, Func<SKColor>? backgroundColor = null, TextAlign? align = null)
	{
		List<TextSpan> list = new List<TextSpan>();
		foreach (TextSpan textPart in old.TextParts)
		{
			textPart.Style.Color = textColor ?? textPart.Style.Color;
			textPart.Style.Weight = (bold.HasValue ? (bold.Value ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal) : textPart.Style.Weight);
			textPart.Style.Slant = (italic.HasValue ? (italic.Value ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright) : textPart.Style.Slant);
			textPart.Style.FontSize = textSize ?? textPart.Style.FontSize;
			textPart.Style.Underlined = underlined ?? textPart.Style.Underlined;
			textPart.Style.BackgroundColor = backgroundColor ?? textPart.Style.BackgroundColor;
			list.Add(textPart);
		}
		return new TextModel(list, align ?? old.Align, FTypeface.Default);
	}

	public static TextModel CopyBasicNew(string newText, TextModel old)
	{
		List<TextSpan> list = new List<TextSpan>();
		TextSpan item = new TextSpan(newText, new TextStyle
		{
			Color = old.TextParts[0].Style.Color,
			Weight = old.TextParts[0].Style.Weight,
			Slant = old.TextParts[0].Style.Slant,
			FontSize = old.TextParts[0].Style.FontSize,
			BackgroundColor = old.TextParts[0].Style.BackgroundColor
		});
		list.Add(item);
		return new TextModel(list, old.Align, FTypeface.Default);
	}

	public static TextModel CreateTest(string text)
	{
		TextAlign align = new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Middle,
			VerticalAlign = TextAlign.AlignType.Middle
		};
		List<TextSpan> list = new List<TextSpan>();
		string[] array = text.Split(' ');
		foreach (string text2 in array)
		{
			TextStyle textStyle = new TextStyle();
			SKColor captured = new SKColor((byte)(Random.Shared.NextSingle() * 255f), (byte)(Random.Shared.NextSingle() * 255f), (byte)(Random.Shared.NextSingle() * 255f), byte.MaxValue);
			textStyle.Color = () => captured;
			if (Random.Shared.NextSingle() > 0.5f)
			{
				textStyle.Weight = SKFontStyleWeight.Bold;
			}
			if (Random.Shared.NextSingle() > 0.5f)
			{
				textStyle.Slant = SKFontStyleSlant.Italic;
			}
			if (Random.Shared.NextSingle() > 0.5f)
			{
				textStyle.Underlined = true;
			}
			list.Add(new TextSpan(text2 + " ", textStyle));
		}
		return new TextModel(list, align, FTypeface.Default);
	}
}
