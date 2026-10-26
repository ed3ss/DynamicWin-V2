using System;
using FenUISharp.Mathematics;
using FenUISharp.Native;

namespace FenUISharp;

public class FOverlayWindow : FTransparentWindow
{
	private int _activeDisplay;

	public int ActiveDisplayIndex
	{
		get
		{
			return _activeDisplay;
		}
		set
		{
			_activeDisplay = value;
			UpdateWindowMetrics(_activeDisplay);
		}
	}

	public FOverlayWindow(string title, string className, int monitorIndex = 0)
		: base(title, className, new Vector2(0f, 0f), new Vector2(0f, 0f))
	{
		UpdateWindowMetrics(monitorIndex);
		base.Properties.ExcludeFromAeroPeek = true;
		base.Properties.VisibleInTaskbar = false;
	}

	public void UpdateWindowMetrics(int activeMonitorDisplay = 0)
	{
		RECT monitorRect = Win32APIs.GetMonitorRect(activeMonitorDisplay);
		int left = monitorRect.left;
		int top = monitorRect.top;
		int cx = monitorRect.right - monitorRect.left;
		int cy = monitorRect.bottom - monitorRect.top;
		Win32APIs.SetWindowPos(base.hWnd, IntPtr.Zero, left, top, cx, cy, 20u);
		FullRedraw();
		base.Shape.RebuildWindowArea();
	}
}
