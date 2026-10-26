using System;
using FenUISharp.Objects;
using FenUISharp.Themes;

namespace FenUISharp;

public static class FContext
{
	[ThreadStatic]
	internal static bool isDisposingWindow;

	[ThreadStatic]
	private static FWindow? CurrentWindow;

	[ThreadStatic]
	private static Dispatcher? CurrentDispatcher;

	[ThreadStatic]
	private static ModelViewPane? RootViewPane;

	[ThreadStatic]
	internal static DateTime frameStartTime;

	public static float Time => CurrentWindow?.Time.Time ?? 0f;

	public static float DeltaTime => CurrentWindow?.Time.DeltaTime ?? 0f;

	public static bool IsDisposingWindow => isDisposingWindow;

	public static double CurrentFrameTimeMillis => (DateTime.Now - frameStartTime).TotalMilliseconds;

	public static FWindow GetCurrentWindow()
	{
		return CurrentWindow ?? throw new Exception("GetCurrentWindow() cannot be called in an invalid FenUI context");
	}

	public static Dispatcher GetCurrentDispatcher()
	{
		return CurrentDispatcher ?? throw new Exception("GetCurrentDispatcher() cannot be called in an invalid FenUI context");
	}

	public static ModelViewPane? GetRootViewPane()
	{
		return RootViewPane;
	}

	public static KeyboardInputManager GetKeyboardInputManager()
	{
		return CurrentWindow?.WindowKeyboardInput ?? throw new Exception("GetKeyboardInputManager() cannot be called in an invalid FenUI context");
	}

	public static ThemeManager GetCurrentThemeManager()
	{
		return CurrentWindow?.WindowThemeManager ?? throw new Exception("GetCurrentWindow() cannot be called in an invalid FenUI context");
	}

	internal static void WithWindow(FWindow window)
	{
		CurrentWindow = window;
		CurrentDispatcher = window.LogicDispatcher;
	}

	internal static void WithRootViewPane(ModelViewPane? view)
	{
		RootViewPane = view;
	}

	public static bool IsValidContext()
	{
		if (CurrentWindow != null && CurrentDispatcher != null)
		{
			return !CurrentWindow._disposingOrDisposed;
		}
		return false;
	}
}
