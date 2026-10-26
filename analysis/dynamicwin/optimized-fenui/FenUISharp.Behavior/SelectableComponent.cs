using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Logging;
using FenUISharp.Objects;
using FenUISharp.WinFeatures;
using SkiaSharp;

namespace FenUISharp.Behavior;

public class SelectableComponent : BehaviorComponent
{
	private KeyBind tabKeybind;

	private KeyBind reverseTabKeybind;

	[ThreadStatic]
	private static List<SelectableComponent> selectableComponents;

	[ThreadStatic]
	private static SelectableComponent? currentlySelected;

	[ThreadStatic]
	private static SelectableComponent? lastSelected;

	private Dispatcher? dispatcher;

	[ThreadStatic]
	private static bool renderSelection;

	public InteractiveSurface Surface { get; init; }

	public bool IsSelected { get; private set; }

	public Action? OnSelectionGained { get; set; }

	public Action? OnSelectionLost { get; set; }

	public SelectableComponent(UIObject owner, InteractiveSurface surface)
		: base(owner)
	{
		Surface = surface;
		if (selectableComponents == null)
		{
			selectableComponents = new List<SelectableComponent>();
		}
		if (selectableComponents.Count <= 0)
		{
			dispatcher = FContext.GetCurrentDispatcher();
			GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
			globalHooks.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(globalHooks.OnMouseAction, new Action<MouseInputCode>(OnGlobalClick));
		}
		selectableComponents.Add(this);
		tabKeybind = new KeyBind
		{
			VKCode = 9,
			OnKeybindExecuted = OnTabPressed
		};
		reverseTabKeybind = new KeyBind
		{
			VKCode = 9,
			Flags = KeyBindFlags.Shift,
			OnKeybindExecuted = OnReverseTabPressed
		};
		surface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(surface.OnMouseAction, new Action<MouseInputCode>(OnClick));
		if (currentlySelected == null)
		{
			SetSelected(this);
		}
	}

	private void OnClick(MouseInputCode c)
	{
		if (c.state == MouseInputState.Up && c.button == MouseInputButton.Left)
		{
			SetSelected(this);
			renderSelection = false;
		}
	}

	private void OnGlobalClick(MouseInputCode c)
	{
		if (c.button == MouseInputButton.Left)
		{
			renderSelection = false;
		}
		if (c.button == MouseInputButton.Left && c.state == MouseInputState.Down)
		{
			dispatcher?.Invoke(delegate
			{
				SetSelected(null);
			});
		}
	}

	public static void SetObjectSelected(UIObject? selectable)
	{
		if (selectable == null)
		{
			SetSelected(null);
		}
		else if (selectable.BehaviorComponents.Any((BehaviorComponent x) => x is SelectableComponent))
		{
			SetSelected((SelectableComponent)selectable.BehaviorComponents.Last((BehaviorComponent x) => x is SelectableComponent));
		}
	}

	public static void SetSelected(SelectableComponent? selectableComponent)
	{
		lastSelected = currentlySelected;
		if (currentlySelected != null)
		{
			currentlySelected.IsSelected = false;
			currentlySelected.OnSelectionLost?.Invoke();
			currentlySelected.Owner?.Invalidate(UIObject.Invalidation.SurfaceDirty);
			FContext.GetKeyboardInputManager().UnregisterKeybind(currentlySelected.tabKeybind);
			FContext.GetKeyboardInputManager().UnregisterKeybind(currentlySelected.reverseTabKeybind);
		}
		currentlySelected = selectableComponent;
		if (selectableComponent != null && currentlySelected != null)
		{
			currentlySelected.IsSelected = true;
			currentlySelected.OnSelectionGained?.Invoke();
			currentlySelected.Owner?.Invalidate(UIObject.Invalidation.SurfaceDirty);
			FContext.GetKeyboardInputManager().RegisterKeybind(currentlySelected.tabKeybind);
			FContext.GetKeyboardInputManager().RegisterKeybind(currentlySelected.reverseTabKeybind);
			if (lastSelected == null)
			{
				lastSelected = currentlySelected;
			}
		}
	}

	public override void ComponentDestroy()
	{
		base.ComponentDestroy();
		if (FContext.IsValidContext())
		{
			if (currentlySelected == this)
			{
				currentlySelected = null;
			}
			InteractiveSurface surface = Surface;
			surface.OnMouseAction = (Action<MouseInputCode>)Delegate.Remove(surface.OnMouseAction, new Action<MouseInputCode>(OnClick));
			selectableComponents.Remove(this);
			FContext.GetKeyboardInputManager().UnregisterKeybind(tabKeybind);
			if (selectableComponents.Count <= 0)
			{
				GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
				globalHooks.OnMouseAction = (Action<MouseInputCode>)Delegate.Remove(globalHooks.OnMouseAction, new Action<MouseInputCode>(OnGlobalClick));
			}
		}
	}

	private void OnTabPressed()
	{
		renderSelection = true;
		if (IsSelected && (currentlySelected != null || selectableComponents.IndexOf(this) != 0))
		{
			int num = ((currentlySelected != null) ? (selectableComponents.IndexOf(currentlySelected) + 1) : ((lastSelected != null) ? (selectableComponents.IndexOf(lastSelected) + 1) : 0)) % selectableComponents.Count;
			if (num < 0)
			{
				num = selectableComponents.Count - 1;
			}
			SetSelected(selectableComponents[num]);
		}
	}

	private void OnReverseTabPressed()
	{
		renderSelection = true;
		if (IsSelected && (currentlySelected != null || selectableComponents.IndexOf(this) != 0))
		{
			int num = ((currentlySelected != null) ? (selectableComponents.IndexOf(currentlySelected) - 1) : ((lastSelected != null) ? (selectableComponents.IndexOf(lastSelected) - 1) : 0)) % selectableComponents.Count;
			if (num < 0)
			{
				num = selectableComponents.Count - 1;
			}
			FLogger.Log("Current Index: " + num);
			SetSelected(selectableComponents[num]);
		}
	}

	public override void HandleEvent(BehaviorEventType type, object? data = null)
	{
		base.HandleEvent(type, data);
		if (type == BehaviorEventType.AfterRender && (SKCanvas)data != null)
		{
			RenderSelection((SKCanvas)data);
		}
	}

	private void RenderSelection(SKCanvas canvas)
	{
		if (!renderSelection || !IsSelected)
		{
			return;
		}
		SKRect rect = base.Owner?.Shape.LocalBounds ?? new SKRect(0f, 0f, 0f, 0f);
		rect.Inflate(3f, 3f);
		using SKPath path = SKSquircle.CreateSquircle(rect, 10f);
		SKPaint sKPaint = new SKPaint();
		sKPaint.IsStroke = true;
		sKPaint.StrokeMiter = 3f;
		sKPaint.PathEffect = SKPathEffect.CreateDash(new float[2] { 2f, 6f }, 1f);
		sKPaint.IsAntialias = true;
		sKPaint.Color = new SKColor(150, 150, 150, 200);
		using SKPaint paint = sKPaint;
		canvas.DrawPath(path, paint);
	}
}
