using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Behavior.Layout;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Behavior;

public class StackContentComponent : LayoutComponent, IStateListener
{
	public enum ContentStackType
	{
		Horizontal,
		Vertical
	}

	public enum ContentStackBehavior
	{
		Overflow,
		SizeToFit,
		SizeToFitAll,
		Scroll
	}

	private float _scrollPosition;

	private float _snappedScrollPosition;

	private float _scrollDisplayPosition;

	private float _lastScrollDisplayPosition;

	private float _scrollMin;

	private float _scrollMax;

	private FScrollBar scrollBar;

	private bool _positionDirty;

	private int? _fadeLayerSaveCount;

	public Vector2 StartAlignment { get; set; }

	public Vector2 AlignInside { get; set; }

	public State<float> Gap { get; set; }

	public State<float> Pad { get; set; }

	public bool AllowScrollOverflow { get; set; } = true;

	public bool ContentFade { get; set; }

	public bool ContentClip { get; set; } = true;

	public float FadeLength { get; set; } = 60f;

	public ContentStackType StackType { get; set; }

	public ContentStackBehavior StackBehavior { get; set; }

	public ContentClipBehaviorProvider? ContentClipBehaviorProvider { get; set; }

	public Func<float, float>? SnappingProvider { get; set; }

	public bool ApplySnapSeparately { get; set; } = true;

	public float ScrollSpeed { get; set; } = 0.45f;

	public float _pageSize { get; protected set; }

	public float _contentSize { get; protected set; }

	public Spring? ScrollSpring { get; set; }

	public List<Vector2> ChildLocalPosition { get; private set; } = new List<Vector2>();

	public Vector2 ChildrenOffset { get; private set; } = new Vector2(0f, 0f);

	public StackContentComponent(UIObject owner, ContentStackType type, ContentStackBehavior behavior, Vector2? startAlign = null, Vector2? alignInside = null)
		: base(owner)
	{
		StackType = type;
		StackBehavior = behavior;
		owner.InteractiveSurface.EnableMouseActions.Value = () => true;
		InteractiveSurface interactiveSurface = owner.InteractiveSurface;
		interactiveSurface.OnDragDelta = (Action<Vector2>)Delegate.Combine(interactiveSurface.OnDragDelta, new Action<Vector2>(OnDrag));
		owner.InteractiveSurface.EnableMouseScrolling.Value = () => StackBehavior == ContentStackBehavior.Scroll;
		InteractiveSurface interactiveSurface2 = owner.InteractiveSurface;
		interactiveSurface2.OnMouseScroll = (Action<float>)Delegate.Combine(interactiveSurface2.OnMouseScroll, new Action<float>(OnScroll));
		Gap = new State<float>(() => 10f, base.Owner, this);
		Pad = new State<float>(() => 15f, base.Owner, this);
		ScrollSpring = new Spring(new Vector2(0f, 0f), 2f, 1.1764705f);
		if (!startAlign.HasValue)
		{
			switch (type)
			{
			case ContentStackType.Horizontal:
				StartAlignment = new Vector2(0f, 0.5f);
				break;
			case ContentStackType.Vertical:
				StartAlignment = new Vector2(0.5f, 0f);
				break;
			}
		}
		else
		{
			StartAlignment = startAlign.Value;
		}
		if (!alignInside.HasValue)
		{
			AlignInside = new Vector2(0.5f, 0.5f);
		}
		else
		{
			AlignInside = alignInside.Value;
		}
		UpdateChildDeps();
	}

	public override void ComponentDestroy()
	{
		base.ComponentDestroy();
		GetAffectedChildren().ForEach(delegate(UIObject x)
		{
			x.Layout.ProcessLayoutPositioning = null;
		});
		if (base.Owner != null)
		{
			InteractiveSurface interactiveSurface = base.Owner.InteractiveSurface;
			interactiveSurface.OnDrag = (Action<Vector2>)Delegate.Remove(interactiveSurface.OnDrag, new Action<Vector2>(OnDrag));
			InteractiveSurface interactiveSurface2 = base.Owner.InteractiveSurface;
			interactiveSurface2.OnMouseScroll = (Action<float>)Delegate.Remove(interactiveSurface2.OnMouseScroll, new Action<float>(OnScroll));
		}
	}

	public Vector2 ProcessChildLayout(Vector2 i)
	{
		return i + ChildrenOffset;
	}

	public override void HandleEvent(BehaviorEventType type, object? data = null)
	{
		base.HandleEvent(type, data);
		switch (type)
		{
		case BehaviorEventType.BeforeDrawChildren:
			OnBeforeRenderChildren((SKCanvas)data);
			break;
		case BehaviorEventType.AfterDrawChildren:
			OnAfterRenderChildren((SKCanvas)data);
			break;
		case BehaviorEventType.BeforeBegin:
			ComponentSetup();
			break;
		case BehaviorEventType.BeforeUpdate:
			UpdateChildDeps();
			Update();
			break;
		case BehaviorEventType.AfterUpdate:
			if (_positionDirty)
			{
				UpdatePosition();
			}
			_positionDirty = false;
			break;
		}
	}

	private List<UIObject> GetAffectedChildren()
	{
		List<UIObject> affected = new List<UIObject>();
		base.Owner?.Composition.GetZOrderedListOfChildren(base.Owner).ForEach(delegate(UIObject x)
		{
			if (!x.BehaviorComponents.Any((BehaviorComponent behaviorComponent) => behaviorComponent is LayoutObject && ((LayoutObject)behaviorComponent).IgnoreParentLayout.CachedValue))
			{
				affected.Add(x);
			}
		});
		return affected;
	}

	private void UpdateChildDeps()
	{
		GetAffectedChildren().ForEach(delegate(UIObject x)
		{
			x.Layout.ProcessLayoutPositioning = ProcessChildLayout;
		});
	}

	private void InvalidateChildren()
	{
		GetAffectedChildren().ForEach(delegate(UIObject x)
		{
			x.Invalidate(UIObject.Invalidation.TransformDirty);
		});
	}

	public void OnBeforeRenderChildren(SKCanvas? canvas)
	{
		if (ContentClip)
		{
			canvas?.ClipRect(base.Owner.Shape.LocalBounds);
		}
		if (StackBehavior == ContentStackBehavior.Scroll && ContentFade && !(_contentSize <= _pageSize))
		{
			SKRect localBounds = base.Owner.Shape.LocalBounds;
			_fadeLayerSaveCount = canvas?.SaveLayer(localBounds, null);
		}
	}

	public void OnAfterRenderChildren(SKCanvas? canvas)
	{
		if (StackBehavior != ContentStackBehavior.Scroll || !ContentFade || _contentSize <= _pageSize || !_fadeLayerSaveCount.HasValue)
		{
			return;
		}
		SKRect localBounds = base.Owner.Shape.LocalBounds;
		using (SKPaint sKPaint = new SKPaint())
		{
			sKPaint.BlendMode = SKBlendMode.DstIn;
			SKPoint start = new SKPoint(0f, localBounds.Top);
			SKPoint end = new SKPoint(0f, localBounds.Bottom);
			if (StackType == ContentStackType.Horizontal)
			{
				start = new SKPoint(localBounds.Left, 0f);
				end = new SKPoint(localBounds.Right, 0f);
			}
			float t = (localBounds.Height - (localBounds.Height - FadeLength)) / localBounds.Height;
			sKPaint.Shader = SKShader.CreateLinearGradient(start, end, new SKColor[4]
			{
				SKColors.Transparent,
				SKColors.Black,
				SKColors.Black,
				SKColors.Transparent
			}, new float[4]
			{
				0f,
				bias(t),
				1f - bias(t),
				1f
			}, SKShaderTileMode.Clamp);
			canvas?.DrawRect(localBounds, sKPaint);
		}
		if (_fadeLayerSaveCount.HasValue)
		{
			canvas?.RestoreToCount(_fadeLayerSaveCount.Value);
		}
		_fadeLayerSaveCount = null;
		static float bias(float num)
		{
			return (float)Math.Pow(num, 1.5);
		}
	}

	public void ComponentSetup()
	{
		scrollBar = new FScrollBar(() => new Vector2(0f, 0f), () => new Vector2(4f, 4f));
		scrollBar.Layout.MarginHorizontal.SetStaticState(8f);
		scrollBar.Layout.MarginVertical.SetStaticState(8f);
		new LayoutObject(scrollBar).IgnoreParentLayout.SetStaticState(value: true);
		scrollBar.Visible.SetStaticState(value: false);
		scrollBar.HorizontalOrientation.SetResponsiveState(() => StackType == ContentStackType.Horizontal);
		scrollBar.Visible.Value = () => StackBehavior == ContentStackBehavior.Scroll && _pageSize < _contentSize;
		FScrollBar fScrollBar = scrollBar;
		fScrollBar.onPositionChanged = (Action<float>)Delegate.Combine(fScrollBar.onPositionChanged, new Action<float>(OnScrollbarUpdate));
		scrollBar.SetParent(base.Owner);
		ChangeScrollbar();
	}

	private void OnScroll(float delta)
	{
		if (StackBehavior == ContentStackBehavior.Scroll)
		{
			_scrollPosition += delta * ScrollSpeed;
		}
	}

	private void OnDrag(Vector2 vector)
	{
		if (StackBehavior == ContentStackBehavior.Scroll)
		{
			_scrollPosition += ((StackType == ContentStackType.Horizontal) ? vector.x : vector.y);
		}
	}

	private void OnScrollbarUpdate(float value)
	{
		if (StackBehavior == ContentStackBehavior.Scroll)
		{
			_scrollPosition = value;
		}
	}

	public void ChangeScrollbar()
	{
		if (StackBehavior == ContentStackBehavior.Scroll)
		{
			if (StackType == ContentStackType.Vertical)
			{
				scrollBar.Transform.LocalPosition.SetStaticState(new Vector2(-5f, 0f));
				scrollBar.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
				scrollBar.Layout.StretchHorizontal.SetStaticState(value: false);
				scrollBar.Layout.StretchVertical.SetStaticState(value: true);
			}
			else if (StackType == ContentStackType.Horizontal)
			{
				scrollBar.Transform.LocalPosition.SetStaticState(new Vector2(0f, -5f));
				scrollBar.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
				scrollBar.Layout.StretchHorizontal.SetStaticState(value: true);
				scrollBar.Layout.StretchVertical.SetStaticState(value: false);
			}
		}
	}

	public void Update()
	{
		if (!AllowScrollOverflow || ScrollSpring == null)
		{
			_scrollPosition = RMath.Clamp(_scrollPosition, 0f - _scrollMax, _scrollMin);
		}
		if (SnappingProvider != null && ApplySnapSeparately)
		{
			_snappedScrollPosition = SnappingProvider(_scrollPosition);
		}
		else
		{
			_snappedScrollPosition = _scrollPosition;
		}
		if (ScrollSpring != null && _contentSize > _pageSize)
		{
			_scrollDisplayPosition = ScrollSpring.Update(FContext.DeltaTime, new Vector2(_snappedScrollPosition, 0f)).x;
		}
		else if (ScrollSpring == null)
		{
			_scrollDisplayPosition = _snappedScrollPosition;
		}
		else
		{
			_scrollDisplayPosition = _pageSize / 2f - _contentSize / 2f;
		}
		if (AllowScrollOverflow && ScrollSpring != null)
		{
			_scrollPosition = RMath.Clamp(_scrollPosition, 0f - _scrollMax, _scrollMin);
		}
		if (SnappingProvider != null && !ApplySnapSeparately)
		{
			_scrollPosition = SnappingProvider(_scrollPosition);
		}
		if (StackBehavior == ContentStackBehavior.Scroll)
		{
			if (StackType == ContentStackType.Horizontal)
			{
				ChildrenOffset = new Vector2(_scrollDisplayPosition, 0f);
			}
			else if (StackType == ContentStackType.Vertical)
			{
				ChildrenOffset = new Vector2(0f, _scrollDisplayPosition);
			}
			if (_contentSize < _pageSize)
			{
				ChildrenOffset += new Vector2(RMath.Lerp((0f - _pageSize) / 2f + _contentSize / 2f, _pageSize / 2f - _contentSize / 2f, AlignInside.x), RMath.Lerp((0f - _pageSize) / 2f + _contentSize / 2f, _pageSize / 2f - _contentSize / 2f, AlignInside.y));
			}
		}
		if (Math.Round(_lastScrollDisplayPosition) != Math.Round(_scrollDisplayPosition))
		{
			scrollBar?.UpdateScrollbar();
			InvalidateChildren();
		}
		ProcessLayout();
		UpdateScrollbar();
	}

	public void GetClipFactors(UIObject child, in float clipStart, in float clipLength, out float startFactor, out float endFactor)
	{
		float num = ((StackType == ContentStackType.Horizontal) ? child.Shape.SurfaceDrawRect.MidX : child.Shape.SurfaceDrawRect.MidY);
		float num2 = ((StackType == ContentStackType.Horizontal) ? base.Owner.Transform.Position.x : base.Owner.Transform.Position.y) + (float)base.Owner.Padding.CachedValue;
		float num3 = Math.Clamp(clipLength, 0.01f, float.MaxValue);
		startFactor = Math.Clamp((num2 - num + clipLength + clipStart) / num3, 0f, 1f);
		endFactor = Math.Clamp(1f - (num2 - num + _pageSize - clipStart) / num3, 0f, 1f);
	}

	public void UpdatePosition()
	{
		List<UIObject> affectedChildren = GetAffectedChildren();
		ChildLocalPosition = new List<Vector2>();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < affectedChildren.Count; i++)
		{
			BehaviorComponent behaviorComponent = affectedChildren[i].BehaviorComponents.FirstOrDefault((BehaviorComponent x) => x is LayoutObject);
			if (behaviorComponent == null || !((LayoutObject)behaviorComponent).IgnoreParentLayout.CachedValue)
			{
				float num4 = ((StackType == ContentStackType.Horizontal) ? affectedChildren[i].Shape.LocalBounds.Width : affectedChildren[i].Shape.LocalBounds.Height);
				num += num4 / 2f;
				num2 += num4 / 2f;
				if (i == 0)
				{
					num += Pad.CachedValue;
					num2 += Pad.CachedValue;
				}
				else
				{
					num += Gap.CachedValue;
					num2 += Gap.CachedValue;
				}
				num3 = Math.Max((StackType == ContentStackType.Horizontal) ? affectedChildren[i].Shape.LocalBounds.Height : affectedChildren[i].Shape.LocalBounds.Width, num3);
				affectedChildren[i].Layout.Alignment.SetStaticState(StartAlignment);
				if (StackType == ContentStackType.Horizontal)
				{
					affectedChildren[i].Transform.LocalPosition.SetStaticState(new Vector2(num, 0f));
				}
				else
				{
					affectedChildren[i].Transform.LocalPosition.SetStaticState(new Vector2(0f, num));
				}
				if (affectedChildren.Count > i && ChildLocalPosition.Capacity > i)
				{
					ChildLocalPosition.Insert(i, affectedChildren[i].Transform.LocalPosition.CachedValue);
				}
				num += num4 / 2f;
				num2 += num4 / 2f;
			}
		}
		num2 = Math.Abs(num2 + Pad.CachedValue);
		num3 += Pad.CachedValue * 2f;
		Vector2 calculatedOwnerSize = new Vector2(base.Owner.Shape.LocalBounds.Width, base.Owner.Shape.LocalBounds.Height);
		switch (StackBehavior)
		{
		case ContentStackBehavior.SizeToFitAll:
			if (StackType == ContentStackType.Horizontal)
			{
				calculatedOwnerSize = new Vector2(num2, num3);
			}
			if (StackType == ContentStackType.Vertical)
			{
				calculatedOwnerSize = new Vector2(num3, num2);
			}
			break;
		case ContentStackBehavior.SizeToFit:
			if (StackType == ContentStackType.Horizontal)
			{
				calculatedOwnerSize = new Vector2(num2, base.Owner.Layout.ClampSize(base.Owner.Transform.Size.CachedValue).y);
			}
			if (StackType == ContentStackType.Vertical)
			{
				calculatedOwnerSize = new Vector2(base.Owner.Layout.ClampSize(base.Owner.Transform.Size.CachedValue).x, num2);
			}
			break;
		}
		base.Owner.Transform.Size.Value = () => calculatedOwnerSize;
		_pageSize = ((StackType == ContentStackType.Horizontal) ? calculatedOwnerSize.x : calculatedOwnerSize.y);
		_scrollMax = num2 - _pageSize;
		_contentSize = num2;
		ProcessLayout();
	}

	private void ProcessLayout()
	{
		if (ContentClipBehaviorProvider != null && StackBehavior == ContentStackBehavior.Scroll)
		{
			List<UIObject> affectedChildren = GetAffectedChildren();
			ContentClipBehaviorProvider.Update(this, affectedChildren);
			for (int i = 0; i < affectedChildren.Count && i < affectedChildren.Count && i < ChildLocalPosition.Count; i++)
			{
				BehaviorComponent behaviorComponent = affectedChildren[i].BehaviorComponents.FirstOrDefault((BehaviorComponent x) => x is LayoutObject);
				if ((behaviorComponent == null || !((LayoutObject)behaviorComponent).IgnoreParentLayout.CachedValue) && affectedChildren[i] != null)
				{
					_ = ChildLocalPosition[i];
					affectedChildren[i].Transform.LocalPosition.SetStaticState(ChildLocalPosition[i]);
					GetClipFactors(affectedChildren[i], ContentClipBehaviorProvider.ClipStart, ContentClipBehaviorProvider.ClipLength, out var startFactor, out var endFactor);
					if (ContentClipBehaviorProvider.Inverse)
					{
						startFactor = 1f - startFactor;
						endFactor = 1f - endFactor;
					}
					float arg = Math.Abs(Math.Max(startFactor, endFactor));
					if (ContentClipBehaviorProvider.Inverse)
					{
						arg = Math.Abs(Math.Min(startFactor, endFactor));
					}
					ContentClipBehaviorProvider.ClipBehavior(ContentClipBehaviorProvider.ClipEase(arg), this, affectedChildren[i], i, startFactor <= endFactor);
				}
			}
		}
		_lastScrollDisplayPosition = _scrollDisplayPosition;
	}

	private void UpdateScrollbar()
	{
		if (scrollBar != null)
		{
			scrollBar.ScrollMin = 0f - _scrollMax;
			scrollBar.ScrollMax = _scrollMin;
			scrollBar.PageSize = _pageSize;
			scrollBar.ContentSize = _contentSize;
			scrollBar.ScrollPosition = _scrollDisplayPosition;
		}
	}

	public override void FullUpdateLayout()
	{
		_positionDirty = true;
	}

	public void OnInternalStateChanged<T>(T value)
	{
		base.Owner.Invalidate(UIObject.Invalidation.LayoutDirty);
	}
}
