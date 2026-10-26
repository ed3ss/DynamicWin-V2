using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public class WiggleCharsLayoutProcessor : LayoutProcessor
{
	private bool needsFullRebuild = true;

	private List<Glyph>? cachedLayout;

	private float time;

	public Vector2 Strength { get; set; } = new Vector2(1f, 1f);

	public Vector2 PeriodMultiplier { get; set; } = new Vector2(1.15f, 2.85f);

	public float Speed { get; set; } = 0.15f;

	public WiggleCharsLayoutProcessor(FText parent, TextLayout innerLayout)
		: base(parent, innerLayout)
	{
		parent.OnAnyChange = (Action)Delegate.Combine(parent.OnAnyChange, (Action)delegate
		{
			needsFullRebuild = true;
		});
	}

	public override List<Glyph> ProcessModel(TextModel model, SKRect bounds)
	{
		time += Speed;
		if (needsFullRebuild || cachedLayout == null)
		{
			needsFullRebuild = false;
			cachedLayout = base.ProcessModel(model, bounds);
		}
		List<Glyph> list = new List<Glyph>(cachedLayout);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Position = new SKPoint(list[i].Position.X + (float)Math.Sin(time + (float)i * PeriodMultiplier.x) * Strength.x, list[i].Position.Y + (float)Math.Sin(time + (float)i * PeriodMultiplier.y) * Strength.y);
			needsFullRebuild = true;
		}
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		return list;
	}
}
