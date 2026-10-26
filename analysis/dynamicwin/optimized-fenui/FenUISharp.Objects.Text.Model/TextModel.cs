using System.Collections.Generic;
using FenUISharp.Objects.Text.Layout;

namespace FenUISharp.Objects.Text.Model;

public class TextModel
{
	public string Text
	{
		get
		{
			string text = "";
			TextParts.ForEach(delegate(TextSpan x)
			{
				text += x.Content;
			});
			return text;
		}
	}

	public List<TextSpan> TextParts { get; init; }

	public FTypeface Typeface { get; init; }

	public TextAlign Align { get; init; }

	public TextModel(List<TextSpan> textParts, TextAlign align, FTypeface? typeface = null)
	{
		Typeface = typeface ?? FTypeface.Default;
		Align = align;
		TextParts = textParts;
	}

	public TextModel(TextModel copy)
	{
		TextParts = copy.TextParts;
		Typeface = copy.Typeface;
		Align = copy.Align;
	}
}
