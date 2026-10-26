using System;
using System.Globalization;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FNumericScroller : Button, IStateListener
{
	private float _value;

	private KeyBind increment;

	private KeyBind decrement;

	private FPopupPanel? activePopup;

	public float Value
	{
		get
		{
			return GetValue();
		}
		set
		{
			SetValue(value);
		}
	}

	public State<float> MinValue { get; init; }

	public State<float> MaxValue { get; init; }

	public State<float> Step { get; init; }

	public State<string> Suffix { get; set; }

	public State<string> FormatProvider { get; init; }

	public State<CultureInfo> Culture { get; init; }

	public FText Label { get; protected set; }

	public Action<float>? OnValueChanged { get; set; }

	public Action<float>? OnUserValueChanged { get; set; }

	public FNumericScroller(FText label, Func<string>? formatProvider = null, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(null, position, size ?? ((Func<Vector2>)(() => new Vector2(30f, 25f))))
	{
		Label = label;
		label.SetParent(this);
		increment = new KeyBind
		{
			VKCode = 39,
			AliasVKCodes = new int[1] { 38 },
			OnKeybindExecuted = delegate
			{
				OpenPopup(stayOpen: true);
				Increment();
			}
		};
		decrement = new KeyBind
		{
			VKCode = 37,
			AliasVKCodes = new int[1] { 40 },
			OnKeybindExecuted = delegate
			{
				OpenPopup(stayOpen: true);
				Decrement();
			}
		};
		SelectableComponent obj = selectableComponent;
		obj.OnSelectionGained = (Action)Delegate.Combine(obj.OnSelectionGained, (Action)delegate
		{
			FContext.GetKeyboardInputManager().RegisterKeybind(increment);
		});
		SelectableComponent obj2 = selectableComponent;
		obj2.OnSelectionLost = (Action)Delegate.Combine(obj2.OnSelectionLost, (Action)delegate
		{
			FContext.GetKeyboardInputManager().UnregisterKeybind(increment);
		});
		SelectableComponent obj3 = selectableComponent;
		obj3.OnSelectionGained = (Action)Delegate.Combine(obj3.OnSelectionGained, (Action)delegate
		{
			FContext.GetKeyboardInputManager().RegisterKeybind(decrement);
		});
		SelectableComponent obj4 = selectableComponent;
		obj4.OnSelectionLost = (Action)Delegate.Combine(obj4.OnSelectionLost, (Action)delegate
		{
			FContext.GetKeyboardInputManager().UnregisterKeybind(decrement);
		});
		base.RenderMaterial.Value = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.TransparentInteractableMaterial;
		label.Layout.StretchHorizontal.SetStaticState(value: true);
		label.Layout.StretchVertical.SetStaticState(value: true);
		MinValue = new State<float>(() => 0f, this, this);
		MaxValue = new State<float>(() => 1f, this, this);
		Step = new State<float>(() => 0.1f, this, this);
		Suffix = new State<string>(() => "", this, this);
		FormatProvider = new State<string>(formatProvider ?? ((Func<string>)(() => "")), this, this);
		Culture = new State<CultureInfo>(() => CultureInfo.CurrentCulture, this, this);
		UpdateText();
	}

	public override void OnInternalStateChanged<T>(T value)
	{
		base.OnInternalStateChanged(value);
		UpdateText();
	}

	private float GetValue()
	{
		return RMath.Clamp(_value, MinValue.CachedValue, MaxValue.CachedValue);
	}

	private void SetValue(float value)
	{
		_value = RMath.Clamp(MathF.Round(value * 10000f) / 10000f, MinValue.CachedValue, MaxValue.CachedValue);
		UpdateText();
		OnValueChanged?.Invoke(Value);
	}

	private void UpdateText()
	{
		string text = Value.ToString(FormatProvider.CachedValue, Culture.CachedValue);
		Label.Model = TextModelFactory.CopyBasicNew(text + Suffix.CachedValue, Label.Model);
		SKRect localBounds = base.Shape.LocalBounds;
		localBounds.Inflate(50f, 25f);
		SKRect boundingRect = Label.LayoutModel.GetBoundingRect(Label.Model, localBounds);
		base.Transform.Size.SetStaticState(new Vector2(boundingRect.Width + 6f * (boundingRect.Width / 30f), boundingRect.Height + 4f * (boundingRect.Width / 50f)));
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			Label.Invalidate(Invalidation.SurfaceDirty | Invalidation.LayoutDirty);
		}, 1uL);
		Invalidate(Invalidation.SurfaceDirty | Invalidation.LayoutDirty);
	}

	protected override void OnInteract()
	{
		base.OnInteract();
		OpenPopup();
	}

	public void OpenPopup(bool stayOpen = false)
	{
		if (activePopup == null)
		{
			CreatePopup();
		}
		if (stayOpen)
		{
			activePopup?.Show(() => base.Transform.LocalToGlobal(base.Transform.LocalPosition.CachedValue));
		}
		else
		{
			activePopup?.ToggleShow(() => base.Transform.LocalToGlobal(base.Transform.LocalPosition.CachedValue));
		}
	}

	private void CreatePopup()
	{
		activePopup = new FPopupPanel(() => new Vector2(70f, 30f), addLayout: false);
		activePopup.CornerRadius.SetStaticState(30f);
		activePopup.DistanceToTarget = (int)(base.Layout.ClampSize(base.Transform.Size.CachedValue).y / 2f - 5f);
		activePopup.InteractiveSurface.EnableMouseScrolling.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = activePopup.InteractiveSurface;
		interactiveSurface.OnMouseScroll = (Action<float>)Delegate.Combine(interactiveSurface.OnMouseScroll, new Action<float>(OnPopupScroll));
		new NumericScrollerPopupScrollDisplay(() => Value, () => Step.CachedValue, Decrement, Increment).SetParent(activePopup);
		FPopupPanel? fPopupPanel = activePopup;
		fPopupPanel.OnObjectDisposed = (Action)Delegate.Combine(fPopupPanel.OnObjectDisposed, (Action)delegate
		{
			InteractiveSurface interactiveSurface2 = activePopup.InteractiveSurface;
			interactiveSurface2.OnMouseScroll = (Action<float>)Delegate.Remove(interactiveSurface2.OnMouseScroll, new Action<float>(OnPopupScroll));
		});
	}

	private void Increment()
	{
		Value += Step.CachedValue;
		OnValueChanged?.Invoke(Value);
		OnUserValueChanged?.Invoke(Value);
	}

	private void Decrement()
	{
		Value -= Step.CachedValue;
		OnValueChanged?.Invoke(Value);
		OnUserValueChanged?.Invoke(Value);
	}

	private void OnPopupScroll(float x)
	{
		if (!(MathF.Abs(x) < 120f))
		{
			x = RMath.Clamp(x, -1f, 1f);
			Value += Step.CachedValue * x;
			OnValueChanged?.Invoke(Value);
			OnUserValueChanged?.Invoke(Value);
		}
	}
}
