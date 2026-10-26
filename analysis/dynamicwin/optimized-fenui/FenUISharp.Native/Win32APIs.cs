using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FenUISharp.Native;

internal static class Win32APIs
{
	internal delegate bool MonitorEnumDelegate(nint hMonitor, nint hdcMonitor, ref RECT lprcMonitor, nint dwData);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool UnhookWindowsHookEx(nint hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern bool SetWindowDisplayAffinity(nint hwnd, double dwAffinity);

	[DllImport("gdi32.dll")]
	internal static extern nint CreateRectRgn(int left, int top, int right, int bottom);

	[DllImport("gdi32.dll")]
	internal static extern nint CreatePolygonRgn(POINT[] pptl, int cPoints, int fnPolyFillMode);

	[DllImport("gdi32.dll")]
	internal static extern int CombineRgn(nint hrgnDest, nint hrgnSrc1, nint hrgnSrc2, int fnCombineMode);

	[DllImport("user32.dll")]
	internal static extern int SetWindowRgn(nint hWnd, nint hRgn, bool bRedraw);

	[DllImport("gdi32.dll", SetLastError = true)]
	internal static extern nint CreateDIBSection(nint hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out nint ppvBits, nint hSection, uint dwOffset);

	[DllImport("gdi32.dll")]
	internal static extern nint SelectObject(nint hdc, nint hgdiobj);

	[DllImport("gdi32.dll")]
	internal static extern bool DeleteObject(nint hObject);

	[DllImport("gdi32.dll")]
	internal static extern nint CreateCompatibleDC(nint hdc);

	[DllImport("gdi32.dll")]
	internal static extern bool DeleteDC(nint hdc);

	[DllImport("user32.dll")]
	internal static extern nint GetDC(nint hWnd);

	[DllImport("user32.dll")]
	internal static extern int ReleaseDC(nint hWnd, nint hDC);

	[DllImport("user32.dll")]
	internal static extern nint SetCapture(nint hWnd);

	[DllImport("user32.dll")]
	internal static extern nint ReleaseCapture(nint hWnd);

	[DllImport("user32.dll")]
	internal static extern uint GetDpiForWindow(nint hWnd);

	[DllImport("user32.dll")]
	internal static extern bool GetCursorPos(out POINT lpPoint);

	[DllImport("user32.dll")]
	internal static extern int GetSystemMetrics(int nIndex);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	internal static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFOEX lpmi);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern nint FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern nint FindWindowEx(nint hwndParent, nint hwndChildAfter, string lpszClass, string lpszWindow);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern nint SetParent(nint hWnd, nint hWndParent);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool DestroyWindow(nint hWnd);

	[DllImport("user32.dll")]
	internal static extern void PostQuitMessage(int nExitCode);

	[DllImport("user32.dll")]
	internal static extern nint SetCursor(nint hCursor);

	[DllImport("user32.dll")]
	internal static extern nint LoadCursor(nint hInstance, int lpCursorName);

	[DllImport("uxtheme.dll", EntryPoint = "#135")]
	internal static extern int ShouldSystemUseDarkMode();

	[DllImport("uxtheme.dll", EntryPoint = "#136")]
	internal static extern void AllowDarkModeForWindow(nint hWnd, bool allow);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern int GetWindowTextA(nint hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern int SetWindowTextA(nint hWnd, string lpString);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool ShowWindow(nint hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	internal static extern bool IsWindowVisible(nint hWnd);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern nint GetModuleHandle(string lpModuleName);

	[DllImport("user32.dll")]
	internal static extern bool SetForegroundWindow(nint hWnd);

	[DllImport("user32.dll")]
	internal static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	internal static extern nint GetWindowLongPtrA(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	internal static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

	[DllImport("user32.dll")]
	internal static extern nint SetWindowLongPtrA(nint hWnd, int nIndex, nint dwNewLong);

	[DllImport("user32.dll")]
	internal static extern bool SetLayeredWindowAttributes(nint hWnd, uint crKey, byte bAlpha, uint dwFlags);

	[DllImport("user32.dll")]
	internal static extern bool GetLayeredWindowAttributes(nint hWnd, out uint pcrKey, out byte pbAlpha, out uint pdwFlags);

	[DllImport("user32.dll")]
	internal static extern bool UpdateLayeredWindow(nint hWnd, nint hdcDst, ref POINT pptDst, ref SIZE psize, nint hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	internal static extern ushort RegisterClassExA(ref WNDCLASSEX lpwcx);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern nint CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	internal static extern nint CreateWindowExA(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

	[DllImport("dwmapi.dll")]
	internal static extern int DwmExtendFrameIntoClientArea(nint hWnd, ref MARGINS pMarInset);

	[DllImport("dwmapi.dll")]
	internal static extern int DwmSetWindowAttribute(nint hwnd, uint attr, ref int attrValue, int attrSize);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool SetWindowSize(nint hWnd, int width, int height);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool MoveWindow(nint hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

	[DllImport("user32.dll")]
	internal static extern nint DefWindowProcW(nint hWnd, uint msg, nint wParam, nint lParam);

	[DllImport("user32.dll")]
	internal static extern nint GetWindowLongPtrW(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	internal static extern nint SetWindowLongPtrW(nint hWnd, int nIndex, nint dwNewLong);

	[DllImport("user32.dll")]
	internal static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	internal static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll")]
	internal static extern bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	internal static extern bool PeekMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

	[DllImport("user32.dll")]
	internal static extern bool TranslateMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll")]
	internal static extern nint DispatchMessage([In] ref MSG lpmsg);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetClientRect(nint hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	internal static extern bool ClientToScreen(nint hWnd, ref POINT lpPoint);

	[DllImport("user32.dll")]
	internal static extern bool ScreenToClient(nint hWnd, ref POINT lpPoint);

	[DllImport("user32.dll")]
	internal static extern nint MonitorFromPoint(POINT pt, uint dwFlags);

	[DllImport("user32.dll")]
	internal static extern nint MonitorFromWindow(nint hwnd, uint dwFlags);

	[DllImport("user32.dll")]
	internal static extern nint LoadImage(nint hInstance, string lpszName, uint uType, int cx, int cy, uint fuLoad);

	[DllImport("shell32.dll")]
	internal static extern bool Shell_NotifyIconA(uint dwMessage, ref NOTIFYICONDATAA lpData);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern int SetCurrentProcessExplicitAppUserModelID(string AppID);

	[DllImport("user32.dll")]
	internal static extern bool SetProcessDpiAwarenessContext(nint dpiContext);

	[DllImport("user32.dll", CharSet = CharSet.Ansi)]
	internal static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

	[DllImport("user32.dll")]
	internal static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

	[DllImport("user32.dll")]
	internal static extern bool EnumDisplayMonitors(nint hdc, nint lprcClip, MonitorEnumDelegate lpfnEnum, nint dwData);

	internal static RECT GetMonitorRect(int monitorIndex)
	{
		RECT rect = default(RECT);
		int count = 0;
		EnumDisplayMonitors(lpfnEnum: delegate(nint hMonitor, nint hdcMonitor, ref RECT lprcMonitor, nint dwData)
		{
			if (count == monitorIndex)
			{
				rect = lprcMonitor;
				return false;
			}
			count++;
			return true;
		}, hdc: IntPtr.Zero, lprcClip: IntPtr.Zero, dwData: IntPtr.Zero);
		return rect;
	}
}
