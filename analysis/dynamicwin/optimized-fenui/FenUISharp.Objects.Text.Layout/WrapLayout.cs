using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FenUISharp.Objects.Text.Model;
using FenUISharp.Objects.Text.Rendering;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public class WrapLayout : TextLayout
{
	private class LayoutLine
	{
		public List<LayoutCharacter> Characters { get; } = new List<LayoutCharacter>();

		public float Width { get; set; }

		public float LineHeight { get; set; }

		public float Baseline { get; set; }

		public float Ascent { get; set; }

		public float Descent { get; set; }

		public void UpdateMetrics(FontMetrics metrics)
		{
			LineHeight = Math.Max(LineHeight, metrics.LineHeight);
			Baseline = Math.Max(Baseline, metrics.Baseline);
			Ascent = Math.Max(Ascent, metrics.Ascent);
			Descent = Math.Max(Descent, metrics.Descent);
		}
	}

	private class LayoutCharacter
	{
		public char Character { get; set; }

		public float Width { get; set; }

		public float CharacterSpacing { get; set; }

		public SKFont Font { get; set; }

		public TextStyle Style { get; set; }
	}

	private class FontMetrics
	{
		public float LineHeight { get; }

		public float Baseline { get; }

		public float Ascent { get; }

		public float Descent { get; }

		public FontMetrics(SKFont font)
		{
			Ascent = 0f - font.Metrics.Ascent;
			Descent = font.Metrics.Descent;
			LineHeight = Ascent + Descent + font.Metrics.Leading;
			Baseline = Ascent;
		}
	}

	public char EllipsisChar { get; set; } = '…';

	public bool AllowLinebreakChar { get; set; } = true;

	public bool AllowLinebreakOnOverflow { get; set; } = true;

	public bool AllowEllipsis { get; set; } = true;

	public WrapLayout(FText Parent)
		: base(Parent)
	{
	}

	public virtual string[] SplitWords(string content)
	{
		return Regex.Split(content, "(\\s+|\\n)");
	}

	public override List<Glyph> ProcessModel(TextModel model, SKRect bounds)
	{
		List<LayoutLine> layoutLines = CalculateLayout(model, bounds);
		return PositionGlyphs(model, bounds, layoutLines);
	}

	private List<LayoutLine> CalculateLayout(TextModel model, SKRect bounds)
	{
		List<LayoutLine> list = new List<LayoutLine>();
		list.Add(new LayoutLine());
		foreach (TextSpan textPart in model.TextParts)
		{
			SKFont font = TextRenderer.CreateFont(model.Typeface, textPart.Style);
			FontMetrics fontMetrics = new FontMetrics(font);
			string[] array = SplitWords(textPart.Content);
			foreach (string text in array)
			{
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				if (text.Contains("\r\n") || text.Contains("\n"))
				{
					if (AllowLinebreakChar && AllowLinebreakOnOverflow)
					{
						LayoutLine layoutLine = new LayoutLine();
						layoutLine.UpdateMetrics(fontMetrics);
						list.Add(layoutLine);
					}
					continue;
				}
				LayoutLine layoutLine2 = list[list.Count - 1];
				layoutLine2.UpdateMetrics(fontMetrics);
				float num = CalculateTextWidth(text, font, textPart.CharacterSpacing);
				if (layoutLine2.Width + num > bounds.Width)
				{
					if (!AllowLinebreakOnOverflow && AllowEllipsis)
					{
						TruncateLineWithEllipsis(layoutLine2, font, textPart, bounds.Width);
						return list;
					}
					if (AllowLinebreakOnOverflow)
					{
						if (layoutLine2.Characters.Count > 0)
						{
							LayoutLine layoutLine3 = new LayoutLine();
							layoutLine3.UpdateMetrics(fontMetrics);
							list.Add(layoutLine3);
							layoutLine2 = list[list.Count - 1];
						}
						bool flag = false;
						if (list.Count > 1)
						{
							flag = CalculateTotalHeight(list) > bounds.Height;
						}
						if (flag && AllowEllipsis)
						{
							list.RemoveAt(list.Count - 1);
							LayoutLine line = list[list.Count - 1];
							TruncateLineWithEllipsis(line, font, textPart, bounds.Width);
							return list;
						}
						if (!TryAddWord(layoutLine2, text, font, textPart, bounds.Width))
						{
							AddWordCharacterByCharacter(list, text, font, textPart, bounds, fontMetrics);
						}
					}
					else
					{
						AddWordToLine(layoutLine2, text, font, textPart);
					}
				}
				else
				{
					AddWordToLine(layoutLine2, text, font, textPart);
				}
				if (list.Count > 1 && CalculateTotalHeight(list) > bounds.Height && AllowEllipsis)
				{
					LayoutLine line2 = list[list.Count - 1];
					TruncateLineWithEllipsis(line2, font, textPart, bounds.Width);
					break;
				}
			}
		}
		return list;
	}

	private bool TryAddWord(LayoutLine line, string word, SKFont font, TextSpan part, float maxWidth)
	{
		float num = CalculateTextWidth(word, font, part.CharacterSpacing);
		if (line.Width + num <= maxWidth)
		{
			AddWordToLine(line, word, font, part);
			return true;
		}
		return false;
	}

	private void AddWordToLine(LayoutLine line, string word, SKFont font, TextSpan part)
	{
		for (int i = 0; i < word.Length; i++)
		{
			char c = word[i];
			if (c != '\n' && c != '\r')
			{
				float num = font.MeasureText(c.ToString());
				LayoutCharacter item = new LayoutCharacter
				{
					Character = c,
					Width = num,
					CharacterSpacing = part.CharacterSpacing,
					Font = font,
					Style = part.Style
				};
				line.Characters.Add(item);
				line.Width += num + part.CharacterSpacing;
			}
		}
	}

	private void AddWordCharacterByCharacter(List<LayoutLine> lines, string word, SKFont font, TextSpan part, SKRect bounds, FontMetrics fontMetrics)
	{
		LayoutLine layoutLine = lines[lines.Count - 1];
		for (int i = 0; i < word.Length; i++)
		{
			char c = word[i];
			if (c == '\n' || c == '\r')
			{
				continue;
			}
			float num = font.MeasureText(c.ToString());
			if (layoutLine.Width + num + part.CharacterSpacing > bounds.Width && AllowLinebreakOnOverflow)
			{
				float num2 = CalculateTotalHeight(lines) + fontMetrics.LineHeight;
				if (lines.Count > 0 && num2 > bounds.Height && AllowEllipsis)
				{
					TruncateLineWithEllipsis(layoutLine, font, part, bounds.Width);
					break;
				}
				LayoutLine layoutLine2 = new LayoutLine();
				layoutLine2.UpdateMetrics(fontMetrics);
				lines.Add(layoutLine2);
				layoutLine = lines[lines.Count - 1];
			}
			LayoutCharacter item = new LayoutCharacter
			{
				Character = c,
				Width = num,
				CharacterSpacing = part.CharacterSpacing,
				Font = font,
				Style = part.Style
			};
			layoutLine.Characters.Add(item);
			layoutLine.Width += num + part.CharacterSpacing;
		}
	}

	private void TruncateLineWithEllipsis(LayoutLine line, SKFont font, TextSpan part, float maxWidth)
	{
		float num = font.MeasureText(EllipsisChar.ToString());
		while (line.Characters.Count > 0 && line.Width + num > maxWidth)
		{
			List<LayoutCharacter> characters = line.Characters;
			LayoutCharacter layoutCharacter = characters[characters.Count - 1];
			line.Width -= layoutCharacter.Width + layoutCharacter.CharacterSpacing;
			line.Characters.RemoveAt(line.Characters.Count - 1);
		}
		LayoutCharacter item = new LayoutCharacter
		{
			Character = EllipsisChar,
			Width = num,
			CharacterSpacing = part.CharacterSpacing,
			Font = font,
			Style = part.Style
		};
		line.Characters.Add(item);
		line.Width += num + part.CharacterSpacing;
	}

	private float CalculateTextWidth(string text, SKFont font, float characterSpacing)
	{
		float num = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c != '\n' && c != '\r')
			{
				num += font.MeasureText(c.ToString()) + characterSpacing;
			}
		}
		return num;
	}

	private float CalculateTotalHeight(List<LayoutLine> lines)
	{
		return lines.Sum((LayoutLine l) => l.LineHeight);
	}

	private List<Glyph> PositionGlyphs(TextModel model, SKRect bounds, List<LayoutLine> layoutLines)
	{
		List<Glyph> list = new List<Glyph>();
		float contentHeight = CalculateTotalHeight(layoutLines);
		float num = CalculateVerticalOffset(model.Align.VerticalAlign, bounds.Height, contentHeight);
		foreach (LayoutLine layoutLine in layoutLines)
		{
			float num2 = CalculateHorizontalOffset(model.Align.HorizontalAlign, bounds.Width, layoutLine.Width);
			float y = num + layoutLine.Baseline;
			foreach (LayoutCharacter character in layoutLine.Characters)
			{
				float num3 = character.Width / 2f;
				float num4 = character.CharacterSpacing / 2f;
				Glyph item = new Glyph(character.Character, new SKPoint(num2 + num3 + num4, y), new SKSize(1f, 1f), new SKPoint(0.5f, 0.5f), new TextStyle(character.Style), new SKSize(character.Width, layoutLine.LineHeight));
				list.Add(item);
				num2 += character.Width + character.CharacterSpacing;
			}
			num += layoutLine.LineHeight;
		}
		return list;
	}

	private float CalculateVerticalOffset(TextAlign.AlignType verticalAlign, float boundsHeight, float contentHeight)
	{
		return verticalAlign switch
		{
			TextAlign.AlignType.Middle => (boundsHeight - contentHeight) / 2f, 
			TextAlign.AlignType.End => boundsHeight - contentHeight, 
			_ => 0f, 
		};
	}

	private float CalculateHorizontalOffset(TextAlign.AlignType horizontalAlign, float boundsWidth, float lineWidth)
	{
		return horizontalAlign switch
		{
			TextAlign.AlignType.Middle => (boundsWidth - lineWidth) / 2f, 
			TextAlign.AlignType.End => boundsWidth - lineWidth, 
			_ => 0f, 
		};
	}
}
