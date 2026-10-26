using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text;
using SkiaSharp;

namespace FenUISharp.Objects.Buttons;

public class FContextMenuButton : Button
{
	private float padding = 7.5f;

	public FText Label { get; protected set; }

	public FContextMenuButton(FText label, Action? onClick = null)
		: base(onClick, () => default(Vector2), () => new Vector2(0f, 20f))
	{
		base.OnClick = onClick;
		base.Layout.StretchHorizontal.SetStaticState(value: true);
		Label = label;
		Label.SetParent(this);
		FText label2 = Label;
		label2.OnAnyChange = (Action)Delegate.Combine(label2.OnAnyChange, new Action(RefreshLabel));
		RefreshLabel();
	}

	private void RefreshLabel()
	{
		float num = RMath.Clamp(Label.LayoutModel.GetBoundingRect(Label.Model, SKRect.Create(0f, 0f, 10000f, 1000f)).Height, 20f, 100f);
		Label.Invalidate(Invalidation.SurfaceDirty | Invalidation.LayoutDirty);
		Label.Layout.StretchHorizontal.SetStaticState(value: true);
		Label.Layout.StretchVertical.SetStaticState(value: true);
		Label.Padding.SetStaticState(0);
		base.Transform.Size.SetStaticState(new Vector2(0f, num + padding * 0.5f));
		Invalidate(Invalidation.LayoutDirty);
	}

	public override void Dispose()
	{
		base.Dispose();
		Label.Dispose();
	}
}
