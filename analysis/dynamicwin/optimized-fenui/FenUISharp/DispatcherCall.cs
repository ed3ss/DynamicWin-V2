using System;

namespace FenUISharp;

internal class DispatcherCall
{
	public Action? action;

	public DateTime timeAtCall;

	public string? uniqueId;

	public float sLater;

	public ulong tickAtCall;

	public ulong ticksLater;

	public int mode;

	public DispatcherCall(Action action, ulong tickAtCall, ulong ticksLater)
	{
		this.action = action;
		this.tickAtCall = tickAtCall;
		this.ticksLater = ticksLater;
		mode = 0;
	}

	public DispatcherCall(Action action, float sLater)
	{
		this.action = action;
		timeAtCall = DateTime.Now;
		this.sLater = sLater;
		mode = 1;
	}

	public DispatcherCall(Action action, string? uniqueID = null)
	{
		uniqueId = uniqueID;
		this.action = action;
		mode = 2;
	}
}
