using System;
using System.Collections.Generic;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public abstract class TextLayout
{
	protected FText Owner { get; init; }

	public TextLayout(FText Parent)
	{
		Owner = Parent;
	}

	public abstract List<Glyph> ProcessModel(TextModel model, SKRect bounds);

	public virtual SKRect GetBoundingRect(TextModel model, SKRect cage, float padding = 1f)
	{
		cage.Inflate(2f, 2f);
		List<Glyph> list = ProcessModel(model, cage);
		float left = float.MaxValue;
		float top = float.MaxValue;
		float right = float.MinValue;
		float bottom = float.MinValue;
		list.ForEach(delegate(Glyph x)
		{
			left = Math.Min(left, x.Position.X);
			top = Math.Min(top, x.Position.Y);
			right = Math.Max(right, x.Position.X + x.Size.Width);
			bottom = Math.Max(bottom, x.Position.Y + x.Size.Height);
		});
		return SKRect.Create(left - padding, top - padding, right - left + padding * 2f, bottom - top + padding * 2f);
	}
}
