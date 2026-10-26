using System;
using FenUISharp.Mathematics;
using FenUISharp.Native;

namespace FenUISharp;

public class FNativeWindow : FWindow
{
	public FNativeWindow(string title, string className, Vector2? position = null, Vector2? size = null)
		: base(title, className, position, size)
	{
	}

	protected override nint CreateWindow(WNDCLASSEX wndClass, Vector2 position, Vector2 size)
	{
		if (position == new Vector2(-1f, -1f))
		{
			RECT monitorRect = Win32APIs.GetMonitorRect(0);
			position = new Vector2((float)((monitorRect.right - monitorRect.left) / 2) - size.x / 2f, (float)((monitorRect.bottom - monitorRect.top) / 2) - size.y / 2f);
		}
		return Win32APIs.CreateWindowExA(2097152, base.WindowClass, null, 13565952, (int)position.x, (int)position.y, (int)size.x, (int)size.y, IntPtr.Zero, IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);
	}
}
