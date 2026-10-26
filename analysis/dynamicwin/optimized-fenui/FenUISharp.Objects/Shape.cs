using System;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.Objects;

public class Shape
{
	public WeakReference<UIObject> Owner { get; init; }

	public SKRect LocalBounds { get; private set; }

	public SKRect GlobalBounds { get; private set; }

	public SKRect SurfaceDrawRect { get; private set; }

	public SKRect LastGlobalBounds { get; private set; }

	public Shape(UIObject owner)
	{
		Owner = new WeakReference<UIObject>(owner);
	}

	public void UpdateShape()
	{
		if (Owner.TryGetTarget(out UIObject target))
		{
			SKRect surfaceDrawRect = SurfaceDrawRect;
			Vector2 vector = target.Layout.ApplyLayoutToSize(target.Transform.Size.CachedValue);
			LocalBounds = new SKRect(0f, 0f, vector.x, vector.y);
			SurfaceDrawRect = new SKRect(-target.Padding.CachedValue, -target.Padding.CachedValue, vector.x + (float)target.Padding.CachedValue, vector.y + (float)target.Padding.CachedValue);
			LastGlobalBounds = GlobalBounds;
			GlobalBounds = target.Transform.DrawLocalToGlobal(SurfaceDrawRect);
			if (surfaceDrawRect.Left != SurfaceDrawRect.Left || surfaceDrawRect.Right != SurfaceDrawRect.Right || surfaceDrawRect.Height != SurfaceDrawRect.Height || surfaceDrawRect.Width != SurfaceDrawRect.Width)
			{
				target.LayoutChangedThisFrame = true;
			}
		}
	}
}
