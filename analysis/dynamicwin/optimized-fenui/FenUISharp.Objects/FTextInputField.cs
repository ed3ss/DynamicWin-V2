using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FTextInputField : Button, IStateListener
{
	public enum TextInputFieldMode
	{
		Any,
		Alphabetic,
		Numeric,
		Alphanumeric,
		Password
	}

	public (Vector2 HorizontalPadding, Vector2 VerticalPadding) TextPadding = (HorizontalPadding: new Vector2(new Vector2(10f, 10f)), VerticalPadding: new Vector2(new Vector2(5f, 5f)));

	private int _caretIndex;

	private const char ARROW_LEFT = '%';

	private const char ARROW_RIGHT = '\'';

	private const char ARROW_UP = '&';

	private const char ARROW_DOWN = '(';

	private const char BACKSPACE = '\b';

	private const char CONTROL_BACKSPACE = '\u007f';

	private const char ESCAPE = '\u001b';

	private const char PASTE = '\u0016';

	private const char COPY = '\u0003';

	private const char CUT = '\u0018';

	private const char UNDO = '\u001a';

	private const char ENTER = '\r';

	private const char SELALL = '\u0001';

	private StringBuilder text = new StringBuilder();

	private int _selectionIndex;

	private float _lastTypedTimer;

	private float _typedTimerResetLength = 0.6f;

	private float viewPosition;

	private string lastValidInput = "";

	private const uint CF_TEXT = 1u;

	private const uint GMEM_MOVEABLE = 2u;

	public FText Label { get; init; }

	public string Text
	{
		get
		{
			return text.ToString();
		}
		set
		{
			text = new StringBuilder(value);
			UpdateText();
		}
	}

	private string _Text
	{
		get
		{
			if (TextInputMode == TextInputFieldMode.Password)
			{
				string text = "";
				for (int i = 0; i < Text.Length; i++)
				{
					text += "*";
				}
				return text;
			}
			return Text;
		}
	}

	public string PlaceholderText { get; set; } = "Type in text...";

	public TextInputFieldMode TextInputMode { get; set; }

	public int CaretIndex
	{
		get
		{
			return _caretIndex;
		}
		set
		{
			_caretIndex = RMath.Clamp(value, 0, text.Length);
			if (!FContext.GetKeyboardInputManager().IsShiftPressed)
			{
				_selectionIndex = _caretIndex;
			}
		}
	}

	public State<SKColor> TextSelectionColor { get; init; }

	public State<SKColor> CaretColor { get; init; }

	public State<float> CaretWidth { get; init; }

	public State<float> CaretHeight { get; init; }

	public State<float> CaretBlinkSpeed { get; init; }

	public State<int> MaxCharacters { get; init; }

	public Action<string>? OnTextChanged { get; set; }

	public Action<string>? OnEnter { get; set; }

	public FTextInputField(FText label)
	{
		Label = label;
		label.LayoutModel = new WrapLayout(label)
		{
			AllowLinebreakChar = false,
			AllowLinebreakOnOverflow = false,
			AllowEllipsis = false
		};
		label.Model = TextModelFactory.CreateBasic(text.ToString());
		label.SetParent(this);
		base.HoverMix.Value = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.HoveredMix.WithAlpha(75);
		base.RenderMaterial.Value = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.PanelMaterial();
		CaretBlinkSpeed = new State<float>(() => 2.5f, this, this);
		CaretWidth = new State<float>(() => 1f, this, this);
		CaretHeight = new State<float>(() => 18f, this, this);
		CaretColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.AddMix(new SKColor(50, 50, 50)), this, this);
		TextSelectionColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.MultiplyMix(new SKColor(180, 180, 180)), this, this);
		MaxCharacters = new State<int>(() => 100, this, this);
		label.Padding.SetStaticState(0);
		label.Layout.StretchVertical.SetStaticState(value: true);
		new CursorComponent(this, Cursor.IBEAM);
		base.Transform.Size.SetStaticState(new Vector2(300f, 30f));
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, new Action<MouseInputCode>(OnMouseAction));
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnDrag = (Action<Vector2>)Delegate.Combine(interactiveSurface2.OnDrag, new Action<Vector2>(OnDrag));
		InteractiveSurface interactiveSurface3 = base.InteractiveSurface;
		interactiveSurface3.OnDoubleMouseAction = (Action<MouseInputButton>)Delegate.Combine(interactiveSurface3.OnDoubleMouseAction, new Action<MouseInputButton>(OnDoubleMouseAction));
		KeyboardInputManager keyboardInputManager = FContext.GetKeyboardInputManager();
		keyboardInputManager.OnTextTyped = (Action<char>)Delegate.Combine(keyboardInputManager.OnTextTyped, new Action<char>(OnKeyTyped));
		KeyboardInputManager keyboardInputManager2 = FContext.GetKeyboardInputManager();
		keyboardInputManager2.OnKeyTyped = (Action<char>)Delegate.Combine(keyboardInputManager2.OnKeyTyped, new Action<char>(OnKeyPressed));
	}

	public override void OnInternalStateChanged<T>(T value)
	{
		base.OnInternalStateChanged(value);
		UpdateText();
	}

	private void OnDoubleMouseAction(MouseInputButton button)
	{
		if (button == MouseInputButton.Left)
		{
			CaretIndex--;
			while (CaretIndex > 0 && !char.IsLetterOrDigit(_Text[CaretIndex]))
			{
				CaretIndex--;
			}
			while (CaretIndex > 0 && char.IsLetterOrDigit(_Text[CaretIndex - 1]))
			{
				CaretIndex--;
			}
			int caretIndex = CaretIndex;
			CaretIndex++;
			while (CaretIndex < _Text.Length && !char.IsLetterOrDigit(_Text[CaretIndex - 1]))
			{
				CaretIndex++;
			}
			while (CaretIndex < _Text.Length && char.IsLetterOrDigit(_Text[CaretIndex]))
			{
				CaretIndex++;
			}
			_selectionIndex = caretIndex;
			UpdateText();
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		KeyboardInputManager keyboardInputManager = FContext.GetKeyboardInputManager();
		keyboardInputManager.OnTextTyped = (Action<char>)Delegate.Remove(keyboardInputManager.OnTextTyped, new Action<char>(OnKeyTyped));
		KeyboardInputManager keyboardInputManager2 = FContext.GetKeyboardInputManager();
		keyboardInputManager2.OnKeyTyped = (Action<char>)Delegate.Remove(keyboardInputManager2.OnKeyTyped, new Action<char>(OnKeyPressed));
	}

	private void OnKeyPressed(char obj)
	{
		if (!selectableComponent.IsSelected)
		{
			return;
		}
		switch (obj)
		{
		case '%':
			CaretIndex--;
			if (FContext.GetKeyboardInputManager().IsControlPressed)
			{
				while (CaretIndex > 0 && !char.IsLetterOrDigit(_Text[CaretIndex]))
				{
					CaretIndex--;
				}
				while (CaretIndex > 0 && char.IsLetterOrDigit(_Text[CaretIndex - 1]))
				{
					CaretIndex--;
				}
			}
			UpdateText();
			break;
		case '\'':
			CaretIndex++;
			if (FContext.GetKeyboardInputManager().IsControlPressed)
			{
				while (CaretIndex < _Text.Length && !char.IsLetterOrDigit(_Text[CaretIndex - 1]))
				{
					CaretIndex++;
				}
				while (CaretIndex < _Text.Length && char.IsLetterOrDigit(_Text[CaretIndex]))
				{
					CaretIndex++;
				}
			}
			UpdateText();
			break;
		case '&':
			CaretIndex = 0;
			UpdateText();
			break;
		case '(':
			CaretIndex = _Text.Length;
			UpdateText();
			break;
		case '\r':
			OnEnter?.Invoke(Text);
			break;
		}
		_lastTypedTimer = _typedTimerResetLength;
	}

	public override void LateBegin()
	{
		base.LateBegin();
		UpdateText();
	}

	protected override void Update()
	{
		base.Update();
		if (_lastTypedTimer > 0f)
		{
			_lastTypedTimer -= FContext.DeltaTime;
		}
		if (selectableComponent.IsSelected)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	private void OnDrag(Vector2 vector)
	{
		int selectionIndex = _selectionIndex;
		CaretIndex = MousePosToCaretIndex(FContext.GetCurrentWindow().ClientMousePosition);
		_selectionIndex = selectionIndex;
		_lastTypedTimer = _typedTimerResetLength;
		UpdateText();
	}

	private void OnMouseAction(MouseInputCode code)
	{
		if (code.button == MouseInputButton.Left && code.state == MouseInputState.Down)
		{
			CaretIndex = MousePosToCaretIndex(FContext.GetCurrentWindow().ClientMousePosition);
			_lastTypedTimer = _typedTimerResetLength;
			UpdateText();
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	private int MousePosToCaretIndex(Vector2 mousePos)
	{
		Vector2 point = Label.Transform.GlobalToDrawLocal(mousePos);
		List<Glyph> list = Label.LayoutModel.ProcessModel(Label.Model, base.Shape.LocalBounds);
		for (int i = 0; i < list.Count; i++)
		{
			SKRect bounds = list[i].Bounds;
			bounds.Inflate(1f, 2f);
			bounds.Offset((0f - bounds.Width) / 2f, 0f);
			if (RMath.ContainsPoint(bounds, point))
			{
				return i;
			}
		}
		if (point.x < base.Shape.LocalBounds.Left)
		{
			return 0;
		}
		return text.Length;
	}

	private void OnKeyTyped(char c)
	{
		if (!selectableComponent.IsSelected)
		{
			return;
		}
		int caretIndex = CaretIndex;
		switch (c)
		{
		case '\u0016':
		{
			RemoveSelectedText();
			TryGetClipboardText(out string result);
			text.Insert(CaretIndex, result);
			CaretIndex += result.Length;
			break;
		}
		case '\u0003':
			TrySetClipboardText(GetSelectedText());
			break;
		case '\u0018':
			TrySetClipboardText(GetSelectedText());
			RemoveSelectedText();
			break;
		case '\b':
			if (_Text.Length == 0)
			{
				return;
			}
			if (_selectionIndex == CaretIndex)
			{
				if (CaretIndex != 0)
				{
					text.Remove(RMath.Clamp(CaretIndex - 1, 0, text.Length - 1), 1);
					CaretIndex--;
				}
			}
			else
			{
				RemoveSelectedText();
			}
			break;
		case '\u007f':
			if (_Text.Length == 0)
			{
				return;
			}
			if (_selectionIndex == CaretIndex)
			{
				CaretIndex--;
				while (CaretIndex > 0 && !char.IsLetterOrDigit(_Text[CaretIndex]))
				{
					CaretIndex--;
				}
				while (CaretIndex > 0 && char.IsLetterOrDigit(_Text[CaretIndex - 1]))
				{
					CaretIndex--;
				}
				int length = caretIndex - CaretIndex;
				text.Remove(CaretIndex, length);
			}
			else
			{
				RemoveSelectedText();
			}
			break;
		case '\u0001':
			CaretIndex = _Text.Length;
			_selectionIndex = 0;
			break;
		default:
			if (c < ' ')
			{
				break;
			}
			RemoveSelectedText();
			if (_Text.Length >= MaxCharacters.CachedValue)
			{
				return;
			}
			text.Insert(CaretIndex, c);
			CaretIndex++;
			_selectionIndex = CaretIndex;
			if (GetWidthFromLeftToCaret() > GetVisibleBounds().Width)
			{
				float num;
				if (_Text.Length == 0 || _Text.Length - CaretIndex <= 0)
				{
					num = 0f;
				}
				else
				{
					TextModel model = CreateTextModel(_Text[CaretIndex - 1].ToString());
					num = Label.LayoutModel.GetBoundingRect(model, SKRect.Create(0f, 0f, 99999f, Label.Shape.LocalBounds.Height), 0f).Width;
				}
				viewPosition = MathF.Max(viewPosition + num, 0f);
			}
			break;
		}
		OnTextChanged?.Invoke(Text);
		UpdateText();
	}

	private string GetSelectedText()
	{
		if (_selectionIndex == CaretIndex)
		{
			return "";
		}
		int num = (int)MathF.Min(CaretIndex, _selectionIndex);
		int length = (int)MathF.Max(CaretIndex, _selectionIndex) - num;
		return _Text.Substring(num, length);
	}

	private void RemoveSelectedText()
	{
		if (_selectionIndex != CaretIndex)
		{
			int num = (int)MathF.Min(CaretIndex, _selectionIndex);
			int length = (int)MathF.Max(CaretIndex, _selectionIndex) - num;
			text = text.Remove(num, length);
			CaretIndex = (int)MathF.Min(CaretIndex, _selectionIndex);
			_selectionIndex = CaretIndex;
		}
	}

	private Vector2 GetCaretPos()
	{
		if (CaretIndex == 0 || _Text.Length == 0)
		{
			return base.Transform.GlobalToDrawLocal(Label.Transform.DrawLocalToGlobal(new Vector2(0f, Label.Shape.LocalBounds.MidY)));
		}
		string overrideText = _Text.Substring(0, CaretIndex);
		TextModel model = CreateTextModel(overrideText);
		float num = Label.LayoutModel.GetBoundingRect(model, SKRect.Create(0f, 0f, 99999f, Label.Shape.LocalBounds.Height)).Width;
		if (CaretIndex - 1 >= 0)
		{
			string overrideText2 = _Text.Substring(CaretIndex - 1, 1);
			TextModel model2 = CreateTextModel(overrideText2);
			num -= Label.LayoutModel.GetBoundingRect(model2, SKRect.Create(0f, 0f, 99999f, Label.Shape.LocalBounds.Height), 0f).Width / 2f;
		}
		num += 2f;
		return base.Transform.GlobalToDrawLocal(Label.Transform.DrawLocalToGlobal(new Vector2(num, Label.Shape.LocalBounds.MidY)));
	}

	private void UpdateText()
	{
		ValidateText();
		if (string.IsNullOrWhiteSpace(_Text))
		{
			Label.Model = CreateTextModel(PlaceholderText);
		}
		else
		{
			Label.Model = CreateTextModel();
		}
		SKRect boundingRect = Label.LayoutModel.GetBoundingRect(Label.Model, SKRect.Create(0f, 0f, 99999f, Label.Shape.LocalBounds.Height));
		Label.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		Label.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		Label.Layout.AbsoluteMarginVertical.SetStaticState(TextPadding.VerticalPadding);
		Label.Transform.Size.SetStaticState(new Vector2(MathF.Max(boundingRect.Width + TextPadding.HorizontalPadding.x + TextPadding.HorizontalPadding.y, base.Shape.LocalBounds.Width), 0f));
		SKRect visibleBounds = GetVisibleBounds();
		float widthFromRightToCaret = GetWidthFromRightToCaret();
		if (widthFromRightToCaret - visibleBounds.Width > 0f)
		{
			viewPosition = RMath.Clamp(MathF.Max(widthFromRightToCaret + 1f - visibleBounds.Width, viewPosition), 0f, float.MaxValue);
		}
		if (widthFromRightToCaret - viewPosition < 0f)
		{
			viewPosition = RMath.Clamp(MathF.Min(widthFromRightToCaret, viewPosition), float.MinValue, viewPosition);
		}
		Label.Transform.LocalPosition.SetStaticState(new Vector2(viewPosition + TextPadding.HorizontalPadding.x, 0f));
	}

	private void ValidateText()
	{
		switch (TextInputMode)
		{
		case TextInputFieldMode.Alphabetic:
			text = new StringBuilder(Regex.Replace(text.ToString(), "[^A-Za-z]", ""));
			break;
		case TextInputFieldMode.Numeric:
		{
			float result;
			if (string.IsNullOrWhiteSpace(text.ToString()))
			{
				text = new StringBuilder("0");
				CaretIndex = 1;
			}
			else if (!float.TryParse(text.ToString(), out result))
			{
				text = new StringBuilder(lastValidInput);
				if (!float.TryParse(text.ToString(), out var _))
				{
					text = new StringBuilder(Regex.Replace(text.ToString(), "[^0-9.,]", ""));
				}
			}
			break;
		}
		case TextInputFieldMode.Alphanumeric:
			text = new StringBuilder(Regex.Replace(text.ToString(), "[^A-Za-z0-9,.]", ""));
			break;
		}
		lastValidInput = _Text;
		if (text.Length > MaxCharacters.CachedValue)
		{
			text = new StringBuilder(text.ToString().Substring(0, MaxCharacters.CachedValue));
		}
		int selectionIndex = _selectionIndex;
		CaretIndex = _caretIndex;
		_selectionIndex = RMath.Clamp(selectionIndex, 0, _Text.Length);
	}

	private TextModel CreateTextModel(string? overrideText = null)
	{
		TextStyle textStyle = new TextStyle
		{
			FontSize = 14f,
			Color = ((overrideText == null) ? ((Func<SKColor>)(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnBackground)) : ((Func<SKColor>)(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(75)))),
			Weight = SKFontStyleWeight.Normal,
			Slant = SKFontStyleSlant.Upright,
			Underlined = false
		};
		TextStyle style = new TextStyle(textStyle)
		{
			BackgroundColor = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.WithAlpha(150)
		};
		TextAlign align = new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.Middle
		};
		List<TextSpan> list = new List<TextSpan>();
		if (overrideText == null)
		{
			int num = (int)MathF.Min(CaretIndex, _selectionIndex);
			int num2 = (int)MathF.Max(CaretIndex, _selectionIndex);
			int length = num2 - num;
			list.Add(new TextSpan(_Text.Substring(0, num), textStyle));
			list.Add(new TextSpan(_Text.Substring(num, length), style));
			list.Add(new TextSpan(_Text.Substring(num2, _Text.Length - num2), textStyle));
		}
		else
		{
			list.Add(new TextSpan(overrideText, textStyle));
		}
		return new TextModel(list, align, FenUISharp.Objects.Text.Model.FTypeface.Default);
	}

	private SKRect GetVisibleBounds()
	{
		SKRect localBounds = base.Shape.LocalBounds;
		return new SKRect(localBounds.Left + TextPadding.HorizontalPadding.x, localBounds.Top + TextPadding.VerticalPadding.x, localBounds.Right - TextPadding.HorizontalPadding.y, localBounds.Bottom - TextPadding.VerticalPadding.y);
	}

	private float GetWidthFromRightToCaret()
	{
		if (_Text.Length == 0 || _Text.Length - CaretIndex <= 0)
		{
			return 0f;
		}
		string overrideText = _Text.Substring(CaretIndex, _Text.Length - CaretIndex);
		TextModel model = CreateTextModel(overrideText);
		return Label.LayoutModel.GetBoundingRect(model, SKRect.Create(0f, 0f, 99999f, Label.Shape.LocalBounds.Height), 0f).Width;
	}

	private float GetWidthFromLeftToCaret()
	{
		if (_Text.Length == 0 || CaretIndex == 0)
		{
			return 0f;
		}
		string overrideText = _Text.Substring(0, CaretIndex);
		TextModel model = CreateTextModel(overrideText);
		return Label.LayoutModel.GetBoundingRect(model, SKRect.Create(0f, 0f, 99999f, Label.Shape.LocalBounds.Height), 0f).Width;
	}

	public override void DrawChildren(SKCanvas? canvas)
	{
		canvas?.ClipRect(GetVisibleBounds(), SKClipOperation.Intersect, antialias: true);
		base.DrawChildren(canvas);
	}

	public override void AfterRender(SKCanvas canvas)
	{
		base.AfterRender(canvas);
		if (!selectableComponent.IsSelected)
		{
			return;
		}
		Vector2 caretPos = GetCaretPos();
		SKRect rect = SKRect.Create(caretPos.x - CaretWidth.CachedValue / 2f, caretPos.y - CaretHeight.CachedValue / 2f, CaretWidth.CachedValue, CaretHeight.CachedValue);
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Color = CaretColor.CachedValue.WithAlpha((byte)RMath.Clamp((_lastTypedTimer > 0f) ? 255f : ((MathF.Sin(FContext.Time * (float)Math.PI * CaretBlinkSpeed.CachedValue) + 1f) * 255f), 0f, 255f));
		using SKRoundRect rect2 = new SKRoundRect(rect, CaretWidth.CachedValue / 2f);
		canvas.DrawRoundRect(rect2, sKPaint);
	}

	[DllImport("user32.dll")]
	private static extern bool OpenClipboard(nint hWndNewOwner);

	[DllImport("user32.dll")]
	private static extern bool CloseClipboard();

	[DllImport("user32.dll")]
	private static extern nint GetClipboardData(uint uFormat);

	[DllImport("user32.dll")]
	private static extern bool IsClipboardFormatAvailable(uint format);

	[DllImport("kernel32.dll")]
	private static extern nint GlobalLock(nint hMem);

	[DllImport("kernel32.dll")]
	private static extern bool GlobalUnlock(nint hMem);

	[DllImport("user32.dll")]
	private static extern bool EmptyClipboard();

	[DllImport("user32.dll")]
	private static extern nint SetClipboardData(uint uFormat, nint hMem);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint GlobalAlloc(uint uFlags, nuint dwBytes);

	private static string CleanInput(string strIn)
	{
		return strIn;
	}

	private static bool TryGetClipboardText(out string result)
	{
		result = null;
		if (!IsClipboardFormatAvailable(1u))
		{
			return false;
		}
		if (!OpenClipboard(IntPtr.Zero))
		{
			return false;
		}
		nint clipboardData = GetClipboardData(1u);
		if (clipboardData == IntPtr.Zero)
		{
			CloseClipboard();
			return false;
		}
		nint num = GlobalLock(clipboardData);
		if (num == IntPtr.Zero)
		{
			CloseClipboard();
			return false;
		}
		result = CleanInput(Marshal.PtrToStringAnsi(num) ?? "");
		GlobalUnlock(clipboardData);
		CloseClipboard();
		return result != null;
	}

	private static bool TrySetClipboardText(string text)
	{
		if (!OpenClipboard(IntPtr.Zero))
		{
			return false;
		}
		if (!EmptyClipboard())
		{
			CloseClipboard();
			return false;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(text + "\0");
		nint num = GlobalAlloc(2u, (nuint)bytes.Length);
		if (num == IntPtr.Zero)
		{
			CloseClipboard();
			return false;
		}
		nint num2 = GlobalLock(num);
		if (num2 == IntPtr.Zero)
		{
			CloseClipboard();
			return false;
		}
		Marshal.Copy(bytes, 0, num2, bytes.Length);
		GlobalUnlock(num);
		if (SetClipboardData(1u, num) == IntPtr.Zero)
		{
			CloseClipboard();
			return false;
		}
		CloseClipboard();
		return true;
	}
}
