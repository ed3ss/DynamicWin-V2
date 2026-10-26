using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Mathematics;
using FenUISharp.States;
using FenUISharp.WinFeatures;
using SkiaSharp;

namespace FenUISharp.Objects;

public class InteractiveSurface : IDisposable
{
	private string uniqueID = Guid.NewGuid().ToString();

	private volatile float _lastDelta;

	private Vector2 _startGlobalMousePos;

	private Vector2 _lastGlobalMousePos;

	private MouseInputCode _lastInputMouseState;

	private DateTime _lastInputMouseStateTime;

	private int _lastInputID;

	[ThreadStatic]
	private static List<InteractiveSurface> _surfaces;

	[ThreadStatic]
	private static InteractiveSurface? _topmostSurface;

	[ThreadStatic]
	private static InteractiveSurface? _topmostSurfaceMouseAction;

	[ThreadStatic]
	private static InteractiveSurface? _topmostSurfaceMouseScroll;

	[ThreadStatic]
	private static int activeInstances;

	public WeakReference<UIObject> Owner { get; private set; }

	private UIObject? owner
	{
		get
		{
			WeakReference<UIObject> weakReference = Owner;
			if (weakReference != null && weakReference.TryGetTarget(out UIObject target))
			{
				return target;
			}
			return null;
		}
	}

	public State<SKRect> GlobalSurface { get; init; }

	private Dispatcher Dispatcher { get; init; }

	public State<int> ExtendInteractionRadius { get; init; }

	public State<bool> IgnoreInteractions { get; init; }

	public State<bool> IgnoreChildInteractions { get; init; }

	public State<bool> EnableMouseActions { get; init; }

	public State<bool> EnableMouseScrolling { get; init; }

	public bool IsMouseHovering { get; set; }

	public bool IsMouseDown { get; set; }

	public bool IsDragging { get; set; }

	public float DoubleMouseActionTimeFrame { get; set; } = 0.4f;

	public float LongMouseActionTime { get; set; } = 1f;

	public bool MouseInteractionCallbackOnChildMouseInteraction { get; set; }

	public Action? OnMouseEnter { get; set; }

	public Action? OnMouseStay { get; set; }

	public Action? OnMouseExit { get; set; }

	public Action<Vector2>? OnMouseMove { get; set; }

	public Action<MouseInputCode>? OnMouseAction { get; set; }

	public Action<MouseInputButton>? OnDoubleMouseAction { get; set; }

	public Action<MouseInputButton>? OnLongMouseAction { get; set; }

	public Action<float>? OnMouseScroll { get; set; }

	public Action<Vector2>? OnDrag { get; set; }

	public Action<Vector2>? OnDragDelta { get; set; }

	public Action? OnDragStart { get; set; }

	public Action? OnDragEnd { get; set; }

	private bool ParentIgnoreChild
	{
		get
		{
			if (owner?.Parent?.InteractiveSurface.IgnoreChildInteractions.CachedValue != true)
			{
				return owner?.Parent?.InteractiveSurface.ParentIgnoreChild == true;
			}
			return true;
		}
	}

	public InteractiveSurface(UIObject owner, Dispatcher dispatcher, Func<SKRect> globalSurface)
	{
		Owner = new WeakReference<UIObject>(owner);
		GlobalSurface = new State<SKRect>(globalSurface, owner, delegate
		{
		});
		Dispatcher = dispatcher;
		ExtendInteractionRadius = new State<int>(() => 2, owner, delegate
		{
		});
		IgnoreInteractions = new State<bool>(() => false, owner, delegate
		{
		});
		IgnoreChildInteractions = new State<bool>(() => false, owner, delegate
		{
		});
		EnableMouseActions = new State<bool>(() => false, owner, delegate
		{
		});
		EnableMouseScrolling = new State<bool>(() => false, owner, delegate
		{
		});
		FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
		callbacks.ClientMouseAction = (Action<MouseInputCode>)Delegate.Combine(callbacks.ClientMouseAction, new Action<MouseInputCode>(FuncOnMouseAction));
		FWindowCallbacks callbacks2 = FContext.GetCurrentWindow().Callbacks;
		callbacks2.OnMouseScroll = (Action<float>)Delegate.Combine(callbacks2.OnMouseScroll, new Action<float>(Global_FuncOnMouseScroll));
		FWindowCallbacks callbacks3 = FContext.GetCurrentWindow().Callbacks;
		callbacks3.OnMouseMove = (Action<Vector2>)Delegate.Combine(callbacks3.OnMouseMove, new Action<Vector2>(Global_FuncOnMouseMove));
		GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
		globalHooks.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(globalHooks.OnMouseAction, new Action<MouseInputCode>(FuncOnMouseActionGlobal));
		if (activeInstances == 0)
		{
			_surfaces = new List<InteractiveSurface>();
			FWindowCallbacks callbacks4 = FContext.GetCurrentWindow().Callbacks;
			callbacks4.OnPreUpdate = (Action)Delegate.Combine(callbacks4.OnPreUpdate, new Action(CacheTopmostMouseAction));
			FWindowCallbacks callbacks5 = FContext.GetCurrentWindow().Callbacks;
			callbacks5.OnPreUpdate = (Action)Delegate.Combine(callbacks5.OnPreUpdate, new Action(CacheTopmostMouseScroll));
		}
		activeInstances++;
		_surfaces.Add(this);
	}

	private void CacheTopmostMouseAction()
	{
		UIObject uIObject = owner;
		if (uIObject == null)
		{
			return;
		}
		List<UIObject> zOrderedListOfEnabled = uIObject.Composition.GetZOrderedListOfEnabled();
		int num = zOrderedListOfEnabled.Count - 1;
		while (num >= 0)
		{
			UIObject obj = zOrderedListOfEnabled[num];
			List<InteractiveSurface> list = _surfaces.Where((InteractiveSurface s) => s.owner == obj && s.owner.GlobalEnabled && !s.IgnoreInteractions.CachedValue && !s.ParentIgnoreChild && s.EnableMouseActions.CachedValue && s.TestForGlobalPoint(FContext.GetCurrentWindow().ClientMousePosition)).ToList();
			if (list.Count == 0)
			{
				_topmostSurfaceMouseAction = null;
				num--;
				continue;
			}
			_topmostSurfaceMouseAction = list[0];
			break;
		}
	}

	private void CacheTopmostMouseScroll()
	{
		UIObject uIObject = owner;
		if (uIObject == null)
		{
			return;
		}
		List<UIObject> zOrderedListOfEnabled = uIObject.Composition.GetZOrderedListOfEnabled();
		int num = zOrderedListOfEnabled.Count - 1;
		while (num >= 0)
		{
			UIObject obj = zOrderedListOfEnabled[num];
			List<InteractiveSurface> list = _surfaces.Where((InteractiveSurface s) => s.owner == obj && s.owner.GlobalEnabled && !s.IgnoreInteractions.CachedValue && !s.ParentIgnoreChild && s.EnableMouseScrolling.CachedValue && s.TestForGlobalPoint(FContext.GetCurrentWindow().ClientMousePosition)).ToList();
			if (list.Count == 0)
			{
				_topmostSurfaceMouseScroll = null;
				num--;
				continue;
			}
			_topmostSurfaceMouseScroll = list[0];
			break;
		}
	}

	private void FuncOnMouseActionGlobal(MouseInputCode code)
	{
		UIObject uIObject = owner;
		if (uIObject != null && uIObject.GlobalEnabled && uIObject.GlobalVisible)
		{
			Dispatcher.InvokeWithID(delegate
			{
				FuncOnMouseMoveGlobal(code);
			}, uniqueID + "-globalmousemove");
		}
	}

	private void Global_FuncOnMouseScroll(float obj)
	{
		UIObject uIObject = owner;
		if (uIObject != null && uIObject.GlobalEnabled && uIObject.GlobalVisible)
		{
			_lastDelta += obj;
			Dispatcher.InvokeWithID(delegate
			{
				FuncOnMouseScroll();
			}, uniqueID + "-mousescroll");
		}
	}

	private void Global_FuncOnMouseMove(Vector2 vector)
	{
		UIObject uIObject = owner;
		if (uIObject != null && uIObject.GlobalEnabled && uIObject.GlobalVisible)
		{
			Dispatcher.InvokeWithID(delegate
			{
				FuncOnMouseMove();
			}, uniqueID + "-mousemove");
		}
	}

	private void FuncOnMouseScroll()
	{
		UIObject uIObject = owner;
		if (uIObject != null && uIObject.GlobalEnabled && uIObject.GlobalVisible && FContext.GetCurrentWindow().Properties.IsWindowFocused && TestIfTopMost_MouseScrolling() && TestForGlobalPoint(FContext.GetCurrentWindow().ClientMousePosition))
		{
			OnMouseScroll?.Invoke(_lastDelta);
			_lastDelta = 0f;
		}
	}

	private void FuncOnMouseMove()
	{
		UIObject uIObject = owner;
		if (uIObject == null || !uIObject.GlobalEnabled || !uIObject.GlobalVisible)
		{
			return;
		}
		OnMouseMove?.Invoke(FContext.GetCurrentWindow().ClientMousePosition);
		FuncProcessDrag();
		if (!TestIfTopMost_MouseInteraction() || !TestForGlobalPoint(FContext.GetCurrentWindow().ClientMousePosition))
		{
			if (IsMouseHovering)
			{
				if (_topmostSurface == this)
				{
					_topmostSurface = null;
				}
				IsMouseHovering = false;
				OnMouseExit?.Invoke();
			}
		}
		else
		{
			if (!IsMouseHovering)
			{
				OnMouseEnter?.Invoke();
				_topmostSurface = this;
			}
			OnMouseStay?.Invoke();
			IsMouseHovering = true;
		}
	}

	private void FuncProcessDrag()
	{
		UIObject uIObject = owner;
		if (uIObject != null && uIObject.GlobalEnabled && uIObject.GlobalVisible)
		{
			if (IsMouseDown && !IsDragging && IsMouseHovering)
			{
				_startGlobalMousePos = FContext.GetCurrentWindow().ClientMousePosition;
				_lastGlobalMousePos = FContext.GetCurrentWindow().ClientMousePosition;
				OnDragStart?.Invoke();
				IsDragging = true;
			}
			if (IsDragging)
			{
				OnDrag?.Invoke(FContext.GetCurrentWindow().ClientMousePosition - _startGlobalMousePos);
				OnDragDelta?.Invoke(FContext.GetCurrentWindow().ClientMousePosition - _lastGlobalMousePos);
				_lastGlobalMousePos = FContext.GetCurrentWindow().ClientMousePosition;
			}
		}
	}

	private void StopDragging()
	{
		_startGlobalMousePos = new Vector2(0f, 0f);
		_lastGlobalMousePos = new Vector2(0f, 0f);
		OnDragEnd?.Invoke();
		IsDragging = false;
	}

	private void FuncOnMouseMoveGlobal(MouseInputCode code)
	{
		UIObject uIObject = owner;
		if (uIObject != null && uIObject.GlobalEnabled && uIObject.GlobalVisible && code.button == MouseInputButton.Left && code.state == MouseInputState.Up && IsMouseDown)
		{
			IsMouseDown = false;
			if (IsDragging)
			{
				StopDragging();
			}
		}
	}

	private void FuncOnMouseAction(MouseInputCode code)
	{
		UIObject uIObject = owner;
		if (uIObject == null || !uIObject.GlobalEnabled || !uIObject.GlobalVisible)
		{
			return;
		}
		if (TestIfTopMost_Dragging() && TestForGlobalPoint(FContext.GetCurrentWindow().ClientMousePosition) && IsDragging)
		{
			StopDragging();
		}
		if (code.state == MouseInputState.Up && code.button == MouseInputButton.Left)
		{
			IsMouseDown = false;
			if (IsDragging)
			{
				StopDragging();
			}
		}
		if (!TestIfTopMost_MouseInteraction() || !TestForGlobalPoint(FContext.GetCurrentWindow().ClientMousePosition))
		{
			return;
		}
		if (code.button == MouseInputButton.Left && code.state == MouseInputState.Down)
		{
			IsMouseDown = true;
		}
		OnMouseAction?.Invoke(code);
		if (_lastInputMouseState.button == code.button && code.state == MouseInputState.Up)
		{
			if ((DateTime.Now - _lastInputMouseStateTime).TotalSeconds <= (double)DoubleMouseActionTimeFrame)
			{
				_lastInputMouseState = new MouseInputCode
				{
					button = MouseInputButton.None
				};
				OnDoubleMouseAction?.Invoke(code.button);
			}
			_lastInputMouseState = code;
			_lastInputMouseStateTime = DateTime.Now;
		}
		if (code.state != MouseInputState.Down)
		{
			return;
		}
		_lastInputID++;
		int id = _lastInputID;
		Dispatcher.InvokeLater(delegate
		{
			if (_lastInputID == id)
			{
				OnLongMouseAction?.Invoke(code.button);
			}
		}, LongMouseActionTime);
	}

	public bool TestForGlobalPoint(in Vector2 point)
	{
		if (owner == null)
		{
			return false;
		}
		if (GetGlobalInteractionRect().Contains(point.x, point.y))
		{
			if (owner.Parent == null)
			{
				return true;
			}
			return owner.Parent.InteractiveSurface.TestForGlobalPoint(in point);
		}
		return false;
	}

	private bool TestIfTopMost_MouseInteraction()
	{
		UIObject uIObject = owner;
		if (uIObject == null)
		{
			return false;
		}
		if (MouseInteractionCallbackOnChildMouseInteraction && uIObject.Children.Any((UIObject x) => x.InteractiveSurface.TestIfTopMost_MouseInteraction()))
		{
			return true;
		}
		return _topmostSurfaceMouseAction == this;
	}

	private bool TestIfTopMost_MouseScrolling()
	{
		return _topmostSurfaceMouseScroll == this;
	}

	private bool TestIfTopMost_Dragging()
	{
		return _topmostSurfaceMouseAction == this;
	}

	public SKRect GetGlobalInteractionRect()
	{
		SKRect cachedValue = GlobalSurface.CachedValue;
		cachedValue.Inflate(ExtendInteractionRadius.CachedValue, ExtendInteractionRadius.CachedValue);
		return cachedValue;
	}

	public void Dispose()
	{
		if (_surfaces != null)
		{
			_surfaces.Remove(this);
		}
		Owner = null;
		if (!FContext.IsDisposingWindow)
		{
			FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
			callbacks.ClientMouseAction = (Action<MouseInputCode>)Delegate.Remove(callbacks.ClientMouseAction, new Action<MouseInputCode>(FuncOnMouseAction));
			FWindowCallbacks callbacks2 = FContext.GetCurrentWindow().Callbacks;
			callbacks2.OnMouseScroll = (Action<float>)Delegate.Remove(callbacks2.OnMouseScroll, new Action<float>(Global_FuncOnMouseScroll));
			FWindowCallbacks callbacks3 = FContext.GetCurrentWindow().Callbacks;
			callbacks3.OnMouseMove = (Action<Vector2>)Delegate.Remove(callbacks3.OnMouseMove, new Action<Vector2>(Global_FuncOnMouseMove));
			GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
			globalHooks.OnMouseAction = (Action<MouseInputCode>)Delegate.Remove(globalHooks.OnMouseAction, new Action<MouseInputCode>(FuncOnMouseActionGlobal));
		}
		activeInstances--;
		if (activeInstances <= 0 && !FContext.IsDisposingWindow)
		{
			FWindowCallbacks callbacks4 = FContext.GetCurrentWindow().Callbacks;
			callbacks4.OnPreUpdate = (Action)Delegate.Combine(callbacks4.OnPreUpdate, new Action(CacheTopmostMouseAction));
			FWindowCallbacks callbacks5 = FContext.GetCurrentWindow().Callbacks;
			callbacks5.OnPreUpdate = (Action)Delegate.Combine(callbacks5.OnPreUpdate, new Action(CacheTopmostMouseScroll));
		}
	}
}
