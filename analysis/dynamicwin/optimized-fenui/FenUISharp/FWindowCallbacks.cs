using System;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp;

public class FWindowCallbacks : IDisposable
{
	private WeakReference<FWindow> window { get; set; }

	public FWindow Window
	{
		get
		{
			if (!window.TryGetTarget(out FWindow target))
			{
				throw new Exception("Window not set.");
			}
			return target;
		}
	}

	public Action<FDropData?>? OnDragEnter { get; set; }

	public Action<FDropData?>? OnDragOver { get; set; }

	public Action<FDropData?>? OnDragDrop { get; set; }

	public Action? OnDragLeave { get; set; }

	public Action<MouseInputCode>? ClientMouseAction { get; set; }

	public Action<MouseInputCode>? TrayMouseAction { get; set; }

	public Action<float>? OnMouseScroll { get; set; }

	public Action<Vector2>? OnMouseMove { get; set; }

	public Action? OnMouseLeft { get; set; }

	public Action? OnBeginRender { get; set; }

	public Action? OnEndRender { get; set; }

	public Action<SKSurface>? OnWindowBeforeDraw { get; set; }

	public Action<SKSurface>? OnWindowAfterDraw { get; set; }

	public Action? OnPostUpdate { get; set; }

	public Action? OnPreUpdate { get; set; }

	public Action? OnDevicesChanged { get; set; }

	public Action? OnFocusLost { get; set; }

	public Action? OnFocusGained { get; set; }

	public Action<Vector2>? OnWindowResize { get; set; }

	public Action<Vector2>? OnWindowEndResize { get; set; }

	public Action<Vector2>? OnWindowMove { get; set; }

	public Action<Vector2>? OnWindowEndMove { get; set; }

	public Action? DPIChanged { get; set; }

	public Action? OnWindowClose { get; set; }

	public Action? OnWindowDestroy { get; set; }

	internal Action<char>? OnKeyboardInputTextReceived { get; set; }

	public FWindowCallbacks(FWindow window)
	{
		this.window = new WeakReference<FWindow>(window);
		Window.LogicDispatcher.Invoke(delegate
		{
			if (Window.DropTarget == null)
			{
				throw new Exception("DropTarget does not exist");
			}
			DragDropHandler? dropTarget = Window.DropTarget;
			dropTarget.dragOver = (Action<FDropData>)Delegate.Combine(dropTarget.dragOver, (Action<FDropData>)delegate
			{
				Window.ClientMousePosition = Window.DropTarget.LastMouseDragPosition;
				Window.Callbacks.OnMouseMove?.Invoke(Window.ClientMousePosition);
			});
		});
	}

	public void Dispose()
	{
	}
}
