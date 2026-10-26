using System;
using FenUISharp.Mathematics;
using FenUISharp.States;

namespace FenUISharp.Objects;

public class Layout : IDisposable, IStateListener
{
	private WeakReference<UIObject>? WeakOwner;

	public UIObject? Owner
	{
		get
		{
			WeakReference<UIObject>? weakOwner = WeakOwner;
			if (weakOwner != null && weakOwner.TryGetTarget(out UIObject target))
			{
				return target;
			}
			return null;
		}
	}

	public State<Vector2> Alignment { get; init; }

	public State<Vector2> AlignmentAnchor { get; init; }

	public State<float> MaxWidth { get; init; }

	public State<float> MaxHeight { get; init; }

	public State<float> MinWidth { get; init; }

	public State<float> MinHeight { get; init; }

	public State<float> MarginHorizontal { get; init; }

	public State<float> MarginVertical { get; init; }

	public State<Vector2> AbsoluteMarginHorizontal { get; init; }

	public State<Vector2> AbsoluteMarginVertical { get; init; }

	public State<bool> StretchHorizontal { get; init; }

	public State<bool> StretchVertical { get; init; }

	public Func<Vector2, Vector2>? ProcessLayoutPositioning { get; set; }

	public Layout(UIObject owner)
	{
		WeakOwner = new WeakReference<UIObject>(owner);
		MaxWidth = new State<float>(() => float.MaxValue, owner, this);
		MaxHeight = new State<float>(() => float.MaxValue, owner, this);
		MinWidth = new State<float>(() => 0f, owner, this);
		MinHeight = new State<float>(() => 0f, owner, this);
		Alignment = new State<Vector2>(() => new Vector2(0.5f, 0.5f), owner, this);
		AlignmentAnchor = new State<Vector2>(() => new Vector2(0.5f, 0.5f), owner, this);
		MarginHorizontal = new State<float>(() => 0f, owner, this);
		MarginVertical = new State<float>(() => 0f, owner, this);
		AbsoluteMarginHorizontal = new State<Vector2>(() => new Vector2(0f, 0f), owner, this);
		AbsoluteMarginVertical = new State<Vector2>(() => new Vector2(0f, 0f), owner, this);
		StretchHorizontal = new State<bool>(() => false, owner, this);
		StretchVertical = new State<bool>(() => false, owner, this);
	}

	public void ApplyLayoutToPositioning(in Vector2 size, out Vector2 offset, out Vector2 anchorCorrection)
	{
		if (Owner != null)
		{
			Vector2 cachedValue = AlignmentAnchor.CachedValue;
			Vector2 cachedValue2 = Alignment.CachedValue;
			Vector2 vector = ClampSize(in size);
			Vector2 vector2 = new Vector2(AbsoluteMarginHorizontal.CachedValue.x * (1f - cachedValue2.x) - AbsoluteMarginHorizontal.CachedValue.y * cachedValue2.x, AbsoluteMarginVertical.CachedValue.x * (1f - cachedValue2.y) - AbsoluteMarginVertical.CachedValue.y * cachedValue2.y);
			Vector2 vector3 = new Vector2(vector.x * cachedValue.x, vector.y * cachedValue.y);
			Vector2 vector4 = new Vector2((Owner.Parent?.Shape.LocalBounds.Width ?? FContext.GetCurrentWindow()?.Shape.Bounds.Width ?? 0f) * cachedValue2.x + vector2.x, (Owner.Parent?.Shape.LocalBounds.Height ?? FContext.GetCurrentWindow()?.Shape.Bounds.Height ?? 0f) * cachedValue2.y + vector2.y);
			offset = ProcessLayoutPositioning?.Invoke(vector4) ?? vector4;
			anchorCorrection = vector3;
		}
		else
		{
			offset = Vector2.Zero;
			anchorCorrection = Vector2.Zero;
		}
	}

	public Vector2 ApplyLayoutToSize(Vector2 localSize)
	{
		if (Owner != null)
		{
			Vector2 vector = ClampSize(in localSize);
			Vector2 vector2 = new Vector2((Owner.Parent?.Shape.LocalBounds.Width ?? FContext.GetCurrentWindow()?.Shape.Bounds.Width ?? 0f) - MarginHorizontal.CachedValue * 2f, (Owner.Parent?.Shape.LocalBounds.Height ?? FContext.GetCurrentWindow()?.Shape.Bounds.Height ?? 0f) - MarginVertical.CachedValue * 2f);
			Vector2 vector3 = new Vector2(AbsoluteMarginHorizontal.CachedValue.x + AbsoluteMarginHorizontal.CachedValue.y, AbsoluteMarginVertical.CachedValue.x + AbsoluteMarginVertical.CachedValue.y);
			return ClampSize(new Vector2(StretchHorizontal.CachedValue ? vector2.x : vector.x, StretchVertical.CachedValue ? vector2.y : vector.y) - vector3);
		}
		return Vector2.Zero;
	}

	public void RecursivelyUpdateLayout()
	{
		Owner?.RecursiveInvalidate(UIObject.Invalidation.LayoutDirty);
	}

	public Vector2 ClampSize(in Vector2 size)
	{
		return Vector2.Clamp(size, new Vector2(MinWidth.CachedValue, MinHeight.CachedValue), new Vector2(MaxWidth.CachedValue, MaxHeight.CachedValue));
	}

	public void OnInternalStateChanged<T>(T value)
	{
		Owner?.Invalidate(UIObject.Invalidation.LayoutDirty);
	}

	public void Dispose()
	{
		WeakOwner = null;
	}
}
