using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Objects;

namespace FenUISharp.RuntimeEffects;

public class PostProcessChain : IDisposable
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

	private List<IPostProcessEffect> _effects { get; set; } = new List<IPostProcessEffect>();

	public PostProcessChain(UIObject owner)
	{
		WeakOwner = new WeakReference<UIObject>(owner);
	}

	public void Attach(IPostProcessEffect effect)
	{
		_effects.Add(effect);
	}

	public void Detatch(IPostProcessEffect effect)
	{
		_effects.Remove(effect);
	}

	public void OnBeforeRender(PPInfo info)
	{
		if (Owner != null)
		{
			info.owner = Owner;
			_effects.ToList().ForEach(delegate(IPostProcessEffect x)
			{
				x.OnBeforeRender(info);
			});
		}
	}

	public void OnAfterRender(PPInfo info)
	{
		if (Owner != null)
		{
			info.owner = Owner;
			_effects.ToList().ForEach(delegate(IPostProcessEffect x)
			{
				x.OnAfterRender(info);
			});
		}
	}

	public void OnLateAfterRender(PPInfo info)
	{
		if (Owner != null)
		{
			info.owner = Owner;
			_effects.ToList().ForEach(delegate(IPostProcessEffect x)
			{
				x.OnLateAfterRender(info);
			});
		}
	}

	public void Dispose()
	{
		_effects.ForEach(delegate(IPostProcessEffect x)
		{
			if (x is IDisposable)
			{
				((IDisposable)x).Dispose();
			}
		});
		_effects = new List<IPostProcessEffect>();
		WeakOwner = null;
	}
}
