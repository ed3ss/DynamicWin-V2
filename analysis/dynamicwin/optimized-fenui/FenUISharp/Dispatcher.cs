using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FenUISharp;

public class Dispatcher
{
	private ulong _currentTick;

	private readonly ConcurrentQueue<DispatcherCall> dispatcherCalls = new ConcurrentQueue<DispatcherCall>();

	private readonly ConcurrentDictionary<string, bool> uniqueIds = new ConcurrentDictionary<string, bool>();

	public int PendingCallsCount => dispatcherCalls.Count;

	public int UniqueIdsCount => uniqueIds.Count;

	internal void UpdateQueue()
	{
		_currentTick++;
		if (dispatcherCalls.IsEmpty) return;
		List<DispatcherCall> list = new List<DispatcherCall>();
		DispatcherCall result;
		while (dispatcherCalls.TryDequeue(out result))
		{
			list.Add(result);
		}
		foreach (DispatcherCall item in list)
		{
			if (item == null)
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			switch (item.mode)
			{
			case 0:
				if (_currentTick - item.tickAtCall >= item.ticksLater)
				{
					flag = true;
				}
				else
				{
					flag2 = true;
				}
				break;
			case 1:
				if (DateTime.Now.Subtract(item.timeAtCall).TotalSeconds >= (double)item.sLater)
				{
					flag = true;
				}
				else
				{
					flag2 = true;
				}
				break;
			case 2:
				flag = true;
				break;
			}
			if (flag)
			{
				item.action?.Invoke();
				if (item.uniqueId != null)
				{
					uniqueIds.TryRemove(item.uniqueId, out var _);
				}
			}
			else if (flag2)
			{
				dispatcherCalls.Enqueue(item);
			}
		}
	}

	public void Invoke(Action action)
	{
		dispatcherCalls.Enqueue(new DispatcherCall(action));
	}

	public void InvokeWithID(Action action, string id)
	{
		if (uniqueIds.TryAdd(id, value: true))
		{
			dispatcherCalls.Enqueue(new DispatcherCall(action, id));
		}
	}

	public void InvokeLater(Action action, ulong ticks)
	{
		dispatcherCalls.Enqueue(new DispatcherCall(action, _currentTick, ticks));
	}

	public void InvokeLater(Action action, float seconds)
	{
		dispatcherCalls.Enqueue(new DispatcherCall(action, seconds));
	}
}
