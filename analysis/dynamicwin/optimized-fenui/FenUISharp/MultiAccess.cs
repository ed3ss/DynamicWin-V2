using System;
using System.Collections.Generic;
using System.Linq;

namespace FenUISharp;

public class MultiAccess<T>
{
	public struct MA_V
	{
		public object caller;

		public T value;

		public float priority;
	}

	public Action<T>? onValueUpdated;

	private List<MA_V> valueList = new List<MA_V>();

	public T Value => GetValue();

	public MultiAccess(T defaultValue = default(T))
	{
		valueList.Add(new MA_V
		{
			caller = this,
			priority = 0f,
			value = defaultValue
		});
	}

	public void SetValue(object caller, T value, float priority)
	{
		if (valueList.Any(delegate(MA_V x)
		{
			if (x.caller == caller && x.value != null)
			{
				ref T value3 = ref x.value;
				object obj = value;
				return value3.Equals(obj);
			}
			return false;
		}))
		{
			return;
		}
		T value2 = Value;
		if (valueList.Any((MA_V x) => x.caller == caller))
		{
			DissolveValue(caller);
		}
		for (int num = 0; num < valueList.Count; num++)
		{
			if (priority > valueList[num].priority)
			{
				valueList.Insert(num, new MA_V
				{
					caller = caller,
					priority = priority,
					value = value
				});
				if (!EqualityComparer<T>.Default.Equals(Value, value2))
				{
					onValueUpdated?.Invoke(GetValue());
				}
				return;
			}
		}
		valueList.Insert(valueList.Count, new MA_V
		{
			caller = caller,
			priority = priority,
			value = value
		});
		if (!EqualityComparer<T>.Default.Equals(Value, value2))
		{
			onValueUpdated?.Invoke(GetValue());
		}
	}

	public void DissolveValue(object caller)
	{
		T value = Value;
		int num = -1;
		for (int i = 0; i < valueList.Count; i++)
		{
			if (valueList[i].caller == caller)
			{
				num = i;
			}
		}
		if (num != -1)
		{
			valueList.RemoveAt(num);
		}
		if (!EqualityComparer<T>.Default.Equals(Value, value))
		{
			onValueUpdated?.Invoke(GetValue());
		}
	}

	private T GetValue()
	{
		return valueList[0].value;
	}
}
