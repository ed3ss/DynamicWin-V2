using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public class StackContentClipBehavior : ContentClipBehaviorProvider
{
	public float DistanceFromEdge { get; set; } = 25f;

	public Vector2 Scale { get; set; } = new Vector2(1f, 1f) * 0.85f;

	public bool FlipZIndex { get; set; }

	public float SeparationDistance { get; set; } = 25f;

	public float SlideStart { get; set; } = 65f;

	public StackContentClipBehavior(StackContentComponent layout)
		: base(layout)
	{
		layout.ContentFade = false;
	}

	public override void ClipBehavior(float t, StackContentComponent layout, UIObject child, int childIndex, bool isBottom)
	{
		Vector2 value = layout.ChildLocalPosition[childIndex];
		layout.GetClipFactors(child, base.ClipStart - 200f, base.ClipLength * 2f, out var startFactor, out var endFactor);
		base.ClipEase(Math.Abs(Math.Max(startFactor, endFactor)));
		layout.GetClipFactors(child, base.ClipStart - 100f, base.ClipLength * 3f, out var startFactor2, out var endFactor2);
		float num = Math.Abs(Math.Max(startFactor2, endFactor2));
		layout.GetClipFactors(child, base.ClipStart - SlideStart, base.ClipLength * 3f, out var startFactor3, out var endFactor3);
		float t2 = Math.Abs(Math.Max(startFactor3, endFactor3));
		layout.GetClipFactors(child, base.ClipStart - SlideStart - 50f, base.ClipLength * 3f, out var startFactor4, out var endFactor4);
		Math.Abs(Math.Max(startFactor4, endFactor4));
		float num2 = ((layout.StackType == StackContentComponent.ContentStackType.Horizontal) ? (child.Transform.Position.x - layout.Owner.Transform.Position.x) : (child.Transform.Position.y - layout.Owner.Transform.Position.y));
		bool flag = ((layout.StackType == StackContentComponent.ContentStackType.Horizontal) ? (num2 < layout._pageSize / 2f) : (num2 < layout._pageSize / 2f));
		float num3 = ((layout.StackType != StackContentComponent.ContentStackType.Vertical) ? ((!flag) ? (layout._pageSize - layout.ChildrenOffset.x - DistanceFromEdge) : (0f - layout.ChildrenOffset.x + DistanceFromEdge)) : ((!flag) ? (layout._pageSize - layout.ChildrenOffset.y - DistanceFromEdge) : (0f - layout.ChildrenOffset.y + DistanceFromEdge)));
		if (FlipZIndex)
		{
			flag = !flag;
		}
		child.Composition.LocalZIndex.SetStaticState((!flag) ? (-childIndex + layout.Owner.Children.Count) : 0);
		if (layout.StackType == StackContentComponent.ContentStackType.Horizontal)
		{
			value.x = RMath.Lerp(value.x, RMath.Lerp(num3, num3 + (isBottom ? SeparationDistance : (0f - SeparationDistance)), t2), t2);
		}
		else
		{
			value.y = RMath.Lerp(value.y, RMath.Lerp(num3, num3 + (isBottom ? SeparationDistance : (0f - SeparationDistance)), t2), t2);
		}
		child.Transform.Scale.SetStaticState(new Vector2(RMath.Remap(1f - num, 0f, 1f, Scale.x, 1f), RMath.Remap(1f - num, 0f, 1f, Scale.y, 1f)));
		child.Transform.LocalPosition.SetStaticState(value);
	}
}
