using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Objects;

namespace FenUISharp.States;

public class State<T> : IDisposable
{
	private List<StateEntry<T>> values = new List<StateEntry<T>>();

	private Func<List<StateEntry<T>>, StateEntry<T>> _resolver;

	private Func<T, T> _processor;

	private T _lastValue;

	private List<IStateListener> _listener = new List<IStateListener>();

	private List<Action<T>> _action = new List<Action<T>>();

	public WeakReference<UIObject> Owner { get; init; }

	public bool ManualResolve { get; set; }

	public Func<T> Value
	{
		private get
		{
			return GetValue();
		}
		set
		{
			SetResponsiveState(value);
		}
	}

	public T CachedValue => _lastValue;

	public bool IgnoreFirstValueIfSetNotEmpty { get; set; } = true;

	public State(Func<T> defaultValue, UIObject owner, Action<T>? action = null, bool manualResolve = false)
	{
		Owner = new WeakReference<UIObject>(owner);
		if (action != null)
		{
			Subscribe(action);
		}
		values.Add(new StateEntry<T>
		{
			Value = defaultValue,
			Priority = 0u
		});
		_lastValue = defaultValue();
		_resolver = (List<StateEntry<T>> entries) => entries.OrderBy((StateEntry<T> x) => x.Priority).Last();
		_processor = (T value) => value;
		ManualResolve = manualResolve;
		if (FContext.GetCurrentWindow() == null)
		{
			throw new Exception("States can only be declared in a valid FenUISharp window context");
		}
		FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
		callbacks.OnPreUpdate = (Action)Delegate.Combine(callbacks.OnPreUpdate, new Action(Update));
		owner.OnObjectDisposed = (Action)Delegate.Combine(owner.OnObjectDisposed, new Action(Dispose));
	}

	public State(Func<T> defaultValue, UIObject owner, IStateListener? listener = null, bool manualResolve = false)
	{
		Owner = new WeakReference<UIObject>(owner);
		if (listener != null)
		{
			Subscribe(listener);
		}
		values.Add(new StateEntry<T>
		{
			Value = defaultValue,
			Priority = 0u
		});
		_lastValue = defaultValue();
		_resolver = (List<StateEntry<T>> entries) => entries.OrderBy((StateEntry<T> x) => x.Priority).Last();
		_processor = (T value) => value;
		ManualResolve = manualResolve;
		if (FContext.GetCurrentWindow() == null)
		{
			throw new Exception("States can only be declared in a valid FenUISharp window context");
		}
		FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
		callbacks.OnPreUpdate = (Action)Delegate.Combine(callbacks.OnPreUpdate, new Action(Update));
		owner.OnObjectDisposed = (Action)Delegate.Combine(owner.OnObjectDisposed, new Action(Dispose));
	}

	private Func<T> GetValue()
	{
		if (values.Count == 0)
		{
			throw new InvalidOperationException("No values available");
		}
		return _resolver(values).Value;
	}

	public void SetResolver(Func<List<StateEntry<T>>, StateEntry<T>>? resolver)
	{
		if (resolver == null)
		{
			_resolver = (List<StateEntry<T>> entries) => entries.OrderBy((StateEntry<T> x) => x.Priority).Last();
		}
		else
		{
			_resolver = resolver;
		}
		UpdateList();
	}

	public void SetProcessor(Func<T, T> processor)
	{
		_processor = processor;
	}

	public void SetStaticState(T value, uint priority = 0u)
	{
		if (priority != uint.MaxValue)
		{
			priority++;
		}
		if (values.Any((StateEntry<T> x) => x.Priority == priority))
		{
			values.RemoveAll((StateEntry<T> x) => x.Priority == priority);
		}
		values.Add(new StateEntry<T>
		{
			Value = () => value,
			Priority = priority,
			IsStatic = true
		});
		UpdateList();
	}

	public void SetResponsiveState(Func<T> value, uint priority = 0u)
	{
		if (priority != uint.MaxValue)
		{
			priority++;
		}
		if (values.Any((StateEntry<T> x) => x.Priority == priority))
		{
			values.RemoveAll((StateEntry<T> x) => x.Priority == priority);
		}
		values.Add(new StateEntry<T>
		{
			Value = value,
			Priority = priority
		});
		UpdateList();
	}

	public void DissolvePriority(uint priority)
	{
		if (values.Any((StateEntry<T> x) => x.Priority == priority))
		{
			values.RemoveAll((StateEntry<T> x) => x.Priority == priority);
		}
	}

	public void UpdateList()
	{
		if (values.Count == 0)
		{
			return;
		}
		List<StateEntry<T>> list = new List<StateEntry<T>>(values);
		if (IgnoreFirstValueIfSetNotEmpty && values.Count > 1)
		{
			list.RemoveAt(0);
		}
		StateEntry<T> stateEntry = _resolver(list);
		T lastValue = _lastValue;
		T arg = stateEntry.Value();
		_lastValue = _processor(arg);
		if (!EqualityComparer<T>.Default.Equals(lastValue, _lastValue))
		{
			FContext.GetCurrentDispatcher().Invoke(delegate
			{
				Notify(_lastValue);
			});
		}
	}

	private void Update()
	{
		if (!ManualResolve)
		{
			ReevaluateValue();
		}
	}

	public void ReevaluateValue(bool forceReevaluation = false)
	{
		if (Value != null)
		{
			T val = _processor(Value());
			T lastValue = _lastValue;
			_lastValue = val;
			if (!EqualityComparer<T>.Default.Equals(lastValue, val) || forceReevaluation)
			{
				Notify(val);
			}
		}
	}

	private void Notify(T value)
	{
		_listener.ToList().ForEach(delegate(IStateListener x)
		{
			x.OnInternalStateChanged(value);
		});
		_action.ToList().ForEach(delegate(Action<T> x)
		{
			x?.Invoke(value);
		});
	}

	public void Subscribe(IStateListener listener)
	{
		_listener.Add(listener);
	}

	public void Unsubscribe(IStateListener listener)
	{
		_listener.Remove(listener);
	}

	public void Subscribe(Action<T> action)
	{
		_action.Add(action);
	}

	public void Unsubscribe(Action<T> action)
	{
		_action.Remove(action);
	}

	public void Dispose()
	{
		if (FContext.IsValidContext())
		{
			FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
			callbacks.OnPreUpdate = (Action)Delegate.Remove(callbacks.OnPreUpdate, new Action(Update));
		}
		if (Owner.TryGetTarget(out UIObject target))
		{
			UIObject uIObject = target;
			uIObject.OnObjectDisposed = (Action)Delegate.Remove(uIObject.OnObjectDisposed, new Action(Dispose));
		}
		values = new List<StateEntry<T>>();
		_resolver = (List<StateEntry<T>> entries) => entries.OrderBy((StateEntry<T> x) => x.Priority).Last();
		_listener = new List<IStateListener>();
		_action = new List<Action<T>>();
	}
}
