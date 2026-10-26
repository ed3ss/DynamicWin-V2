using System;
using System.Runtime.InteropServices;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Native;
using FenUISharp.Objects;

namespace FenUISharp;

public class FWindowProcedure : IDisposable
{
	private Vector2 oldSize;

	internal volatile bool _isSizing;

	internal volatile bool _isSizeMoving;

	internal volatile bool _isNotInitialResize;

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

	public Func<bool>? _isRunning { get; set; }

	public FWindowProcedure(FWindow window)
	{
		this.window = new WeakReference<FWindow>(window);
	}

	public nint WindowsProcedure(nint hWnd, uint msg, nint wParam, nint lParam)
	{
		switch (msg)
		{
		case 512u:
		{
			int num4 = (short)((IntPtr)lParam).ToInt32();
			int num5 = ((IntPtr)lParam).ToInt32() >> 16;
			Window.ClientMousePosition = new Vector2(num4, num5) / Window.Shape.WindowDPIScale;
			Window.Callbacks.OnMouseMove?.Invoke(Window.ClientMousePosition);
			return IntPtr.Zero;
		}
		case 522u:
		{
			int num2 = (short)((((IntPtr)wParam).ToInt64() >> 16) & 0xFFFF);
			Window.Callbacks.OnMouseScroll?.Invoke(num2);
			return IntPtr.Zero;
		}
		case 258u:
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.OnKeyboardInputTextReceived?.Invoke((char)wParam);
			});
			return IntPtr.Zero;
		case 537u:
			FLogger.Log<FWindowProcedure>($"WM_DEVICECHANGE: Device change for window {Window.hWnd}");
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.OnDevicesChanged?.Invoke();
			});
			return IntPtr.Zero;
		case 36u:
		{
			MINMAXINFO structure = Marshal.PtrToStructure<MINMAXINFO>(lParam);
			float windowDPIScale = Window.Shape.WindowDPIScale;
			structure.ptMinTrackSize = new POINT
			{
				x = (int)(Window.Shape.MinSize.x * windowDPIScale),
				y = (int)(Window.Shape.MinSize.y * windowDPIScale)
			};
			structure.ptMaxTrackSize = new POINT
			{
				x = (int)(Window.Shape.MaxSize.x * windowDPIScale),
				y = (int)(Window.Shape.MaxSize.y * windowDPIScale)
			};
			Marshal.StructureToPtr(structure, lParam, fDeleteOld: true);
			return IntPtr.Zero;
		}
		case 1025u:
			if ((int)lParam == 517)
			{
				Window.LogicDispatcher.Invoke(delegate
				{
					Window.Callbacks.TrayMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Right, MouseInputState.Up));
				});
			}
			if ((int)lParam == 514)
			{
				Window.LogicDispatcher.Invoke(delegate
				{
					Window.Callbacks.TrayMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Left, MouseInputState.Up));
				});
			}
			return IntPtr.Zero;
		case 26u:
		case 279u:
			FLogger.Log<FWindowProcedure>($"WM_INITMENUPOPUP/WM_SETTINGCHANGE: Window {Window.hWnd}");
			Window.Properties.UpdateSysDarkmode();
			return IntPtr.Zero;
		case 736u:
			Window.Callbacks.DPIChanged?.Invoke();
			return IntPtr.Zero;
		case 5u:
		case 532u:
			_isSizing = true;
			WindowRzd(Window.Shape.Size);
			return IntPtr.Zero;
		case 3u:
		case 534u:
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.OnWindowMove?.Invoke(Window.Shape.Position);
			});
			return IntPtr.Zero;
		case 561u:
			FLogger.Log<FWindowProcedure>("");
			FLogger.Log<FWindowProcedure>($"WM_ENTERSIZEMOVE: Window {Window.hWnd}");
			_isSizeMoving = true;
			return IntPtr.Zero;
		case 562u:
			FLogger.Log<FWindowProcedure>("");
			FLogger.Log<FWindowProcedure>($"WM_EXITSIZEMOVE: Window {Window.hWnd}");
			if (_isSizing)
			{
				Vector2 size = Window.Shape.Size;
				FLogger.Log<FWindowProcedure>($"Stopped resizing: {size}");
				Window.LogicDispatcher.Invoke(delegate
				{
					Window.Callbacks.OnWindowEndResize?.Invoke(size);
				});
				Window.FullRedraw();
			}
			else
			{
				FLogger.Log<FWindowProcedure>($"Stopped moving: {Window.Shape.Position}");
				Window.LogicDispatcher.Invoke(delegate
				{
					Window.Callbacks.OnWindowEndMove?.Invoke(Window.Shape.Position);
				});
			}
			Window.FullRedraw();
			_isSizeMoving = false;
			_isSizing = false;
			return IntPtr.Zero;
		case 8u:
			Window._isDirty = true;
			Window.Properties._isFocused = false;
			Window.LogicDispatcher?.Invoke(delegate
			{
				Window.Callbacks.OnFocusLost?.Invoke();
			});
			return IntPtr.Zero;
		case 7u:
			Window._isDirty = true;
			Window.Properties._isFocused = true;
			Window.LogicDispatcher?.Invoke(delegate
			{
				Window.Callbacks.OnFocusGained?.Invoke();
			});
			return IntPtr.Zero;
		case 256u:
			return IntPtr.Zero;
		case 513u:
			Win32APIs.SetCapture(Window.hWnd);
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.ClientMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Left, MouseInputState.Down));
			});
			return IntPtr.Zero;
		case 516u:
			Win32APIs.SetCapture(Window.hWnd);
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.ClientMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Right, MouseInputState.Down));
			});
			return IntPtr.Zero;
		case 519u:
			Win32APIs.SetCapture(Window.hWnd);
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.ClientMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Middle, MouseInputState.Down));
			});
			return IntPtr.Zero;
		case 514u:
			Win32APIs.ReleaseCapture(Window.hWnd);
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.ClientMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Left, MouseInputState.Up));
			});
			return IntPtr.Zero;
		case 517u:
			Win32APIs.ReleaseCapture(Window.hWnd);
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.ClientMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Right, MouseInputState.Up));
			});
			return IntPtr.Zero;
		case 520u:
			Win32APIs.ReleaseCapture(Window.hWnd);
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.ClientMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Middle, MouseInputState.Up));
			});
			return IntPtr.Zero;
		case 675u:
			Window.Callbacks.OnMouseLeft?.Invoke();
			return IntPtr.Zero;
		case 32u:
		{
			int num3 = ((IntPtr)lParam).ToInt32() & 0xFFFF;
			if (num3 == 10 || num3 == 11 || num3 == 12 || num3 == 13 || num3 == 14 || num3 == 15 || num3 == 16 || num3 == 17)
			{
				return Win32APIs.DefWindowProcW(hWnd, msg, wParam, lParam);
			}
			Win32APIs.SetCursor(Win32APIs.LoadCursor(IntPtr.Zero, (int)Window.ActiveCursor.Value));
			return IntPtr.Zero;
		}
		case 130u:
		{
			FLogger.Log<FWindowProcedure>($"WM_NCDESTROY: {Window.hWnd}");
			FLogger.Log<FWindow>("Cleaning up GCHandle...");
			nint windowLongPtrA = Win32APIs.GetWindowLongPtrA(hWnd, -21);
			if (windowLongPtrA != IntPtr.Zero)
			{
				GCHandle.FromIntPtr(windowLongPtrA).Free();
				FLogger.Log<FWindow>($"Resetting userdata (-21) for window {hWnd}...");
				Win32APIs.SetWindowLongPtrA(hWnd, -21, IntPtr.Zero);
			}
			FLogger.Log<FWindow>("Disabling running flag");
			Window._isRunning = false;
			FLogger.Log<FWindowProcedure>("Cleaning up all resources...");
			Window.CleanUp();
			FLogger.Log<FWindowProcedure>($"Fully destroyed window {hWnd}. Bye!");
			FLogger.Log<FWindowProcedure>("");
			break;
		}
		case 132u:
		{
			nint num = Window.WindowHitTest(wParam, lParam);
			if (num != IntPtr.Zero)
			{
				return num;
			}
			return Win32APIs.DefWindowProcW(hWnd, msg, wParam, lParam);
		}
		case 16u:
			FLogger.Log<FWindowProcedure>($"WM_CLOSE: Window with handle {Window.hWnd} closed");
			Window.Callbacks.OnWindowClose?.Invoke();
			if (!Window.Properties.HideWindowOnClose)
			{
				FLogger.Log<FWindowProcedure>($"Disposing window {Window.hWnd}...");
				Window.Dispose();
			}
			else
			{
				Window.Properties.IsWindowVisible = false;
			}
			return IntPtr.Zero;
		case 2u:
			if (FenUI.IsMainThread)
			{
				FLogger.Log<FWindowProcedure>("WM_DESTROY: Posting quit message 0");
				Win32APIs.PostQuitMessage(0);
			}
			return IntPtr.Zero;
		}
		return Win32APIs.DefWindowProcW(hWnd, msg, wParam, lParam);
	}

	protected virtual void WindowRzd(Vector2 size)
	{
		if (oldSize != size)
		{
			Window.LogicDispatcher.Invoke(delegate
			{
				Window.Callbacks.OnWindowResize?.Invoke(size);
			});
		}
		oldSize = size;
		Window._isDirty = true;
		Window.LogicDispatcher.Invoke(delegate
		{
			Window.Redraw();
		});
		Window.LogicDispatcher.Invoke(delegate
		{
			Window.Surface.RootViewPane?.RecursiveInvalidate(UIObject.Invalidation.LayoutDirty);
		});
	}

	public void Dispose()
	{
	}
}
