using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Behavior;
using FenUISharp.Behavior.Layout;
using FenUISharp.Behavior.RuntimeEffects;
using FenUISharp.Logging;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.RuntimeEffects;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public abstract class UIObject : IDisposable, IStateListener
{
	[Flags]
	public enum Invalidation
	{
		Clean = 0,
		ChildDirty = 1,
		TransformDirty = 2,
		SurfaceDirty = 4,
		LayoutDirty = 8,
		All = 0xF
	}

	private static uint _lastInstanceID;

	private bool _wasBeginCalled;

	private bool _insideParent = true;

	private bool lateLayoutUpdate;

	private bool lateTransformUpdate;

	public uint InstanceID { get; init; }

	public Transform Transform { get; private set; }

	public Layout Layout { get; private set; }

	public Shape Shape { get; private set; }

	public Compositor Composition { get; private set; }

	public InteractiveSurface InteractiveSurface { get; private set; }

	public ImageEffects ImageEffects { get; private set; }

	public PostProcessChain PostProcessChain { get; private set; }

	public State<Material> RenderMaterial { get; private set; }

	internal List<BehaviorComponent> BehaviorComponents { get; private set; }

	public UIObject? Parent { get; private set; }

	public List<UIObject> Children { get; private set; }

	public State<bool> Enabled { get; init; }

	public State<bool> Visible { get; init; }

	public State<bool> LocalVisible { get; init; }

	public bool IsParentRoot => Parent == FContext.GetRootViewPane();

	public bool DisableWhenOutOfParentBounds { get; set; } = true;

	public bool GlobalEnabled
	{
		get
		{
			if (Enabled.CachedValue)
			{
				UIObject? parent = Parent;
				if (parent == null || parent.Enabled.CachedValue)
				{
					if (!DisableWhenOutOfParentBounds)
					{
						return true;
					}
					return _insideParent;
				}
			}
			return false;
		}
	}

	public bool GlobalVisible
	{
		get
		{
			if (LocalVisible.CachedValue && Visible.CachedValue)
			{
				UIObject? parent = Parent;
				if (parent == null || parent.Visible.CachedValue)
				{
					if (!DisableWhenOutOfParentBounds)
					{
						return true;
					}
					return _insideParent;
				}
			}
			return false;
		}
	}

	public bool IsDisposed { get; private set; }

	public Invalidation InvalidationState { get; private set; }

	public bool WindowRedrawThisObject { get; internal set; }

	public bool LayoutChangedThisFrame { get; internal set; }

	public bool LockInvalidation { get; set; }

	public bool LockInvalidationAll { get; set; }

	public bool GlobalLockInvalidation
	{
		get
		{
			if (!LockInvalidationAll)
			{
				return Parent?.GlobalLockInvalidation ?? false;
			}
			return true;
		}
	}

	public State<float> Quality { get; init; }

	public State<int> Padding { get; init; }

	public CachedSurface ObjectSurface { get; protected set; }

	public Action? OnObjectDisposed { get; set; }

	public UIObject(Func<Vector2>? position = null, Func<Vector2>? size = null)
	{
		if (!FContext.IsValidContext())
		{
			throw new Exception("Invalid FenUISharp window context.");
		}
		FContext.GetCurrentWindow().WindowThemeManager.ThemeChanged += OnThemeChanged;
		InstanceID = _lastInstanceID++;
		RenderMaterial = new State<Material>(FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.DefaultMaterial, this, this);
		Enabled = new State<bool>(() => true, this, this);
		Visible = new State<bool>(() => true, this, this);
		LocalVisible = new State<bool>(() => true, this, this);
		Transform = new Transform(this);
		Layout = new Layout(this);
		Shape = new Shape(this);
		Composition = new Compositor(this);
		InteractiveSurface = new InteractiveSurface(this, FContext.GetCurrentDispatcher(), () => Transform.DrawLocalToGlobal(Shape.LocalBounds));
		PostProcessChain = new PostProcessChain(this);
		Quality = new State<float>(() => 1f, this, delegate
		{
			Invalidate(Invalidation.SurfaceDirty);
		});
		Quality.SetResolver(StateResolverTemplates.SmallestFloatResolver);
		Padding = new State<int>(() => 2, this, delegate
		{
			Invalidate(Invalidation.SurfaceDirty);
		});
		Padding.SetResolver(StateResolverTemplates.BiggestIntResolver);
		Transform.LocalPosition.Value = position ?? ((Func<Vector2>)(() => Vector2.Zero));
		Transform.Size.Value = size ?? ((Func<Vector2>)(() => new Vector2(100f, 100f)));
		ObjectSurface = new CachedSurface(RenderToSurface);
		SetParent(FContext.GetRootViewPane());
		BehaviorComponents = new List<BehaviorComponent>();
		Children = new List<UIObject>();
		ImageEffects = new ImageEffects(this);
		Invalidate(Invalidation.All);
	}

	public void CheckIfInsideParent()
	{
		if (!IsDisposed)
		{
			if (!_wasBeginCalled || !DisableWhenOutOfParentBounds)
			{
				_insideParent = true;
			}
			bool insideParent = RMath.IsRectPartiallyInside(Parent?.Shape.GlobalBounds ?? FContext.GetCurrentWindow().Shape.Bounds, Shape.GlobalBounds);
			_insideParent = insideParent;
		}
	}

	protected virtual void OnThemeChanged()
	{
		if (!IsDisposed)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public void RecursiveInvalidate(Invalidation invalidation)
	{
		if (!IsDisposed)
		{
			Invalidate(invalidation);
			Children.ToList().ForEach(delegate(UIObject x)
			{
				x.RecursiveInvalidate(invalidation);
			});
		}
	}

	public void Invalidate(Invalidation invalidation)
	{
		if (IsDisposed || LockInvalidation || LockInvalidationAll || GlobalLockInvalidation)
		{
			return;
		}
		if (invalidation == Invalidation.LayoutDirty)
		{
			if (Parent != null)
			{
				Parent.InvalidationState |= Invalidation.LayoutDirty;
			}
			Composition.GetZOrderedListOfChildren(this).ForEach(delegate(UIObject x)
			{
				x.InvalidationState |= Invalidation.LayoutDirty;
			});
			new List<LayoutComponent>();
			List<LayoutComponent> list = SearchForLayoutComponentsRecursive(Parent ?? this);
			list.Reverse();
			list.ForEach(delegate(LayoutComponent x)
			{
				x.FullUpdateLayout();
			});
		}
		InvalidationState |= invalidation;
		Parent?.Invalidate(Invalidation.ChildDirty);
		if (InvalidationState != Invalidation.Clean && InvalidationState != Invalidation.ChildDirty)
		{
			WindowRedrawThisObject = true;
		}
		OnInvalidate(invalidation);
	}

	private List<LayoutComponent> SearchForLayoutComponentsRecursive(UIObject obj)
	{
		List<LayoutComponent> returnList = new List<LayoutComponent>();
		obj.BehaviorComponents.ForEach(delegate(BehaviorComponent x)
		{
			if (x is LayoutComponent)
			{
				returnList.Add((LayoutComponent)x);
			}
		});
		obj.Children.ForEach(delegate(UIObject x)
		{
			x.SearchForLayoutComponentsRecursive(x).ForEach(delegate(LayoutComponent y)
			{
				if (!returnList.Contains(y))
				{
					returnList.Add(y);
				}
			});
		});
		return returnList;
	}

	protected virtual void OnInvalidate(Invalidation invalidation)
	{
	}

	public void ClearInvalidation(Invalidation invalidation = Invalidation.All)
	{
		InvalidationState &= ~invalidation;
	}

	public void SetParent(UIObject? parent)
	{
		if (!IsDisposed)
		{
			if (Parent != null)
			{
				Parent.Children.Remove(this);
			}
			if (parent == null)
			{
				FContext.GetRootViewPane()?.Children.Add(this);
				Parent = FContext.GetRootViewPane();
			}
			else
			{
				Parent = parent;
				parent.Children.Add(this);
			}
		}
	}

	private void RemoveFromParent()
	{
		if (Parent != null)
		{
			Parent.Children.Remove(this);
		}
		Parent = null;
	}

	protected SKPaint GetRenderPaint()
	{
		return new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.White
		};
	}

	protected SKPaint GetDrawPaint()
	{
		return new SKPaint
		{
			IsAntialias = true,
			Color = SKColors.White
		};
	}

	public virtual void Begin()
	{
	}

	public virtual void LateBegin()
	{
	}

	public void OnReverseUpdate()
	{
		if (!IsDisposed)
		{
			if (InvalidationState.HasFlag(Invalidation.TransformDirty) || InvalidationState.HasFlag(Invalidation.LayoutDirty) || InvalidationState.HasFlag(Invalidation.SurfaceDirty))
			{
				Shape.UpdateShape();
			}
			ReverseUpdate();
		}
	}

	protected virtual void ReverseUpdate()
	{
	}

	public void OnLateReverseUpdate()
	{
		if (!IsDisposed)
		{
			if (lateLayoutUpdate || lateTransformUpdate)
			{
				Shape.UpdateShape();
			}
			ReverseUpdate();
		}
	}

	protected virtual void LateReverseUpdate()
	{
	}

	public void OnUpdate()
	{
		if (IsDisposed)
		{
			return;
		}
		CheckIfInsideParent();
		CheckIfSurfaceCanBeDisposed();
		if (!Enabled.CachedValue && _wasBeginCalled)
		{
			return;
		}
		DispatchBehaviorEvent(BehaviorEventType.BeforeUpdate);
		if (!_wasBeginCalled)
		{
			DispatchBehaviorEvent(BehaviorEventType.BeforeBegin);
			Begin();
			DispatchBehaviorEvent(BehaviorEventType.AfterBegin);
		}
		if (GlobalEnabled)
		{
			Update();
		}
		if (InvalidationState.HasFlag(Invalidation.TransformDirty) || InvalidationState.HasFlag(Invalidation.LayoutDirty) || InvalidationState.HasFlag(Invalidation.SurfaceDirty))
		{
			lateLayoutUpdate = true;
			if (InvalidationState.HasFlag(Invalidation.TransformDirty) || InvalidationState.HasFlag(Invalidation.LayoutDirty))
			{
				lateTransformUpdate = true;
				ClearInvalidation(Invalidation.TransformDirty);
				ClearInvalidation(Invalidation.LayoutDirty);
				DispatchBehaviorEvent(BehaviorEventType.BeforeLayout);
				DispatchBehaviorEvent(BehaviorEventType.BeforeTransform);
				Transform.UpdateTransform();
				Children.ForEach(delegate(UIObject x)
				{
					x.Invalidate(Invalidation.TransformDirty);
				});
				DispatchBehaviorEvent(BehaviorEventType.AfterTransform);
				DispatchBehaviorEvent(BehaviorEventType.AfterLayout);
			}
			Shape.UpdateShape();
			if (InvalidationState.HasFlag(Invalidation.SurfaceDirty))
			{
				ClearInvalidation(Invalidation.SurfaceDirty);
				ObjectSurface.InvalidateSurface(Shape.LocalBounds, Quality.CachedValue, Padding.CachedValue);
			}
		}
		if (GlobalEnabled)
		{
			Composition.GetZOrderedListOfChildren(this).ForEach(delegate(UIObject x)
			{
				x.OnUpdate();
			});
			if (InvalidationState.HasFlag(Invalidation.ChildDirty))
			{
				ClearInvalidation(Invalidation.ChildDirty);
			}
			DispatchBehaviorEvent(BehaviorEventType.AfterUpdate);
		}
	}

	protected virtual void Update()
	{
	}

	public void OnLateUpdate()
	{
		if (IsDisposed)
		{
			return;
		}
		DispatchBehaviorEvent(BehaviorEventType.BeforeLateUpdate);
		if (!_wasBeginCalled)
		{
			DispatchBehaviorEvent(BehaviorEventType.BeforeLateBegin);
			LateBegin();
			DispatchBehaviorEvent(BehaviorEventType.AfterLateBegin);
			_wasBeginCalled = true;
		}
		if (lateTransformUpdate || lateLayoutUpdate)
		{
			lateLayoutUpdate = false;
			if (lateTransformUpdate)
			{
				lateTransformUpdate = false;
				ClearInvalidation(Invalidation.TransformDirty);
				DispatchBehaviorEvent(BehaviorEventType.BeforeLayout);
				DispatchBehaviorEvent(BehaviorEventType.BeforeTransform);
				Transform.UpdateTransform();
				Children.ForEach(delegate(UIObject x)
				{
					x.Invalidate(Invalidation.TransformDirty);
				});
				DispatchBehaviorEvent(BehaviorEventType.AfterTransform);
				DispatchBehaviorEvent(BehaviorEventType.AfterLayout);
			}
		}
		LateUpdate();
		Composition.GetZOrderedListOfChildren(this).ForEach(delegate(UIObject x)
		{
			x.OnLateUpdate();
		});
		DispatchBehaviorEvent(BehaviorEventType.AfterLateUpdate);
	}

	protected virtual void LateUpdate()
	{
	}

	public void OnEarlyUpdate()
	{
		if (!IsDisposed)
		{
			DispatchBehaviorEvent(BehaviorEventType.BeforeEarlyUpdate);
			EarlyUpdate();
			Composition.GetZOrderedListOfChildren(this).ForEach(delegate(UIObject x)
			{
				x.OnEarlyUpdate();
			});
			DispatchBehaviorEvent(BehaviorEventType.AfterEarlyUpdate);
		}
	}

	protected virtual void EarlyUpdate()
	{
	}

	public void RenderToSurface(SKCanvas? canvas)
	{
		if (IsDisposed || canvas == null)
		{
			return;
		}
		if (FContext.GetCurrentWindow().Loop.CapFrameTime && FContext.CurrentFrameTimeMillis > (double)FContext.GetCurrentWindow().Loop.MaxFrameTime)
		{
			FContext.GetCurrentDispatcher().Invoke(delegate
			{
				Invalidate(Invalidation.SurfaceDirty);
			});
			return;
		}
		DispatchBehaviorEvent(BehaviorEventType.BeforeRender, canvas);
		Render(canvas);
		AfterRender(canvas);
		DispatchBehaviorEvent(BehaviorEventType.AfterRender, canvas);
		if (!FContext.GetCurrentWindow().DebugDisplayBounds)
		{
			return;
		}
		using SKPaint paint = new SKPaint
		{
			IsStroke = true,
			StrokeWidth = 2f,
			Color = SKColors.Green
		};
		canvas?.DrawRect(Shape.LocalBounds, paint);
	}

	public virtual void Render(SKCanvas canvas)
	{
	}

	public virtual void AfterRender(SKCanvas canvas)
	{
	}

	public virtual void DrawChildren(SKCanvas? canvas)
	{
		if (!IsDisposed)
		{
			DispatchBehaviorEvent(BehaviorEventType.BeforeDrawChildren, canvas);
			Composition.GetZOrderedListOfChildren(this).ForEach(delegate(UIObject x)
			{
				DispatchBehaviorEvent(BehaviorEventType.BeforeDrawChild, canvas);
				x.DrawToSurface(canvas);
				DispatchBehaviorEvent(BehaviorEventType.AfterDrawChild, (canvas, x));
			});
			DispatchBehaviorEvent(BehaviorEventType.AfterDrawChildren, canvas);
		}
	}

	public bool RenderThisFrame()
	{
		if (IsDisposed)
		{
			return false;
		}
		if (!RMath.IsRectPartiallyInside(Parent?.Shape.GlobalBounds ?? FContext.GetCurrentWindow().Shape.Bounds, Shape.GlobalBounds) && DisableWhenOutOfParentBounds)
		{
			return false;
		}
		if (!GlobalEnabled)
		{
			return false;
		}
		if (!GlobalVisible)
		{
			return false;
		}
		return true;
	}

	public virtual void DrawToSurface(SKCanvas? canvas)
	{
		if (IsDisposed || (!RenderThisFrame() && LocalVisible.CachedValue) || canvas == null)
		{
			return;
		}
		int? num = canvas?.Save();
		DispatchBehaviorEvent(BehaviorEventType.BeforeSurfaceDraw, canvas);
		canvas?.Concat(Transform.DrawMatrix);
		if (LocalVisible.CachedValue && canvas != null)
		{
			using SKPaint paint = GetDrawPaint();
			ObjectSurface.DrawFullChainToTarget(canvas, Shape.SurfaceDrawRect, paint, PostProcessChain);
		}
		DrawChildren(canvas);
		if (FContext.GetCurrentWindow().DebugDisplayBounds)
		{
			using SKPaint paint2 = new SKPaint
			{
				IsStroke = true,
				StrokeWidth = 2f,
				Color = SKColors.Red
			};
			canvas?.DrawRect(Shape.SurfaceDrawRect, paint2);
		}
		DispatchBehaviorEvent(BehaviorEventType.AfterSurfaceDraw, canvas);
		if (num.HasValue)
		{
			canvas?.RestoreToCount(num.Value);
		}
	}

	internal void DispatchBehaviorEvent(BehaviorEventType type, object? data = null)
	{
		if (IsDisposed)
		{
			return;
		}
		foreach (BehaviorComponent item in BehaviorComponents.ToList())
		{
			if (item.Enabled)
			{
				item.HandleEvent(type, data);
			}
		}
	}

	internal void OnEndFrame()
	{
		Children?.ForEach(delegate(UIObject x)
		{
			x.OnEndFrame();
		});
		LayoutChangedThisFrame = false;
	}

	public virtual void Dispose()
	{
		if (IsDisposed)
		{
			FLogger.Warn(GetType().FullName + " has already been disposed.");
			return;
		}
		OnObjectDisposed?.Invoke();
		OnObjectDisposed = null;
		Children.ToList().ForEach(delegate(UIObject x)
		{
			x.Dispose();
		});
		Children.Clear();
		BehaviorComponents.ToList().ForEach(delegate(BehaviorComponent x)
		{
			x.Dispose();
		});
		BehaviorComponents.Clear();
		Transform.Dispose();
		Transform = null;
		Layout.Dispose();
		Layout = null;
		InteractiveSurface.Dispose();
		InteractiveSurface = null;
		Composition.Dispose();
		Composition = null;
		ImageEffects.Dispose();
		ImageEffects = null;
		PostProcessChain.Dispose();
		PostProcessChain = null;
		RemoveFromParent();
		ObjectSurface.Dispose();
		ObjectSurface = null;
		IsDisposed = true;
	}

	private void CheckIfSurfaceCanBeDisposed()
	{
		if (!IsDisposed && (!GlobalEnabled || !GlobalVisible))
		{
			ObjectSurface.DisposeSurface();
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public virtual void OnInternalStateChanged<T>(T value)
	{
		if (!IsDisposed)
		{
			Invalidate(Invalidation.All);
		}
	}

	public override string ToString()
	{
		return $"[{InstanceID}] {GetType().FullName}";
	}
}
