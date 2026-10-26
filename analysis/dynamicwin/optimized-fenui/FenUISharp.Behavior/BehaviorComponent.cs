using System;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public class BehaviorComponent : IDisposable
{
	private WeakReference<UIObject>? WeakOwner;

	public UIObject? Owner
	{
		get
		{
			WeakReference<UIObject>? weakOwner = WeakOwner;
			if (weakOwner != null && weakOwner.TryGetTarget(out UIObject target))
			{
				return target;
			}
			return null;
		}
	}

	public bool Enabled { get; set; } = true;

	public BehaviorComponent(UIObject owner)
	{
		WeakOwner = new WeakReference<UIObject>(owner);
		Owner?.BehaviorComponents.Add(this);
	}

	public virtual void HandleEvent(BehaviorEventType type, object? data = null)
	{
	}

	public virtual void ComponentDestroy()
	{
	}

	public void Dispose()
	{
		ComponentDestroy();
		Owner?.BehaviorComponents.Remove(this);
	}
}
