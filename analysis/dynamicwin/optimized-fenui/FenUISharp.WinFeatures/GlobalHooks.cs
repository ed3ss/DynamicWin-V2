using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using FenUISharp.Mathematics;
using FenUISharp.Native;

namespace FenUISharp.WinFeatures;

public class GlobalHooks : IDisposable
{
	internal struct InputEvent
	{
		public int type;

		public int nCode;

		public nint wParam;

		public nint lParam;

		internal KBDLLHOOKSTRUCT keyInfo;
	}

	private static GlobalHooks? instance;

	private static nint _mouseHookID = IntPtr.Zero;

	private static nint _keyboardHookID = IntPtr.Zero;

	private LowLevelMouseProc _mouseProc = MouseHookCallback;

	private LowLevelKeyboardProc _keyboardProc = KeyboardHookCallback;

	private static float lastScrollDelta = 0f;

	private static Vector2 lastMousePosition = new Vector2(0f, 0f);

	private static Vector2 mousePosition = new Vector2(0f, 0f);

	private static float scrollDelta = 0f;

	private static Vector2 capturedMousePos;

	private static bool mouseUpdate;

	private AutoResetEvent signal = new AutoResetEvent(initialState: false);

	private ConcurrentQueue<InputEvent> queue = new ConcurrentQueue<InputEvent>();

	private bool[] keyFlags = new bool[256];

	private bool _isRegistered;

	public Action<float>? OnMouseScroll { get; set; }

	public Action<Vector2>? OnMouseMove { get; set; }

	public Action<Vector2>? OnMouseMoveDelta { get; set; }

	public Action<MouseInputCode>? OnMouseAction { get; set; }

	public Action<int>? OnKeyPressed { get; set; }

	public Action<int>? OnKeyTyped { get; set; }

	public Action<int>? OnKeyReleased { get; set; }

	public static Vector2 MousePosition => mousePosition;

	public static bool MouseDown { get; private set; }

	public GlobalHooks()
	{
		if (instance == null)
		{
			instance = this;
			Thread thread = new Thread((ThreadStart)delegate
			{
				while (instance != null)
				{
					signal.WaitOne();
					HandleConcurrentQueue();
				}
			});
			thread.IsBackground = true;
			thread.Start();
			return;
		}
		throw new InvalidOperationException("You can not create multiple instances of the GlobalHooks class.");
	}

	private void HandleConcurrentQueue()
	{
		InputEvent result;
		while (queue.TryDequeue(out result))
		{
			HandleEvent(result);
		}
	}

	private void HandleEvent(InputEvent evt)
	{
		int nCode = evt.nCode;
		nint wParam = evt.wParam;
		nint lParam = evt.lParam;
		KBDLLHOOKSTRUCT keyInfo = evt.keyInfo;
		if (nCode < 0)
		{
			return;
		}
		if (evt.type == 1 || mouseUpdate)
		{
			mouseUpdate = false;
			int num = (int)capturedMousePos.x;
			int num2 = (int)capturedMousePos.y;
			if (lastScrollDelta != scrollDelta)
			{
				OnMouseScroll?.Invoke(scrollDelta);
			}
			if (lastMousePosition != capturedMousePos)
			{
				mousePosition.x = num;
				mousePosition.y = num2;
				OnMouseMove?.Invoke(new Vector2(num, num2));
				OnMouseMoveDelta?.Invoke(new Vector2((float)num - lastMousePosition.x, (float)num2 - lastMousePosition.y));
				lastMousePosition.x = num;
				lastMousePosition.y = num2;
			}
			if (evt.type == 1)
			{
				Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
				switch ((MouseMessages)wParam)
				{
				case MouseMessages.WM_LBUTTONDOWN:
					OnMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Left, MouseInputState.Down));
					MouseDown = true;
					break;
				case MouseMessages.WM_LBUTTONUP:
					OnMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Left, MouseInputState.Up));
					MouseDown = false;
					break;
				case MouseMessages.WM_RBUTTONDOWN:
					OnMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Right, MouseInputState.Down));
					break;
				case MouseMessages.WM_RBUTTONUP:
					OnMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Right, MouseInputState.Up));
					break;
				case MouseMessages.WM_MBUTTONDOWN:
					OnMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Middle, MouseInputState.Down));
					break;
				case MouseMessages.WM_MBUTTONUP:
					OnMouseAction?.Invoke(new MouseInputCode(MouseInputButton.Middle, MouseInputState.Up));
					break;
				case (MouseMessages)515:
				case (MouseMessages)518:
					break;
				}
			}
		}
		else
		{
			if (evt.type != 2 || keyInfo.vkCode > 255)
			{
				return;
			}
			switch (wParam)
			{
			case 256:
				if (!keyFlags[keyInfo.vkCode])
				{
					keyFlags[keyInfo.vkCode] = true;
					OnKeyPressed?.Invoke(keyInfo.vkCode);
				}
				OnKeyTyped?.Invoke(keyInfo.vkCode);
				break;
			case 257:
				OnKeyReleased?.Invoke(keyInfo.vkCode);
				keyFlags[keyInfo.vkCode] = false;
				break;
			}
		}
	}

	private unsafe static nint MouseHookCallback(int nCode, nint wParam, nint lParam)
	{
		if (instance == null)
		{
			return Win32APIs.CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
		}
		mouseUpdate = true;
		int x = ((MSLLHOOKSTRUCT*)lParam)->pt.x;
		int y = ((MSLLHOOKSTRUCT*)lParam)->pt.y;
		switch (wParam)
		{
		case 522:
		{
			short num = (short)((((MSLLHOOKSTRUCT*)lParam)->mouseData >> 16) & 0xFFFF);
			scrollDelta += num;
			return Win32APIs.CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
		}
		case 512:
			capturedMousePos.x = x;
			capturedMousePos.y = y;
			return Win32APIs.CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
		default:
			instance.queue.Enqueue(new InputEvent
			{
				type = 1,
				lParam = lParam,
				wParam = wParam,
				nCode = nCode
			});
			instance.signal.Set();
			return Win32APIs.CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
		}
	}

	private unsafe static nint KeyboardHookCallback(int nCode, nint wParam, nint lParam)
	{
		if (instance == null)
		{
			return Win32APIs.CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
		}
		KBDLLHOOKSTRUCT keyInfo = *(KBDLLHOOKSTRUCT*)lParam;
		instance.queue.Enqueue(new InputEvent
		{
			type = 2,
			lParam = lParam,
			wParam = wParam,
			nCode = nCode,
			keyInfo = keyInfo
		});
		instance.signal.Set();
		return Win32APIs.CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
	}

	public void RegisterHooks()
	{
		if (!_isRegistered)
		{
			_isRegistered = true;
			nint moduleHandle = Win32APIs.GetModuleHandle(null);
			_keyboardHookID = Win32APIs.SetWindowsHookEx(13, _keyboardProc, moduleHandle, 0u);
			AppDomain.CurrentDomain.ProcessExit += UnregHooksExit;
		}
	}

	private void UnregHooksExit(object? sender, EventArgs e)
	{
		UnregisterHooks();
	}

	public void UnregisterHooks()
	{
		if (_isRegistered)
		{
			_isRegistered = false;
			if (_keyboardHookID != IntPtr.Zero)
			{
				Win32APIs.UnhookWindowsHookEx(_keyboardHookID);
				_keyboardHookID = IntPtr.Zero;
			}
			if (_mouseHookID != IntPtr.Zero)
			{
				Win32APIs.UnhookWindowsHookEx(_mouseHookID);
				_mouseHookID = IntPtr.Zero;
			}
			AppDomain.CurrentDomain.ProcessExit -= UnregHooksExit;
		}
	}

	public void Dispose()
	{
		UnregisterHooks();
		instance = null;
	}

	public static string GetKeyName(int vkCode)
	{
		if (!Enum.IsDefined(typeof(ConsoleKey), vkCode))
		{
			return $"VK_{vkCode}";
		}
		ConsoleKey consoleKey = (ConsoleKey)vkCode;
		return consoleKey.ToString();
	}
}
