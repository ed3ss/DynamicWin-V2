using System;
using System.Runtime.InteropServices;
using FenUISharp.Logging;
using FenUISharp.Native;

namespace FenUISharp;

public class FWindowProperties : IDisposable
{
	private bool _useMica;

	private MicaBackdropType _micaBackdropType = MicaBackdropType.MainWindow;

	private bool _alwaysOnTop;

	internal nint _hiddenOwnerWindowHandle = IntPtr.Zero;

	private bool _taskbarIconVisible;

	private bool _sysDarkMode;

	internal bool _isFocused;

	private NOTIFYICONDATAA _trayIconData;

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

	public bool HideWindowOnClose { get; set; }
	public bool ShowWithoutActivation { get; set; }

	public bool IsWindowVisible
	{
		get
		{
			return Win32APIs.IsWindowVisible(Window.hWnd);
		}
		set
		{
			ShowWindow(value ? (Window is FOverlayWindow || ShowWithoutActivation ? (ShowWindowCommand)8 : ShowWindowCommand.SW_SHOW) : ShowWindowCommand.SW_HIDE);
		}
	}

	public bool AllowResize
	{
		set
		{
			SetWindowStyle(262144, value);
		}
	}

	public bool HasSystemMenu
	{
		set
		{
			SetWindowStyle(524288, value);
		}
	}

	public bool AllowMaximize
	{
		set
		{
			SetWindowStyle(65536, value);
		}
	}

	public bool AllowMinimize
	{
		set
		{
			SetWindowStyle(131072, value);
		}
	}

	public bool UseMica
	{
		get
		{
			return _useMica;
		}
		set
		{
			UpdateMica(value, MicaBackdropType);
		}
	}

	public MicaBackdropType MicaBackdropType
	{
		get
		{
			return _micaBackdropType;
		}
		set
		{
			UpdateMica(_useMica, value);
		}
	}

	public bool AlwaysOnTop
	{
		get
		{
			return _alwaysOnTop;
		}
		set
		{
			_alwaysOnTop = value;
			Win32APIs.SetWindowPos(Window.hWnd, _alwaysOnTop ? (-1) : (-2), 0, 0, 0, 0, 0x213u);
		}
	}

	public bool ExcludeFromAeroPeek
	{
		set
		{
			int attrValue = (value ? 1 : 0);
			Win32APIs.DwmSetWindowAttribute(Window.hWnd, 12u, ref attrValue, 4);
		}
	}

	public bool TransparentIgnoreClientInteractions
	{
		set
		{
			UpdateTransparentIgnoreClientInteractions(value);
		}
	}

	public bool VisibleInTaskbar
	{
		get
		{
			return _taskbarIconVisible;
		}
		set
		{
			HideTaskbarIcon(value);
		}
	}

	public bool UseSystemDarkMode
	{
		get
		{
			return _sysDarkMode;
		}
		set
		{
			_sysDarkMode = value;
			UpdateSysDarkmode();
		}
	}

	public bool ExcludeFromDesktopDuplication
	{
		set
		{
			UpdateExcludeFromDesktopDuplication(value);
		}
	}

	public bool IsWindowFocused => _isFocused;

	public FWindowProperties(FWindow window)
	{
		this.window = new WeakReference<FWindow>(window);
	}

	public void ShowWindow(ShowWindowCommand showWindowCommand)
	{
		FLogger.Log<FWindowProperties>("Setting window visibility: " + showWindowCommand);
		Win32APIs.ShowWindow(Window.hWnd, (int)showWindowCommand);
	}

	public void SetWindowStyle(int style, bool enabled)
	{
		int windowLong = Win32APIs.GetWindowLong(Window.hWnd, -16);
		windowLong = ((!enabled) ? (windowLong & ~style) : (windowLong | style));
		Win32APIs.SetWindowLong(Window.hWnd, -16, windowLong);
	}

	public void OverrideWindowStyle(int style)
	{
		Win32APIs.SetWindowLong(Window.hWnd, -16, style);
	}

	public void HideTaskbarIcon(bool isVisible)
	{
		_taskbarIconVisible = false;
		if (isVisible)
		{
			Win32APIs.SetWindowLongPtr(Window.hWnd, -8, IntPtr.Zero);
			if (_hiddenOwnerWindowHandle != IntPtr.Zero)
			{
				Win32APIs.DestroyWindow(_hiddenOwnerWindowHandle);
				_hiddenOwnerWindowHandle = IntPtr.Zero;
			}
			return;
		}
		try
		{
			_hiddenOwnerWindowHandle = Win32APIs.CreateWindowEx(128, "STATIC", "", 13565952, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, Win32APIs.GetModuleHandle(null), IntPtr.Zero);
			Win32APIs.ShowWindow(_hiddenOwnerWindowHandle, 0);
			if (Window.hWnd != IntPtr.Zero)
			{
				Win32APIs.SetWindowLongPtr(Window.hWnd, -8, _hiddenOwnerWindowHandle);
			}
		}
		catch (Exception ex)
		{
			FLogger.Error("Exception while creating invisible parent " + ex.Message);
		}
	}

	internal virtual void UpdateSysDarkmode()
	{
		Window._isDirty = true;
		int attrValue = (_sysDarkMode ? 1 : 0);
		if (Win32APIs.DwmSetWindowAttribute(Window.hWnd, 20u, ref attrValue, 4) != 0)
		{
			Win32APIs.DwmSetWindowAttribute(Window.hWnd, 19u, ref attrValue, 4);
		}
		if (Win32APIs.ShouldSystemUseDarkMode() == 1)
		{
			Win32APIs.AllowDarkModeForWindow(Window.hWnd, allow: true);
		}
	}

	internal void UpdateExcludeFromDesktopDuplication(bool exclude)
	{
		Win32APIs.SetWindowDisplayAffinity(Window.hWnd, exclude ? 17 : 0);
	}

	internal void UpdateMica(bool useMica, MicaBackdropType backdropType = MicaBackdropType.MainWindow)
	{
		_useMica = useMica;
		_micaBackdropType = backdropType;
		int attrValue = (int)(useMica ? backdropType : MicaBackdropType.None);
		Win32APIs.DwmSetWindowAttribute(Window.hWnd, 38u, ref attrValue, Marshal.SizeOf<int>());
	}

	internal void UpdateTransparentIgnoreClientInteractions(bool isSet)
	{
		int windowLong = Win32APIs.GetWindowLong(Window.hWnd, -20);
		windowLong = ((!isSet) ? (windowLong & -524289) : (windowLong | 0x80000));
		Win32APIs.SetWindowLong(Window.hWnd, -20, windowLong);
		Window.Shape.Position = Window.Shape.Position;
	}

	public void CreateTrayIcon(string iconPath, string tooltip)
	{
		if (_trayIconData.hWnd == Window.hWnd)
		{
			throw new InvalidOperationException($"Another tray icon has already been added for the window {Window.hWnd}!");
		}
		FLogger.Log<FWindowProperties>($"Adding tray icon for window {Window.hWnd}");
		_trayIconData = new NOTIFYICONDATAA
		{
			cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATAA)),
			hWnd = Window.hWnd,
			uID = 1u,
			uFlags = 7u,
			uCallbackMessage = 1025u,
			szTip = tooltip
		};
		FLogger.Log<FWindowProperties>("Turning image to pointer");
		nint num = Win32APIs.LoadImage(IntPtr.Zero, iconPath, 1u, 0, 0, 16u);
		_trayIconData.hIcon = num;
		FLogger.Log<FWindowProperties>($"Setting Shell Notify Icon to image with pointer {num}");
		Win32APIs.Shell_NotifyIconA(0u, ref _trayIconData);
	}

	public void SetWindowIcon(string? iconPath, string? smallIconPath = null)
	{
		if (iconPath == null)
		{
			FLogger.Log<FWindowProperties>($"Removing window icon for window {Window.hWnd}");
			Win32APIs.SendMessage(Window.hWnd, 128, 1, IntPtr.Zero);
			Win32APIs.SendMessage(Window.hWnd, 128, 0, IntPtr.Zero);
			return;
		}
		FLogger.Log<FWindowProperties>("Loading big icon from file...");
		nint num = Win32APIs.LoadImage(IntPtr.Zero, iconPath, 1u, 0, 0, 16u);
		nint num2 = num;
		if (smallIconPath != null)
		{
			FLogger.Log<FWindowProperties>("Loading small icon from file...");
			num2 = Win32APIs.LoadImage(IntPtr.Zero, smallIconPath, 1u, 0, 0, 16u);
		}
		if (num != IntPtr.Zero && num2 != IntPtr.Zero)
		{
			FLogger.Log<FWindowProperties>($"Setting big icon ({num}) and small icon ({num2})...");
			Win32APIs.SendMessage(Window.hWnd, 128, 1, num);
			Win32APIs.SendMessage(Window.hWnd, 128, 0, num2);
			return;
		}
		throw new Exception("Failed to load icon.");
	}

	public void Dispose()
	{
	}

	public void DisposeParent()
	{
		Win32APIs.SetWindowLongPtr(Window.hWnd, -8, IntPtr.Zero);
		if (_hiddenOwnerWindowHandle != IntPtr.Zero)
		{
			Win32APIs.DestroyWindow(_hiddenOwnerWindowHandle);
		}
		_hiddenOwnerWindowHandle = IntPtr.Zero;
	}
}

