using System;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FScrollBar : UIObject, IStateListener
{
	private float _lastAlpha;

	private SKRect lastThumbInteractionRect;

	public Action<float>? onPositionChanged;

	private float _scrollDragPosition;

	private float _lastScrollPos;

	private float _mouseStartScrollPos;

	public float ScrollPosition { get; set; }

	public float ScrollMin { get; set; }

	public float ScrollMax { get; set; }

	public float PageSize { get; set; }

	public float ContentSize { get; set; }

	public float MinThumbSize { get; set; } = 15f;

	public State<bool> HorizontalOrientation { get; set; }

	public State<SKColor> ScrollAreaColor { get; init; }

	public State<SKColor> ScrollPositionColor { get; init; }

	public float Alpha { get; set; }

	public float AlphaFadeSpeed { get; set; } = 0.75f;

	public float AlphaFadeTime { get; set; } = 2f;

	public FScrollBar(Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(position, size)
	{
		ScrollAreaColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.SurfaceVariant, this, this);
		ScrollPositionColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, this, this);
		HorizontalOrientation = new State<bool>(() => false, this, this);
		base.InteractiveSurface.EnableMouseActions.Value = () => true;
		base.InteractiveSurface.ExtendInteractionRadius.Value = () => 5;
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnDrag = (Action<Vector2>)Delegate.Combine(interactiveSurface.OnDrag, new Action<Vector2>(OnDrag));
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnDragEnd = (Action)Delegate.Combine(interactiveSurface2.OnDragEnd, new Action(OnDragEnd));
		InteractiveSurface interactiveSurface3 = base.InteractiveSurface;
		interactiveSurface3.OnDragStart = (Action)Delegate.Combine(interactiveSurface3.OnDragStart, new Action(OnDragStart));
		InteractiveSurface interactiveSurface4 = base.InteractiveSurface;
		interactiveSurface4.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface4.OnMouseEnter, new Action(MouseEnter));
		InteractiveSurface interactiveSurface5 = base.InteractiveSurface;
		interactiveSurface5.OnMouseExit = (Action)Delegate.Combine(interactiveSurface5.OnMouseExit, new Action(MouseExit));
		base.Visible.Value = () => ContentSize > PageSize;
	}

	public override void OnInternalStateChanged<T>(T value)
	{
		base.OnInternalStateChanged(value);
		Invalidate(Invalidation.All);
	}

	public override void Dispose()
	{
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseEnter = (Action)Delegate.Remove(interactiveSurface.OnMouseEnter, new Action(MouseEnter));
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnMouseExit = (Action)Delegate.Remove(interactiveSurface2.OnMouseExit, new Action(MouseExit));
		base.Dispose();
	}

	protected override void Update()
	{
		base.Update();
		lastThumbInteractionRect = GetThumbRect(base.Shape.LocalBounds);
		Alpha -= AlphaFadeSpeed * FContext.DeltaTime;
		if (Alpha < 0f)
		{
			Alpha = 0f;
		}
		if (base.InteractiveSurface.IsMouseHovering)
		{
			Alpha = AlphaFadeTime;
		}
		if (RMath.Clamp(Alpha, 0f, 1f) != RMath.Clamp(_lastAlpha, 0f, 1f))
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
		_lastAlpha = Alpha;
	}

	private void OnDrag(Vector2 delta)
	{
		SKRect thumbRect = GetThumbRect(base.Shape.LocalBounds);
		float num = (HorizontalOrientation.CachedValue ? (base.Layout.ClampSize(base.Transform.Size.CachedValue).x - thumbRect.Width) : (base.Layout.ClampSize(base.Transform.Size.CachedValue).y - thumbRect.Height));
		float num2 = (HorizontalOrientation.CachedValue ? delta.x : delta.y);
		float value = ((ContentSize > 0f) ? (PageSize / ContentSize) : 1f);
		value = RMath.Clamp(value, 0.01f, 1f);
		float num3 = num2 / num * (ScrollMax - ScrollMin) * value;
		_scrollDragPosition = RMath.Clamp(_mouseStartScrollPos + num3, ScrollMin, ScrollMax);
		if (_lastScrollPos != _scrollDragPosition)
		{
			onPositionChanged?.Invoke(_scrollDragPosition);
		}
		_lastScrollPos = _scrollDragPosition;
	}

	private void OnDragStart()
	{
		Invalidate(Invalidation.SurfaceDirty);
		SKRect sKRect = lastThumbInteractionRect;
		sKRect.Inflate(base.InteractiveSurface.ExtendInteractionRadius.CachedValue, base.InteractiveSurface.ExtendInteractionRadius.CachedValue);
		_mouseStartScrollPos = ScrollPosition;
	}

	private void OnDragEnd()
	{
		Invalidate(Invalidation.SurfaceDirty);
	}

	public void UpdateScrollbar()
	{
		Alpha = AlphaFadeTime;
		Invalidate(Invalidation.SurfaceDirty);
	}

	protected void MouseEnter()
	{
		Invalidate(Invalidation.SurfaceDirty);
	}

	protected void MouseExit()
	{
		Invalidate(Invalidation.SurfaceDirty);
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint sKPaint = GetRenderPaint();
		SKRect localBounds = base.Shape.LocalBounds;
		sKPaint.Color = ScrollAreaColor.CachedValue.WithAlpha((byte)(255f * RMath.Clamp(Alpha, 0f, (base.InteractiveSurface.IsMouseHovering || base.InteractiveSurface.IsDragging) ? 1f : 0.4f)));
		canvas.DrawRoundRect(localBounds, 5f, 5f, sKPaint);
		canvas.ClipRoundRect(new SKRoundRect(localBounds, 5f, 5f), SKClipOperation.Intersect, antialias: true);
		SKRect thumbRect = GetThumbRect(localBounds);
		sKPaint.Color = ScrollPositionColor.CachedValue.WithAlpha((byte)(255f * RMath.Clamp(Alpha, 0f, (base.InteractiveSurface.IsMouseHovering || base.InteractiveSurface.IsDragging) ? 1f : 0.4f)));
		canvas.DrawRoundRect(thumbRect, 5f, 5f, sKPaint);
	}

	private SKRect GetThumbRect(SKRect scrollArea)
	{
		SKRect result;
		if (HorizontalOrientation.CachedValue)
		{
			float width = scrollArea.Width;
			float val = ((ContentSize > 0f) ? (PageSize / ContentSize * width) : width);
			val = Math.Max(val, MinThumbSize);
			float num = width - val;
			float num2 = ((ScrollMax - ScrollMin > 0f) ? ((ScrollPosition - ScrollMin) / (ScrollMax - ScrollMin)) : 0f);
			float num3 = scrollArea.Left + num * (1f - num2);
			result = new SKRect(num3, scrollArea.Top, num3 + val, scrollArea.Bottom);
		}
		else
		{
			float height = scrollArea.Height;
			float val2 = ((ContentSize > 0f) ? (PageSize / ContentSize * height) : height);
			val2 = Math.Max(val2, MinThumbSize);
			float num4 = height - val2;
			float num5 = ((ScrollMax - ScrollMin > 0f) ? ((ScrollPosition - ScrollMin) / (ScrollMax - ScrollMin)) : 0f);
			float num6 = scrollArea.Top + num4 * (1f - num5);
			result = new SKRect(scrollArea.Left, num6, scrollArea.Right, num6 + val2);
		}
		return result;
	}
}
