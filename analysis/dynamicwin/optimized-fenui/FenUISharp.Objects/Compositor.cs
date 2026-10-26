using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class Compositor : IDisposable, IStateListener
{
	[ThreadStatic]
	private static int GlobalCreationIndex = 0;

	[ThreadStatic]
	private static int activeInstances = 0;

	[ThreadStatic]
	private static List<UIObject> _cachedOrderedList = new List<UIObject>();

	public WeakReference<UIObject> Owner { get; init; }

	public State<int> LocalZIndex { get; init; }

	public int CreationIndex { get; init; }

	public static bool EnableDump { get; set; } = false;

	public Compositor(UIObject owner)
	{
		Owner = new WeakReference<UIObject>(owner);
		CreationIndex = GlobalCreationIndex;
		GlobalCreationIndex++;
		LocalZIndex = new State<int>(() => 0, owner, this);
		if (activeInstances == 0 && FContext.IsValidContext())
		{
			FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
			callbacks.OnPreUpdate = (Action)Delegate.Combine(callbacks.OnPreUpdate, new Action(CacheZOrderedListOfEverything));
		}
		activeInstances++;
	}

	private static void CacheZOrderedListOfEverything()
	{
		if (FContext.GetRootViewPane() != null)
		{
			List<UIObject> list = new List<UIObject>();
			TraverseAndCollect(FContext.GetRootViewPane(), list);
			_cachedOrderedList = list;
		}
	}

	public List<UIObject> GetZOrderedListOfChildren(UIObject root)
	{
		return (from child in root.Children
			orderby child.Composition.LocalZIndex.CachedValue, child.Composition.CreationIndex
			select child).ToList();
	}

	public List<UIObject> GetZOrderedListOfEnabled()
	{
		return _cachedOrderedList.Where((UIObject x) => x.GlobalEnabled && x.GlobalVisible).ToList();
	}

	public List<UIObject> GetZOrderedListOfEverything()
	{
		return _cachedOrderedList;
	}

	private static void TraverseAndCollect(UIObject current, List<UIObject> list, bool enabledAndVisibleOnly = false)
	{
		if (enabledAndVisibleOnly && (!current.GlobalVisible || !current.GlobalEnabled))
		{
			return;
		}
		list.Add(current);
		foreach (UIObject item in (from child in current.Children.Where(delegate(UIObject x)
			{
				if (!enabledAndVisibleOnly)
				{
					return true;
				}
				return x.GlobalEnabled && x.GlobalVisible;
			})
			orderby child.Composition.LocalZIndex.CachedValue, child.Composition.CreationIndex
			select child).ToList())
		{
			TraverseAndCollect(item, list, enabledAndVisibleOnly);
		}
	}

	public bool TestIfTopMost()
	{
		UIObject uIObject = GetZOrderedListOfEnabled().LastOrDefault((UIObject x) => x != null);
		if (Owner.TryGetTarget(out UIObject target))
		{
			if (uIObject != null)
			{
				return uIObject == target;
			}
			return false;
		}
		return false;
	}

	public SKImage? GrabBehindPlusBuffer(SKRect globalBounds, float quality)
	{
		if (globalBounds.Height < 1f || globalBounds.Width < 1f)
		{
			return null;
		}
		return FContext.GetCurrentWindow().SkiaDirectCompositionContext?.CaptureWindowRegion(globalBounds, quality);
	}

	public static void Dump(SKImage? image, string name)
	{
		if (!EnableDump || image == null)
		{
			return;
		}
		string text = Path.Combine(AppContext.BaseDirectory, "Dumps");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		using SKData sKData = image.Encode(SKEncodedImageFormat.Png, 80);
		using FileStream target = File.OpenWrite(Path.Combine(text, $"{name}_dump{DateTime.Now.Ticks}.png"));
		sKData.SaveTo(target);
	}

	public void Dispose()
	{
		activeInstances--;
		if (activeInstances <= 0 && FContext.IsValidContext())
		{
			FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
			callbacks.OnPreUpdate = (Action)Delegate.Remove(callbacks.OnPreUpdate, new Action(CacheZOrderedListOfEverything));
		}
	}

	public void OnInternalStateChanged<T>(T value)
	{
		if (Owner.TryGetTarget(out UIObject target))
		{
			target.Invalidate(UIObject.Invalidation.TransformDirty);
		}
	}
}
