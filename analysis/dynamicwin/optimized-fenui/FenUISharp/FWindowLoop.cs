using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FenUISharp.Logging;
using FenUISharp.Native;
using FenUISharp.Objects;

namespace FenUISharp;

public class FWindowLoop : IDisposable
{
	private bool _delayedFocus = true;

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

	public bool PauseUpdateLoopWhenLoseFocus { get; set; }

	public bool PauseUpdateLoopWhenHidden { get; set; } = true;

	public bool CapFrameTime { get; set; }

	public float MaxFrameTime { get; set; } = 32f;

	public Func<bool>? _logicIsRunning { get; set; }

	public Func<bool>? _windowIsRunning { get; set; }

	private Thread? LogicThread { get; set; }

	public FWindowLoop(FWindow window)
	{
		this.window = new WeakReference<FWindow>(window);
	}

	public void Begin(Action? setupLogic)
	{
		Thread.CurrentThread.Name = "Window Thread";
		LogicThread = new Thread((ThreadStart)delegate
		{
			setupLogic?.Invoke();
			SetupLogic();
			LoopLogic();
		});
		LogicThread.Name = "Logic Thread";
		LogicThread.IsBackground = false;
		LogicThread.Priority = ThreadPriority.Normal;
		LogicThread.SetApartmentState(ApartmentState.STA);
		LogicThread.Start();
		while (true)
		{
			Func<bool>? windowIsRunning = _windowIsRunning;
			if (windowIsRunning != null && windowIsRunning())
			{
				Window?.WindowDispatcher?.UpdateQueue();
				MSG lpMsg;
				while (Win32APIs.PeekMessage(out lpMsg, IntPtr.Zero, 0u, 0u, 1u))
				{
					Win32APIs.TranslateMessage(ref lpMsg);
					Win32APIs.DispatchMessage(ref lpMsg);
				}
				Thread.Sleep((Window?.Properties?.IsWindowFocused == true) ? 2 : 15);
				continue;
			}
			break;
		}
	}

	protected virtual void SetupLogic()
	{
	}

	protected virtual void LoopLogic()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		using FrameWaiter frameWaiter = new FrameWaiter();
		double num = 1000.0 / (double)Window.TargetRefreshRate;
		double num2 = 0.0;
		double num3 = 0.0;
		while (true)
		{
			Func<bool>? logicIsRunning = _logicIsRunning;
			if (logicIsRunning == null || !logicIsRunning())
			{
				break;
			}
			double totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
			double num4 = num2 - totalMilliseconds;
			if ((!_delayedFocus && PauseUpdateLoopWhenLoseFocus) || (!Window.Properties.IsWindowVisible && PauseUpdateLoopWhenHidden))
			{
				_delayedFocus = Window.Properties.IsWindowFocused;
				num3 = totalMilliseconds;
				WindowUpdate(isPaused: true);
				if (!_logicIsRunning())
				{
					break;
				}
				Thread.Sleep((!Window.Properties.IsWindowVisible) ? 300 : 100);
			}
			else if (num4 <= 0.0)
			{
				Window.Time.DeltaTime = (float)(totalMilliseconds - num3) / 1000f;
				num3 = totalMilliseconds;
				FContext.frameStartTime = DateTime.Now;
				WindowUpdate();
				if (!_logicIsRunning())
				{
					break;
				}
				if (Window.Surface.IsNextFrameRendering())
				{
					Window.Callbacks.OnBeginRender?.Invoke();
					Window.SkiaDirectCompositionContext?.Draw();
					Window.Callbacks.OnEndRender?.Invoke();
					Window._isDirty = false;
					Window._fullRedraw = false;
				}
				num2 = totalMilliseconds + num;
				_delayedFocus = Window.Properties.IsWindowFocused;
			}
			else
			{
				frameWaiter.Wait(num4);
			}
		}
	}

	protected virtual void WindowUpdate(bool isPaused = false)
	{
		Window.Time.Time += Window.Time.DeltaTime;
		Window.CallUpdate();
		Window.Shape.UpdateShape();
		Window.LogicDispatcher.UpdateQueue();
		Func<bool>? logicIsRunning = _logicIsRunning;
		if ((logicIsRunning == null || logicIsRunning()) && !isPaused)
		{
			Window.Callbacks.OnPreUpdate?.Invoke();
			List<UIObject> obj = Window.Surface.RootViewPane?.Composition.GetZOrderedListOfEverything().ToList();
			obj?.Reverse();
			obj?.ForEach(delegate(UIObject x)
			{
				x.OnReverseUpdate();
			});
			Window.Surface.RootViewPane?.OnEarlyUpdate();
			Window.Surface.RootViewPane?.OnUpdate();
			Window.Callbacks.OnPostUpdate?.Invoke();
			obj?.ForEach(delegate(UIObject x)
			{
				x.OnLateReverseUpdate();
			});
			Window.Surface.RootViewPane?.OnLateUpdate();
			Window.Surface.RootViewPane?.OnEndFrame();
		}
	}

	public void Dispose()
	{
		FLogger.Log<FWindowLoop>("Disposing window loop and logic thread");
		LogicThread?.Interrupt();
		LogicThread = null;
	}
}
