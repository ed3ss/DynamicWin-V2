using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public abstract class ContentClipBehaviorProvider
{
	protected StackContentComponent Layout;

	public float ClipStart { get; set; }

	public float ClipLength { get; set; } = 35f;

	public Func<float, float> ClipEase { get; set; } = Easing.EaseOutCubic;

	public bool Inverse { get; set; }

	public ContentClipBehaviorProvider(StackContentComponent layout)
	{
		Layout = layout;
	}

	public virtual void Update(StackContentComponent layout, List<UIObject> children)
	{
	}

	public abstract void ClipBehavior(float t, StackContentComponent layout, UIObject child, int childIndex, bool isBottom);
}
