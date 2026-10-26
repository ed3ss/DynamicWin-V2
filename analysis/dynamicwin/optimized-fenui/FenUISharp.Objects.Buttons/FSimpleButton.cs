using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects.Buttons;

public class FSimpleButton : Button, IStateListener
{
	private float padding = 7.5f;

	private float minWidth;

	private float maxWidth;

	public FText Label { get; protected set; }

	public FSimpleButton(FText label, Action? onClick = null, Func<Vector2>? position = null, float minWidth = 25f, float maxWidth = 175f)
		: base(onClick, position, () => new Vector2(0f, 0f))
	{
		base.OnClick = onClick;
		this.maxWidth = RMath.Clamp(maxWidth, minWidth, float.MaxValue);
		this.minWidth = RMath.Clamp(minWidth, 0f, this.maxWidth);
		Label = label;
		Label.SetParent(this);
		FText label2 = Label;
		label2.OnAnyChange = (Action)Delegate.Combine(label2.OnAnyChange, new Action(RefreshLabel));
		RefreshLabel();
	}

	private void RefreshLabel()
	{
		SKRect boundingRect = Label.LayoutModel.GetBoundingRect(Label.Model, SKRect.Create(0f, 0f, maxWidth, 1000f));
		float num = RMath.Clamp(boundingRect.Width, minWidth, maxWidth);
		float num2 = RMath.Clamp(boundingRect.Height, 20f, 100f);
		Label.Invalidate(Invalidation.SurfaceDirty | Invalidation.LayoutDirty);
		Label.Layout.StretchHorizontal.SetStaticState(value: true);
		Label.Layout.StretchVertical.SetStaticState(value: true);
		Label.Padding.SetStaticState(0);
		base.Transform.Size.SetStaticState(new Vector2(num + padding * 2.5f, num2 + padding * 0.5f));
	}

	public override void Dispose()
	{
		base.Dispose();
		Label.Dispose();
	}
}
