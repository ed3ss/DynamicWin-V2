using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.WinFeatures;

namespace FenUISharp;

public class KeyboardInputManager : IDisposable
{
	private const char VK_CONTROL = '\u0011';

	private const char VK_SHIFT = '\u0010';

	private const char VK_LMENU = '¤';

	private const char VK_RMENU = '¥';

	private List<KeyBind> keybinds;

	public bool IsControlPressed { get; private set; }

	public bool IsShiftPressed { get; private set; }

	public bool IsAltPressed { get; private set; }

	public Action<char>? OnKeyTyped { get; set; }

	public Action<char>? OnKeyPressed { get; set; }

	public Action<char>? OnKeyReleased { get; set; }

	public Action<char>? OnTextTyped { get; set; }

	private FWindow _window { get; init; }

	public KeyboardInputManager(FWindow window)
	{
		_window = window;
		keybinds = new List<KeyBind>();
		GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
		globalHooks.OnKeyTyped = (Action<int>)Delegate.Combine(globalHooks.OnKeyTyped, new Action<int>(KeyTyped));
		GlobalHooks globalHooks2 = WindowFeatures.GlobalHooks;
		globalHooks2.OnKeyPressed = (Action<int>)Delegate.Combine(globalHooks2.OnKeyPressed, new Action<int>(KeyPressed));
		GlobalHooks globalHooks3 = WindowFeatures.GlobalHooks;
		globalHooks3.OnKeyReleased = (Action<int>)Delegate.Combine(globalHooks3.OnKeyReleased, new Action<int>(KeyReleased));
		FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
		callbacks.OnKeyboardInputTextReceived = (Action<char>)Delegate.Combine(callbacks.OnKeyboardInputTextReceived, new Action<char>(TextTyped));
	}

	private void KeyTyped(int obj)
	{
		_window.LogicDispatcher.Invoke(delegate
		{
			OnKeyTyped?.Invoke((char)obj);
		});
	}

	private void TextTyped(char c)
	{
		_window.LogicDispatcher.Invoke(delegate
		{
			OnTextTyped?.Invoke(c);
		});
	}

	private void KeyReleased(int vkCode)
	{
		vkCode = NormalizeVKCode(vkCode);
		char c = (char)vkCode;
		switch (c)
		{
		case '\u0011':
			IsControlPressed = false;
			break;
		case '\u0010':
			IsShiftPressed = false;
			break;
		case '\u0012':
		case '¥':
			IsAltPressed = false;
			break;
		}
		_window.LogicDispatcher.Invoke(delegate
		{
			OnKeyReleased?.Invoke(c);
		});
	}

	private void KeyPressed(int vkCode)
	{
		vkCode = NormalizeVKCode(vkCode);
		char c = (char)vkCode;
		switch (c)
		{
		case '\u0011':
			IsControlPressed = true;
			break;
		case '\u0010':
			IsShiftPressed = true;
			break;
		case '\u0012':
		case '¥':
			IsAltPressed = true;
			break;
		}
		foreach (KeyBind keybind in keybinds.ToList())
		{
			if ((vkCode == keybind.VKCode || (keybind.AliasVKCodes != null && keybind.AliasVKCodes.Contains(vkCode))) && (!keybind.Flags.HasFlag(KeyBindFlags.Control) || IsControlPressed) && (!keybind.Flags.HasFlag(KeyBindFlags.Alt) || IsAltPressed) && (!keybind.Flags.HasFlag(KeyBindFlags.Shift) || IsShiftPressed) && (keybind.Flags.HasFlag(KeyBindFlags.Control) || !IsControlPressed) && (keybind.Flags.HasFlag(KeyBindFlags.Alt) || !IsAltPressed) && (keybind.Flags.HasFlag(KeyBindFlags.Shift) || !IsShiftPressed) && (keybind.IgnoreFocus || _window.Properties.IsWindowFocused))
			{
				_window.LogicDispatcher.Invoke(delegate
				{
					keybind.OnKeybindExecuted?.Invoke();
				});
			}
		}
		if (_window.Properties.IsWindowFocused)
		{
			_window.LogicDispatcher.Invoke(delegate
			{
				OnKeyPressed?.Invoke(c);
			});
		}
	}

	private int NormalizeVKCode(int vkCode)
	{
		if (vkCode == 160 || vkCode == 161)
		{
			vkCode = 16;
		}
		if (vkCode == 162 || vkCode == 163)
		{
			vkCode = 17;
		}
		if (vkCode == 164 || vkCode == 165)
		{
			vkCode = 18;
		}
		return vkCode;
	}

	public void RegisterKeybind(KeyBind keybind)
	{
		keybinds.Add(keybind);
	}

	public void UnregisterKeybind(KeyBind keybind)
	{
		keybinds.Remove(keybind);
	}

	public void Dispose()
	{
		GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
		globalHooks.OnKeyPressed = (Action<int>)Delegate.Remove(globalHooks.OnKeyPressed, new Action<int>(KeyPressed));
		GlobalHooks globalHooks2 = WindowFeatures.GlobalHooks;
		globalHooks2.OnKeyReleased = (Action<int>)Delegate.Remove(globalHooks2.OnKeyReleased, new Action<int>(KeyReleased));
	}
}
