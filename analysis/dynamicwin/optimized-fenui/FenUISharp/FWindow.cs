using System;
using System.Runtime.InteropServices;
using System.Text;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Native;
using FenUISharp.Objects;
using FenUISharp.Themes;
using SkiaSharp;

namespace FenUISharp;

public abstract class FWindow : IDisposable
{
	public MultiAccess<Cursor> ActiveCursor = new MultiAccess<Cursor>(Cursor.ARROW);

	internal bool _disposingOrDisposed;

	internal bool _isRunning;

	internal bool _isDirty;

	internal bool _fullRedraw;

	private static readonly WndProcDelegate _wndProcDelegate = StaticWndProc;

	private bool _disableSDXCC;

	public nint hWnd { get; private set; }

	public string WindowTitle
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			Win32APIs.GetWindowTextA(hWnd, stringBuilder, 999);
			return stringBuilder.ToString();
		}
		set
		{
			FLogger.Log<FWindow>($"Changing title of {hWnd} to {value}");
			Win32APIs.SetWindowTextA(hWnd, value);
		}
	}

	public string WindowClass { get; protected set; }

	public int TargetRefreshRate { get; set; } = 60;

	public FWindowShape Shape { get; protected set; }

	public FWindowProcedure Procedure { get; protected set; }

	public FWindowLoop Loop { get; protected set; }

	public FWindowCallbacks Callbacks { get; protected set; }

	public FWindowProperties Properties { get; protected set; }

	public FWindowSurface Surface { get; protected set; }

	public DragDropHandler? DropTarget { get; private set; }

	public FTime Time { get; protected set; } = new FTime();

	public Dispatcher LogicDispatcher { get; private set; }

	public Dispatcher WindowDispatcher { get; private set; }

	public KeyboardInputManager? WindowKeyboardInput { get; private set; }

	public ThemeManager WindowThemeManager { get; private set; }

	public Vector2 ClientMousePosition { get; internal set; }

	public SkiaDirectCompositionContext? SkiaDirectCompositionContext { get; set; }

	public bool DebugDisplayAreaCache { get; internal set; }

	public bool DebugDisplayBounds { get; internal set; }

	public bool DebugDisplayObjectIDs { get; internal set; }

	public FWindow(string title, string className, Vector2? position = null, Vector2? size = null)
	{
		FLogger.Log<FWindow>("Creating window with title " + title + " and class " + className);
		if (!FenUI.HasBeenInitialized)
		{
			throw new Exception("FenUI has to be initialized before creating a window.");
		}
		FenUI.activeInstances.Add(this);
		FLogger.Log<FWindow>("Creating dispatchers");
		LogicDispatcher = new Dispatcher();
		WindowDispatcher = new Dispatcher();
		FLogger.Log<FWindow>("Registering class...");
		WindowClass = className;
		WNDCLASSEX wndClass = RegisterClass(className);
		FLogger.Log<FWindow>($"Creating window of type {GetType()}...");
		hWnd = CreateWindow(wndClass, position ?? new Vector2(-1f, -1f), size ?? new Vector2(600f, 800f));
		if (hWnd == IntPtr.Zero)
		{
			throw new Exception($"Window creation failed: error {Marshal.GetLastWin32Error()}");
		}
		WindowTitle = title;
		FLogger.Log<FWindow>("Gathering window components");
		(FWindowShape, FWindowProcedure, FWindowLoop, FWindowCallbacks, FWindowProperties, FWindowSurface) components = GetComponents();
		FLogger.Log<FWindow>("Assinging window components to fields");
		Shape = components.Item1;
		Procedure = components.Item2;
		Loop = components.Item3;
		Callbacks = components.Item4;
		Properties = components.Item5;
		Surface = components.Item6;
		FWindowCallbacks callbacks = Callbacks;
		callbacks.OnWindowEndMove = (Action<Vector2>)Delegate.Combine(callbacks.OnWindowEndMove, (Action<Vector2>)delegate
		{
			SetTargetRefreshrateToMonitorRefreshrate();
		});
		Properties.UseSystemDarkMode = true;
		WindowThemeManager = new ThemeManager(Resources.GetTheme("default-dark"));
		FullRedraw();
		FLogger.Log<FWindow>("Allocating this class and getting GCHandle");
		GCHandle value = GCHandle.Alloc(this);
		FLogger.Log<FWindow>("Allocating the GCHandle in GWLP_USERDATA");
		Win32APIs.SetWindowLongPtr(hWnd, -21, GCHandle.ToIntPtr(value));
		FLogger.Log<FWindow>("Pre-Init OLE DragDrop");
		DragDropRegistration.Initialize();
		OnAfterWindowCreation();
		FLogger.Log<FWindow>("Window creation done!");
		FLogger.Log<FWindow>("");
	}

	protected virtual void OnAfterWindowCreation()
	{
	}

	protected virtual (FWindowShape, FWindowProcedure, FWindowLoop, FWindowCallbacks, FWindowProperties, FWindowSurface) GetComponents()
	{
		FWindowShape item = new FWindowShape(this);
		FWindowProcedure item2 = new FWindowProcedure(this)
		{
			_isRunning = () => _isRunning
		};
		FWindowCallbacks item3 = new FWindowCallbacks(this);
		FWindowLoop item4 = new FWindowLoop(this)
		{
			_logicIsRunning = () => _isRunning && !_disposingOrDisposed,
			_windowIsRunning = () => _isRunning
		};
		FWindowProperties item5 = new FWindowProperties(this);
		FWindowSurface item6 = new FWindowSurface(this);
		return (item, item2, item4, item3, item5, item6);
	}

	public virtual void WithView(View model)
	{
		LogicDispatcher.Invoke(delegate
		{
			if (Surface.RootViewPane != null)
			{
				Surface.RootViewPane.ViewModel = model;
			}
		});
	}

	protected abstract nint CreateWindow(WNDCLASSEX wndClass, Vector2 position, Vector2 size);

	protected virtual WNDCLASSEX RegisterClass(string className)
	{
		WindowClass = className;
		WNDCLASSEX lpwcx = new WNDCLASSEX
		{
			cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
			style = 32u,
			lpfnWndProc = _wndProcDelegate,
			hInstance = Marshal.GetHINSTANCE(typeof(FenUI).Module),
			lpszClassName = className
		};
		Win32APIs.RegisterClassExA(ref lpwcx);
		return lpwcx;
	}

	private static nint StaticWndProc(nint hWnd, uint msg, nint wParam, nint lParam)
	{
		nint windowLongPtrW = Win32APIs.GetWindowLongPtrW(hWnd, -21);
		if (windowLongPtrW != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(windowLongPtrW);
			if (gCHandle.Target != null && gCHandle.Target is FWindow)
			{
				return ((FWindow)gCHandle.Target).Procedure.WindowsProcedure(hWnd, msg, wParam, lParam);
			}
		}
		return Win32APIs.DefWindowProcW(hWnd, msg, wParam, lParam);
	}

	protected virtual void SetupWindowLogicOnBegin()
	{
		FContext.WithWindow(this);
		if (!_disableSDXCC)
		{
			SkiaDirectCompositionContext = new SkiaDirectCompositionContext(this, DrawAction);
		}
		WindowKeyboardInput = new KeyboardInputManager(this);
		Surface.SetupSurface();
	}

	public void DisableDirectContextCreation()
	{
		_disableSDXCC = true;
	}

	protected virtual void DrawAction(SKCanvas canvas)
	{
		Surface.Draw(canvas);
		_isDirty = false;
		_fullRedraw = false;
	}

	internal virtual void ClearSurface(SKCanvas canvas)
	{
		canvas.Clear(Properties.UseMica ? SKColors.Transparent : Surface.ClearColor());
	}

	internal virtual void DrawBackdrop(SKCanvas canvas)
	{
	}

	public void BeginWindowLoop()
	{
		FLogger.Log<FWindow>("Setting up OLE DragDrop...");
		SetupOLEDragDrop();
		FLogger.Log<FWindow>("Begin the loop!");
		FLogger.Log<FWindow>("");
		_isRunning = true;
		Loop.Begin(SetupWindowLogicOnBegin);
	}

	private void SetupOLEDragDrop()
	{
		FLogger.Log<FWindow>("Creating DragDropHandler");
		DropTarget = new DragDropHandler(this)
		{
			dragEnter = delegate(FDropData? x)
			{
				LogicDispatcher.Invoke(delegate
				{
					Callbacks.OnDragEnter?.Invoke(x);
				});
			},
			dragDrop = delegate(FDropData? x)
			{
				LogicDispatcher.Invoke(delegate
				{
					Callbacks.OnDragDrop?.Invoke(x);
				});
			},
			dragOver = delegate(FDropData? x)
			{
				LogicDispatcher.Invoke(delegate
				{
					Callbacks.OnDragOver?.Invoke(x);
				});
			},
			dragLeave = delegate
			{
				LogicDispatcher.Invoke(delegate
				{
					Callbacks.OnDragLeave?.Invoke();
				});
			}
		};
		FLogger.Log<FWindow>("Get COM interface pointer for the created DragDropHandler");
		nint comInterfaceForObject = Marshal.GetComInterfaceForObject(DropTarget, typeof(IDropTarget));
		FLogger.Log<FWindow>($"Registering DragDrop for window {hWnd} using drop target {comInterfaceForObject}");
		int num = DragDropRegistration.RegisterDragDrop(hWnd, comInterfaceForObject);
		if (num != 0)
		{
			Marshal.ThrowExceptionForHR(num);
		}
		FLogger.Log<FWindow>("Release COM interface pointer");
		Marshal.Release(comInterfaceForObject);
	}

	public void RequestFocus()
	{
		Win32APIs.SetForegroundWindow(hWnd);
	}

	internal void CallUpdate()
	{
		Update();
	}

	protected virtual void Update()
	{
	}

	public virtual void Dispose()
	{
		if (!_disposingOrDisposed)
		{
			FContext.isDisposingWindow = true;
			_disposingOrDisposed = true;
			Properties.IsWindowVisible = false;
			FLogger.Log<FWindow>("Early cleanup of window components...");
			EarlyCleanUp();
			Callbacks.OnWindowDestroy?.Invoke();
			FLogger.Log<FWindow>("Invoking window destruction...");
			WindowDispatcher.Invoke(delegate
			{
				FLogger.Log<FWindow>("Destroying window");
				Win32APIs.DestroyWindow(hWnd);
			});
			FenUI.activeInstances.Remove(this);
		}
	}

	internal void EarlyCleanUp()
	{
		FLogger.Log<FWindow>("Disposing surface...");
		Surface?.Dispose();
		FLogger.Log<FWindow>("Disposing parent window...");
		Properties?.DisposeParent();
		FLogger.Log<FWindow>("Disposing window keyboard input...");
		WindowKeyboardInput?.Dispose();
		WindowKeyboardInput = null;
	}

	internal void CleanUp()
	{
		FLogger.Log<FWindow>("Disposing of window components...");
		Shape?.Dispose();
		Shape = null;
		Procedure?.Dispose();
		Procedure = null;
		Loop?.Dispose();
		Loop = null;
		Properties?.Dispose();
		Properties = null;
		Surface = null;
		FLogger.Log<FWindow>("Disposing SkiaDirectCompositionContext...");
		SkiaDirectCompositionContext?.Dispose();
		SkiaDirectCompositionContext = null;
	}

	protected virtual void SetTargetRefreshrateToMonitorRefreshrate()
	{
		FLogger.Log<FWindow>($"Setting the target refresh rate of window {hWnd}...");
		DISPLAY_DEVICE lpDisplayDevice = default(DISPLAY_DEVICE);
		lpDisplayDevice.cb = Marshal.SizeOf(lpDisplayDevice);
		if (Win32APIs.EnumDisplayDevices(null, (uint)Shape.CurrentMonitorIndex, ref lpDisplayDevice, 0u))
		{
			DEVMODE devMode = new DEVMODE
			{
				dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE))
			};
			FLogger.Log<FWindow>("Window is on monitor " + lpDisplayDevice.DeviceName);
			if (Win32APIs.EnumDisplaySettings(lpDisplayDevice.DeviceName, -1, ref devMode))
			{
				TargetRefreshRate = (int)devMode.dmDisplayFrequency;
				FLogger.Log<FWindow>($"Set target refresh rate of window {hWnd} to {TargetRefreshRate}!");
			}
		}
	}

	internal virtual nint WindowHitTest(nint wParam, nint lParam)
	{
		return IntPtr.Zero;
	}

	public void Redraw()
	{
		_isDirty = true;
	}

	public void FullRedraw()
	{
		_isDirty = true;
		_fullRedraw = true;
		if (Surface.RootViewPane != null)
		{
			Surface.RootViewPane.RecursiveInvalidate(UIObject.Invalidation.All);
		}
	}
}
