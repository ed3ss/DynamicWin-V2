using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FenUISharp;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Native;

namespace DynamicWin;

public class WindowsWorkingAreaHelper
{
	public struct APPBARDATA
	{
		public int cbSize;

		public nint hWnd;

		public uint uCallbackMessage;

		public uint uEdge;

		public RECT rc;

		public int lParam;
	}

	private const int ABM_NEW = 0;

	private const int ABM_REMOVE = 1;

	private const int ABM_QUERYPOS = 2;

	private const int ABM_SETPOS = 3;

	private const int ABE_BOTTOM = 3;

	private const int ABE_TOP = 1;

	[DllImport("shell32.dll", SetLastError = true)]
	private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

	public void RegisterAppBar(nint hwnd)
	{
		APPBARDATA pData = new APPBARDATA
		{
			cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
			hWnd = hwnd,
			uEdge = 1u
		};
		int setting = Settings.GetSetting("general.active-display", 0);
		if (setting >= Screen.AllScreens.Length)
		{
			FLogger.Error("Screen index out of bounds. Clamping to next possible value");
		}
		Rectangle bounds = Screen.AllScreens[RMath.Clamp(setting, 0, Screen.AllScreens.Length)].Bounds;
		FLogger.Log($"Registering AppBar to screen {setting} at bounds {bounds}");
		pData.rc.left = bounds.Left;
		pData.rc.top = bounds.Top;
		pData.rc.right = bounds.Left + bounds.Width;
		pData.rc.bottom = bounds.Top + (int)(29f * FContext.GetCurrentWindow().Shape.WindowDPIScale);
		SHAppBarMessage(0u, ref pData);
		SHAppBarMessage(3u, ref pData);
	}

	public void UnregisterAppBar(nint hwnd)
	{
		APPBARDATA pData = new APPBARDATA
		{
			cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
			hWnd = hwnd
		};
		SHAppBarMessage(1u, ref pData);
	}

	public WindowsWorkingAreaHelper()
	{
		UpdateWorkingArea();
		Settings.SetListenerForSettingChange("general.shorten-work-area", UpdateWorkingArea);
		Settings.SetListenerForSettingChange("general.active-display", delegate
		{
			UnregisterAppBar(DynamicWin.MainWindow.hWnd);
			RegisterAppBar(DynamicWin.MainWindow.hWnd);
		});
	}

	private void UpdateWorkingArea()
	{
		if (Settings.GetSetting("general.shorten-work-area", defaultValue: false))
		{
			RegisterAppBar(DynamicWin.MainWindow.hWnd);
		}
		else
		{
			UnregisterAppBar(DynamicWin.MainWindow.hWnd);
		}
	}
}
