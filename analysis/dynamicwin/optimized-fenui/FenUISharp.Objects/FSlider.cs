using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FSlider : UIObject, IStateListener
{
	protected float _value;

	private List<float>? _snappingHotspots;

	private float _snappingInterval;

	private float _knobPosition;

	private KeyBind increment;

	private KeyBind decrement;

	protected SelectableComponent selectableComponent;

	public float Value
	{
		get
		{
			return RMath.Remap(_value, 0f, 1f, MinValue.CachedValue, MaxValue.CachedValue);
		}
		set
		{
			float num = RMath.Remap(Math.Clamp(value, MinValue.CachedValue, MaxValue.CachedValue), MinValue.CachedValue, MaxValue.CachedValue, 0f, 1f);
			if (!RMath.Approximately(_value, num))
			{
				_value = SnapIfNeeded(num);
				OnValueChanged?.Invoke(Value);
				Invalidate(Invalidation.SurfaceDirty);
			}
		}
	}

	public State<float> MaxValue { get; init; }

	public State<float> MinValue { get; init; }

	public float SnappingInterval
	{
		get
		{
			return _snappingInterval;
		}
		set
		{
			_snappingInterval = value;
			UpdateHostpots();
		}
	}

	public int MaxSnapIndicators { get; set; } = 50;

	public List<float> ExtraHotspots { get; set; } = new List<float>();

	public State<SKColor> BarFill { get; init; }

	public State<SKColor> Bar { get; init; }

	public State<SKColor> BarBorder { get; init; }

	public State<SKColor> KnobFill { get; init; }

	public State<SKColor> KnobBorder { get; init; }

	public State<SKColor> KnobShadow { get; init; }

	public State<SKColor> SnappingHandle { get; init; }

	public Spring KnobPositionSpring { get; set; }

	public Vector2 KnobSize { get; set; } = new Vector2(20f, 20f);

	public float KnobCornerRadius { get; set; } = 20f;

	public float KnobBorderSize { get; set; } = 1f;

	public Vector2 SnappingHandleSize { get; set; } = new Vector2(3f, 10f);

	public float SnappingHandleCornerRadius { get; set; } = 2f;

	public float BarBorderSize { get; set; } = 1f;

	public float BarCornerRadius { get; set; } = 5f;

	public float BarHeight
	{
		get
		{
			return base.Layout.ClampSize(base.Transform.Size.CachedValue).y;
		}
		set
		{
			base.Transform.Size.SetStaticState(new Vector2(base.Layout.ClampSize(base.Transform.Size.CachedValue).x, value));
		}
	}

	public bool DisplayFill { get; set; } = true;

	public bool ClampKnob { get; set; }

	public Action<float>? OnValueChanged { get; set; }

	public Action<float>? OnUserValueChanged { get; set; }

	public FSlider(Func<Vector2>? position = null, float width = 100f)
		: base(position, () => new Vector2(width, 3f))
	{
		FSlider fSlider = this;
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, new Action<MouseInputCode>(MouseAction));
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnDrag = (Action<Vector2>)Delegate.Combine(interactiveSurface2.OnDrag, new Action<Vector2>(OnDragSlider));
		increment = new KeyBind
		{
			VKCode = 39,
			OnKeybindExecuted = Increment
		};
		decrement = new KeyBind
		{
			VKCode = 37,
			OnKeybindExecuted = Decrement
		};
		base.RenderMaterial.Value = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.InteractableMaterial();
		selectableComponent = new SelectableComponent(this, base.InteractiveSurface);
		SelectableComponent obj = selectableComponent;
		obj.OnSelectionGained = (Action)Delegate.Combine(obj.OnSelectionGained, (Action)delegate
		{
			FContext.GetKeyboardInputManager().RegisterKeybind(fSlider.increment);
		});
		SelectableComponent obj2 = selectableComponent;
		obj2.OnSelectionLost = (Action)Delegate.Combine(obj2.OnSelectionLost, (Action)delegate
		{
			FContext.GetKeyboardInputManager().UnregisterKeybind(fSlider.increment);
		});
		SelectableComponent obj3 = selectableComponent;
		obj3.OnSelectionGained = (Action)Delegate.Combine(obj3.OnSelectionGained, (Action)delegate
		{
			FContext.GetKeyboardInputManager().RegisterKeybind(fSlider.decrement);
		});
		SelectableComponent obj4 = selectableComponent;
		obj4.OnSelectionLost = (Action)Delegate.Combine(obj4.OnSelectionLost, (Action)delegate
		{
			FContext.GetKeyboardInputManager().UnregisterKeybind(fSlider.decrement);
		});
		MinValue = new State<float>(() => 0f, this, this);
		MaxValue = new State<float>(() => 1f, this, this);
		base.ImageEffects.Opacity.SetResponsiveState(() => (!fSlider.InteractiveSurface.IgnoreInteractions.CachedValue) ? 1f : 0.35f, 10u);
		BarFill = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary, this, this);
		Bar = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Secondary, this, this);
		BarBorder = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSecondary.WithAlpha(35), this, this);
		KnobFill = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Secondary, this, this);
		KnobBorder = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSecondary.WithAlpha(45), this, this);
		KnobShadow = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Shadow, this, this);
		SnappingHandle = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.SecondaryVariant, this, this);
		base.Padding.SetStaticState(15);
		base.InteractiveSurface.ExtendInteractionRadius.SetStaticState(10);
		KnobPositionSpring = new Spring(5f, 1.4f);
	}

	private void Increment()
	{
		float value = GetValue();
		Value += ((SnappingInterval != 0f) ? SnappingInterval : ((MaxValue.CachedValue - MinValue.CachedValue) / 10f));
		float value2 = GetValue();
		if (value != value2)
		{
			OnValueChanged?.Invoke(Value);
			OnUserValueChanged?.Invoke(Value);
		}
	}

	private void Decrement()
	{
		float value = GetValue();
		Value -= ((SnappingInterval != 0f) ? SnappingInterval : ((MaxValue.CachedValue - MinValue.CachedValue) / 10f));
		float value2 = GetValue();
		if (value != value2)
		{
			OnValueChanged?.Invoke(Value);
			OnUserValueChanged?.Invoke(Value);
		}
	}

	private void MouseAction(MouseInputCode code)
	{
		if (code.button == MouseInputButton.Left && code.state == MouseInputState.Down)
		{
			SetToPoint(FContext.GetCurrentWindow().ClientMousePosition);
		}
	}

	public override void OnInternalStateChanged<T>(T value)
	{
		base.OnInternalStateChanged(value);
		UpdateHostpots();
		Invalidate(Invalidation.SurfaceDirty);
	}

	public void SilentSetValue(float value)
	{
		Value = value;
		Invalidate(Invalidation.SurfaceDirty);
		OnValueChanged?.Invoke(Value);
		float num = RMath.Remap(GetValue(), MinValue.CachedValue, MaxValue.CachedValue, 0f, 1f);
		KnobPositionSpring.ResetVector(new Vector2(num * 100f, 0f));
	}

	public float GetValue(float? c = null)
	{
		if (SnappingInterval != 0f)
		{
			List<float>? obj = _snappingHotspots ?? new List<float>();
			obj.AddRange(ExtraHotspots);
			return obj.OrderBy((float x) => Math.Abs(x - RMath.Remap(c ?? _value, 0f, 1f, MinValue.CachedValue, MaxValue.CachedValue))).First();
		}
		return RMath.Remap(c ?? _value, 0f, 1f, MinValue.CachedValue, MaxValue.CachedValue);
	}

	private float SnapIfNeeded(float normalized)
	{
		if (SnappingInterval <= 0f)
		{
			return normalized;
		}
		float actualValue = RMath.Remap(normalized, 0f, 1f, MinValue.CachedValue, MaxValue.CachedValue);
		List<float> list = new List<float>(_snappingHotspots ?? new List<float>());
		list.AddRange(ExtraHotspots);
		if (list.Count == 0)
		{
			return normalized;
		}
		return RMath.Remap(list.OrderBy((float x) => Math.Abs(x - actualValue)).First(), MinValue.CachedValue, MaxValue.CachedValue, 0f, 1f);
	}

	protected override void Update()
	{
		base.Update();
		float knobPosition = _knobPosition;
		_knobPosition = KnobPositionSpring.Update(FContext.DeltaTime, new Vector2(_value * 100f, 0f)).x / 100f;
		if (Math.Round(knobPosition * 100f) != Math.Round(_knobPosition * 100f))
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	private void OnDragSlider(Vector2 mousePos)
	{
		SetToPoint(FContext.GetCurrentWindow().ClientMousePosition);
	}

	private void SetToPoint(Vector2 pos)
	{
		float value = GetValue();
		Value = Math.Clamp(RMath.Remap(pos.x, base.Shape.GlobalBounds.Left + (float)base.Padding.CachedValue, base.Shape.GlobalBounds.Right - (float)base.Padding.CachedValue, MinValue.CachedValue, MaxValue.CachedValue), MinValue.CachedValue, MaxValue.CachedValue);
		float value2 = GetValue();
		if (value != value2)
		{
			OnValueChanged?.Invoke(Value);
			OnUserValueChanged?.Invoke(Value);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		float knobPosition = _knobPosition;
		using (GetRenderPaint())
		{
			SKRect localBounds = base.Shape.LocalBounds;
			localBounds.Offset(0.5f, 0.5f);
			RenderBackground(canvas, localBounds);
			SKRect rect = SKRect.Create(base.Shape.LocalBounds.Left, base.Shape.LocalBounds.MidY - base.Layout.ClampSize(base.Transform.Size.CachedValue).y / 2f, base.Shape.LocalBounds.Width * knobPosition, base.Layout.ClampSize(base.Transform.Size.CachedValue).y);
			rect.Offset(0.5f, 0.5f);
			RenderFilledBackground(canvas, rect);
			List<float> hotspots = new List<float>();
			if (_snappingHotspots != null)
			{
				_snappingHotspots.ForEach(delegate(float x)
				{
					hotspots.Add(x);
				});
			}
			if (ExtraHotspots != null)
			{
				ExtraHotspots.ForEach(delegate(float x)
				{
					hotspots.Add(x);
				});
			}
			if (hotspots != null)
			{
				RenderHotspots(canvas, hotspots);
			}
			SKRect knobRect = SKRect.Create(RMath.Lerp(base.Shape.LocalBounds.Left, base.Shape.LocalBounds.Right, knobPosition) - KnobSize.x / 2f, base.Shape.LocalBounds.MidY - KnobSize.y / 2f, KnobSize.x, KnobSize.y);
			if (ClampKnob)
			{
				knobRect = SKRect.Create(RMath.Clamp(RMath.Lerp(base.Shape.LocalBounds.Left, base.Shape.LocalBounds.Right, knobPosition) - KnobSize.x / 2f, base.Shape.LocalBounds.Left, base.Shape.LocalBounds.Right - knobRect.Width), base.Shape.LocalBounds.MidY - KnobSize.y / 2f, KnobSize.x, KnobSize.y);
			}
			knobRect.Offset(0.5f, 0.5f);
			RenderKnob(canvas, knobRect);
		}
	}

	protected virtual void RenderKnob(SKCanvas canvas, SKRect knobRect)
	{
		using SKPaint paint = GetRenderPaint();
		using SKRoundRect rect = new SKRoundRect(knobRect, KnobCornerRadius);
		base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object>
		{
			["BaseColor"] = (Func<SKColor>)(() => KnobFill.CachedValue),
			["BorderColor"] = (Func<SKColor>)(() => KnobBorder.CachedValue)
		}).DrawWithMaterial(canvas, rect, this, paint);
	}

	protected virtual void RenderBackground(SKCanvas canvas, SKRect rect)
	{
		using SKPaint paint = GetRenderPaint();
		using SKRoundRect rect2 = new SKRoundRect(rect, BarCornerRadius);
		base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object>
		{
			["BaseColor"] = (Func<SKColor>)(() => Bar.CachedValue),
			["BorderColor"] = (Func<SKColor>)(() => BarBorder.CachedValue),
			["DropShadowRadius"] = (Func<float>)(() => 0f)
		}).DrawWithMaterial(canvas, rect2, this, paint);
	}

	protected virtual void RenderFilledBackground(SKCanvas canvas, SKRect rect)
	{
		if (!DisplayFill)
		{
			return;
		}
		using SKPaint paint = GetRenderPaint();
		using SKRoundRect rect2 = new SKRoundRect(rect, BarCornerRadius);
		base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object>
		{
			["BaseColor"] = (Func<SKColor>)(() => BarFill.CachedValue),
			["BorderColor"] = (Func<SKColor>)(() => SKColors.Transparent),
			["DropShadowRadius"] = (Func<float>)(() => 0f)
		}).DrawWithMaterial(canvas, rect2, this, paint);
	}

	protected virtual void RenderHotspots(SKCanvas canvas, List<float> hotspots)
	{
		if (hotspots.Count > MaxSnapIndicators)
		{
			return;
		}
		using SKPaint paint = GetRenderPaint();
		canvas.Save();
		SKRect localBounds = base.Shape.LocalBounds;
		localBounds.Inflate(2f, SnappingHandleSize.y);
		canvas.ClipRect(localBounds, SKClipOperation.Intersect, antialias: true);
		for (int i = 0; i < hotspots.Count; i++)
		{
			float value = RMath.Remap(hotspots[i], MinValue.CachedValue, MaxValue.CachedValue, 0f, 1f);
			using SKRoundRect rect = new SKRoundRect(SKRect.Create(RMath.Lerp(base.Shape.LocalBounds.Left, base.Shape.LocalBounds.Right, value) - SnappingHandleSize.x / 2f, base.Shape.LocalBounds.MidY - SnappingHandleSize.y / 2f, SnappingHandleSize.x, SnappingHandleSize.y), SnappingHandleCornerRadius);
			base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object>
			{
				["BaseColor"] = (Func<SKColor>)(() => (!DisplayFill || !(_value > value)) ? SnappingHandle.CachedValue : BarFill.CachedValue),
				["BorderColor"] = (Func<SKColor>)(() => SKColors.Transparent)
			}).DrawWithMaterial(canvas, rect, this, paint);
		}
		canvas.Restore();
	}

	private void UpdateHostpots()
	{
		if (SnappingInterval != 0f)
		{
			_snappingHotspots = GetSnapHotSpots(MinValue.CachedValue, MaxValue.CachedValue, SnappingInterval);
		}
	}

	private List<float> GetSnapHotSpots(float min, float max, float interval)
	{
		List<float> list = new List<float>();
		for (float num = min; num < max; num += interval)
		{
			list.Add(num);
		}
		if (!list.Contains(min))
		{
			list.Add(min);
		}
		if (!list.Contains(max))
		{
			list.Add(max);
		}
		list.Sort();
		return list;
	}
}
