using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public class ScaleContentClipBehavior : ContentClipBehaviorProvider
{
	public Vector2 DefaultScale { get; set; } = new Vector2(1f, 1f);

	public Vector2 ClipScale { get; set; } = new Vector2(0f, 0f);

	public ScaleContentClipBehavior(StackContentComponent layout)
		: base(layout)
	{
		base.Inverse = true;
	}

	public override void ClipBehavior(float t, StackContentComponent layout, UIObject child, int childIndex, bool isBottom)
	{
		child.Transform.Scale.SetStaticState(Vector2.Lerp(ClipScale, DefaultScale, t));
		child.Invalidate(UIObject.Invalidation.TransformDirty);
	}
}
