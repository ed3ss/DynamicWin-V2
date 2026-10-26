using System;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public class AnimatorComponent : BehaviorComponent
{
	private float _timePassed;

	private Func<float, float> easing;

	private Func<float, float> inverseEasing;

	private float startValue;

	private float targetValue;

	private float currentValue;

	public float Duration { get; set; } = 1f;

	public float Time
	{
		get
		{
			if (Duration <= 0f)
			{
				return 1f;
			}
			float arg = Math.Clamp(_timePassed / Duration, 0f, 1f);
			if (!Inverse)
			{
				return easing(arg);
			}
			return inverseEasing(arg);
		}
	}

	public float UneasedTime
	{
		get
		{
			if (Duration <= 0f)
			{
				return 1f;
			}
			return Math.Clamp(_timePassed / Duration, 0f, 1f);
		}
	}

	public float TimePassed => _timePassed;

	public Action<float>? OnValueUpdate { get; set; }

	public Action? OnComplete { get; set; }

	public bool IsRunning { get; private set; }

	public bool Inverse { get; set; }

	public AnimatorComponent(UIObject owner, Func<float, float> easing, Func<float, float>? inverseEasing = null)
		: base(owner)
	{
		this.easing = easing;
		if (inverseEasing == null)
		{
			this.inverseEasing = easing;
		}
		else
		{
			this.inverseEasing = inverseEasing;
		}
		currentValue = ((!Inverse) ? 0f : 1f);
		startValue = currentValue;
		targetValue = (Inverse ? 0f : 1f);
	}

	public void Start()
	{
		startValue = currentValue;
		targetValue = (Inverse ? 0f : 1f);
		_timePassed = 0f;
		IsRunning = true;
	}

	public void Restart()
	{
		startValue = (Inverse ? 1f : 0f);
		targetValue = (Inverse ? 0f : 1f);
		_timePassed = 0f;
		IsRunning = true;
	}

	public void Break()
	{
		_timePassed = 0f;
		IsRunning = false;
	}

	public override void HandleEvent(BehaviorEventType type, object? data = null)
	{
		base.HandleEvent(type, data);
		if (type == BehaviorEventType.BeforeUpdate && IsRunning)
		{
			_timePassed += FContext.DeltaTime;
			float arg = Math.Clamp(_timePassed / Duration, 0f, 1f);
			float num = (Inverse ? inverseEasing(arg) : easing(arg));
			currentValue = startValue + (targetValue - startValue) * num;
			OnValueUpdate?.Invoke(currentValue);
			if (_timePassed >= Duration)
			{
				IsRunning = false;
				OnComplete?.Invoke();
			}
		}
	}

	public override void ComponentDestroy()
	{
		base.ComponentDestroy();
		OnValueUpdate = null;
		OnComplete = null;
	}
}
