using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Native;
using SkiaSharp;

namespace FenUISharp;

public class FWindowShape : IDisposable
{
	private Dictionary<object, Func<SKRect>> WindowRegion = new Dictionary<object, Func<SKRect>>();

	private bool _windowRegionsDirty;

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

	public float WindowDPIScale => GetDpiScale();

	public Vector2 Position
	{
		get
		{
			Win32APIs.GetWindowRect(Window.hWnd, out var lpRect);
			return new Vector2(lpRect.left, lpRect.top);
		}
		set
		{
			Win32APIs.MoveWindow(Window.hWnd, (int)value.x, (int)value.y, (int)Size.x, (int)Size.y, bRepaint: true);
		}
	}

	public Vector2 Size
	{
		get
		{
			Win32APIs.GetWindowRect(Window.hWnd, out var lpRect);
			return new Vector2(lpRect.Width, lpRect.Height);
		}
		set
		{
			Win32APIs.MoveWindow(Window.hWnd, (int)Position.x, (int)Position.y, (int)value.x, (int)value.y, bRepaint: true);
		}
	}

	public Vector2 ClientSize
	{
		get
		{
			Win32APIs.GetClientRect(Window.hWnd, out var lpRect);
			return new Vector2(lpRect.Width, lpRect.Height) / WindowDPIScale;
		}
	}

	public SKRect Bounds => SKRect.Create(Position.x, Position.y, ClientSize.x, ClientSize.y);

	public Vector2 MinSize { get; set; } = new Vector2(150f, 150f);

	public Vector2 MaxSize { get; set; } = new Vector2(float.MaxValue, float.MaxValue);

	public int CurrentMonitorIndex => CurrentMonitor();

	public string CurrentMonitorName => CurrentMonitorNameString();

	public nint CurrentMonitorHandle => Win32APIs.MonitorFromWindow(Window.hWnd, 2u);

	public FWindowShape(FWindow window)
	{
		this.window = new WeakReference<FWindow>(window);
	}

	public Vector2 ClientPointToGlobal(Vector2 clientPoint)
	{
		POINT lpPoint = new POINT
		{
			x = (int)clientPoint.x,
			y = (int)clientPoint.y
		};
		Win32APIs.ClientToScreen(Window.hWnd, ref lpPoint);
		return new Vector2(lpPoint.x, lpPoint.y);
	}

	public Vector2 GlobalPointToClient(Vector2 globalPoint)
	{
		POINT lpPoint = new POINT
		{
			x = (int)globalPoint.x,
			y = (int)globalPoint.y
		};
		Win32APIs.ClientToScreen(Window.hWnd, ref lpPoint);
		return new Vector2(lpPoint.x, lpPoint.y);
	}

	public int GetMonitorIndexFromPoint(Vector2 p)
	{
		nint item = Win32APIs.MonitorFromPoint(new POINT
		{
			x = (int)p.x,
			y = (int)p.y
		}, 2u);
		List<nint> monitors = new List<nint>();
		Win32APIs.EnumDisplayMonitors(lpfnEnum: delegate(nint hMonitor, nint hdcMonitor, ref RECT lprcMonitor, nint dwData)
		{
			monitors.Add(hMonitor);
			return true;
		}, hdc: IntPtr.Zero, lprcClip: IntPtr.Zero, dwData: IntPtr.Zero);
		return monitors.IndexOf(item);
	}

	private int CurrentMonitor()
	{
		nint item = Win32APIs.MonitorFromWindow(Window.hWnd, 2u);
		List<nint> monitors = new List<nint>();
		Win32APIs.EnumDisplayMonitors(lpfnEnum: delegate(nint hMonitor, nint hdcMonitor, ref RECT lprcMonitor, nint dwData)
		{
			monitors.Add(hMonitor);
			return true;
		}, hdc: IntPtr.Zero, lprcClip: IntPtr.Zero, dwData: IntPtr.Zero);
		return monitors.IndexOf(item);
	}

	private string CurrentMonitorNameString()
	{
		nint hMonitor = Win32APIs.MonitorFromWindow(Window.hWnd, 2u);
		MONITORINFOEX lpmi = default(MONITORINFOEX);
		lpmi.cbSize = Marshal.SizeOf(lpmi);
		Win32APIs.GetMonitorInfo(hMonitor, ref lpmi);
		return lpmi.szDevice;
	}

	private float GetDpiScale()
	{
		return (float)Win32APIs.GetDpiForWindow(Window.hWnd) / 96f;
	}

	public void AddOrUpdateWindowRegion(object owner, Func<SKRect> area)
	{
		if (WindowRegion.ContainsKey(owner))
		{
			SKRect sKRect = area();
			SKRect sKRect2 = WindowRegion[owner]();
			if (DifferentSize(sKRect2.Left, sKRect.Left) || DifferentSize(sKRect2.Right, sKRect.Right) || DifferentSize(sKRect2.Top, sKRect.Top) || DifferentSize(sKRect2.Bottom, sKRect.Bottom))
			{
				_windowRegionsDirty = true;
			}
			WindowRegion[owner] = area;
		}
		else
		{
			WindowRegion.Add(owner, area);
			_windowRegionsDirty = true;
		}
	}

	private bool DifferentSize(float a, float b)
	{
		return Math.Abs(a - b) > 0.5f;
	}

	public void DissolveWindowRegion(object owner)
	{
		if (WindowRegion.ContainsKey(owner))
		{
			WindowRegion.Remove(owner);
			_windowRegionsDirty = true;
		}
	}

	internal List<Func<SKRect>> GetWinRegion()
	{
		return WindowRegion.Values.ToList();
	}

	private void UpdateWindowRegions()
	{
		if (WindowRegion.Count == 0)
		{
			Win32APIs.SetWindowRgn(Window.hWnd, IntPtr.Zero, bRedraw: true);
			return;
		}
		FLogger.Log<FWindowShape>("Rebuild window regions");
		nint num = IntPtr.Zero;
		foreach (KeyValuePair<object, Func<SKRect>> item in WindowRegion.ToList())
		{
			SKRect source = item.Value();
			source = SKMatrix.CreateScale(Window.Shape.WindowDPIScale, Window.Shape.WindowDPIScale).MapRect(source);
			nint num2 = Win32APIs.CreateRectRgn((int)source.Left, (int)source.Top, (int)(source.Left + source.Width), (int)(source.Top + source.Height));
			if (num == IntPtr.Zero)
			{
				num = num2;
				continue;
			}
			Win32APIs.CombineRgn(num, num, num2, 2);
			Win32APIs.DeleteObject(num2);
		}
		if (num != IntPtr.Zero)
		{
			Win32APIs.SetWindowRgn(Window.hWnd, num, bRedraw: true);
		}
		else
		{
			Win32APIs.SetWindowRgn(Window.hWnd, IntPtr.Zero, bRedraw: true);
		}
		_windowRegionsDirty = false;
	}

	public void RebuildWindowArea()
	{
		_windowRegionsDirty = true;
	}

	internal void UpdateShape()
	{
		if (_windowRegionsDirty)
		{
			UpdateWindowRegions();
		}
	}

	public void Dispose()
	{
	}
}
