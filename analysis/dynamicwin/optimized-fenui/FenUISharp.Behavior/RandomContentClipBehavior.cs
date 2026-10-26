using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public class RandomContentClipBehavior : ContentClipBehaviorProvider
{
	private List<Vector2> offsets;

	public float Spread { get; set; } = 500f;

	public float PerpendicularSpread { get; set; } = 100f;

	public RandomContentClipBehavior(StackContentComponent layout)
		: base(layout)
	{
		layout.ContentFade = false;
		offsets = new List<Vector2>();
	}

	public override void Update(StackContentComponent layout, List<UIObject> children)
	{
		base.Update(layout, children);
		if (offsets == null)
		{
			offsets = new List<Vector2>();
			for (int i = 0; i < children.Count; i++)
			{
				offsets.Add(new Vector2(Random.Shared.NextSingle() * Spread - Spread / 2f, 0f - (25f + Random.Shared.NextSingle() * PerpendicularSpread)));
			}
		}
	}

	public override void ClipBehavior(float t, StackContentComponent layout, UIObject child, int childIndex, bool isBottom)
	{
		Vector2 vector = layout.ChildLocalPosition[childIndex];
		vector = ((layout.StackType != StackContentComponent.ContentStackType.Vertical) ? Vector2.Lerp(vector, isBottom ? (new Vector2(layout._pageSize, 0f) - layout.ChildrenOffset - offsets[childIndex].Swapped) : (layout.ChildrenOffset * -1f + offsets[childIndex].Swapped), t) : Vector2.Lerp(vector, isBottom ? (new Vector2(0f, layout._pageSize) - layout.ChildrenOffset - offsets[childIndex]) : (layout.ChildrenOffset * -1f + offsets[childIndex]), t));
		child.Transform.LocalPosition.SetStaticState(vector);
	}
}
