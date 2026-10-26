using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using SkiaSharp;

namespace FenUISharp.Objects;

internal class FSegmentedSelectionPaneSelectableButton : SelectableButton
{
	private float padding = 5f;

	public FText Label { get; protected set; }

	public FSegmentedSelectionPaneSelectableButton(FText label, Action? onClick = null)
		: base(onClick, null, () => default(Vector2), () => new Vector2(0f, 0f))
	{
		base.OnClick = onClick;
		Label = label;
		Label.SetParent(this);
		FText label2 = Label;
		label2.OnAnyChange = (Action)Delegate.Combine(label2.OnAnyChange, new Action(RefreshLabel));
		RefreshLabel();
		base.InteractiveSurface.ExtendInteractionRadius.SetStaticState(-10);
	}

	private void RefreshLabel()
	{
		SKRect boundingRect = Label.LayoutModel.GetBoundingRect(Label.Model, SKRect.Create(0f, 0f, 1000f, 1000f));
		float num = RMath.Clamp(boundingRect.Width, 15f, 200f);
		float num2 = RMath.Clamp(boundingRect.Height, 20f, 200f);
		Label.Invalidate(Invalidation.SurfaceDirty | Invalidation.LayoutDirty);
		Label.Layout.StretchHorizontal.SetStaticState(value: true);
		Label.Layout.StretchVertical.SetStaticState(value: true);
		Label.Padding.SetStaticState(0);
		base.Transform.Size.SetStaticState(new Vector2(num + padding * 4f, num2 + padding));
	}

	public override void Render(SKCanvas canvas)
	{
	}

	public override void Dispose()
	{
		base.Dispose();
		Label.Dispose();
	}
}
