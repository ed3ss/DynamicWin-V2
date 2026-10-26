using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace DynamicWin;

internal class MainView : NotchView
{
	private static MainView? instance;

	private ModelViewPane? ViewPane;

	private MainViewSidebarDots? sidebarDots;

	private static List<NotchView> Views = new List<NotchView>();

	private int currentSelection;

	internal static NotchView? CurrentView { get; private set; }

	public override bool ForceExpanded => CurrentView?.ForceExpanded ?? false;

	public override float ExpandedCornerRadius => CurrentView?.ExpandedCornerRadius ?? 10f;

	public override float MarginDivisor => CurrentView?.MarginDivisor ?? 1f;

	public override Vector2 ExpandedIslandSize => CurrentView?.ExpandedIslandSize ?? base.RootNotch.ContractedSize;

	public static Action? OnViewsChanged { get; set; }

	private int selectionMax => Views.Count;

	public MainView(Notch island)
		: base(island)
	{
	}

	public override List<UIObject> Create()
	{
		List<UIObject> list = new List<UIObject>();
		instance = this;
		if (ViewPane == null)
		{
			ViewPane = new ModelViewPane(new MusicActivityView(base.RootNotch));
			ViewPane.Layout.StretchHorizontal.SetStaticState(value: true);
			ViewPane.Layout.MarginHorizontal.SetStaticState(0f);
			ViewPane.Layout.StretchVertical.SetStaticState(value: true);
			ViewPane.Layout.MarginVertical.SetStaticState(0f);
			ViewPane.AnimInDuration = 0.2f;
			ViewPane.AnimOutDuration = 0.2f;
			ViewPane.Padding.SetStaticState(25);
			ViewPane.InteractiveSurface.EnableMouseScrolling.SetStaticState(value: true);
			InteractiveSurface interactiveSurface = ViewPane.InteractiveSurface;
			interactiveSurface.OnMouseScroll = (Action<float>)Delegate.Combine(interactiveSurface.OnMouseScroll, new Action<float>(MouseScroll));
		}
		ActivityManager.OnViewRegistered = (Action<Type>)Delegate.Combine(ActivityManager.OnViewRegistered, new Action<Type>(ViewRegistered));
		ActivityManager.OnViewSilentRegistered = (Action<Type>)Delegate.Combine(ActivityManager.OnViewSilentRegistered, new Action<Type>(ViewRegisteredSilent));
		ActivityManager.OnViewUnregistered = (Action<Type>)Delegate.Combine(ActivityManager.OnViewUnregistered, new Action<Type>(ViewUnregistered));
		GetActivityList();
		UpdateView();
		list.Add(ViewPane);
		sidebarDots = new MainViewSidebarDots();
		sidebarDots.ImageEffects.Opacity.SetResponsiveState(() => RMath.Clamp(base.RootNotch.IsBigNotch, 0f, 1f), 1u);
		sidebarDots.Transform.Size.SetResponsiveState(() => new Vector2(sidebarDots.DotSize + 10f, 100f));
		sidebarDots.Transform.LocalPosition.SetResponsiveState(() => sidebarDots.Transform.GlobalToLocal(new Vector2(base.RootNotch.Shape.GlobalBounds.Right - (float)base.RootNotch.Padding.CachedValue + sidebarDots.DotSize + 10f, base.RootNotch.Shape.GlobalBounds.MidY - 15f)));
		sidebarDots.Composition.LocalZIndex.SetStaticState(99);
		sidebarDots.DisableWhenOutOfParentBounds = false;
		FContext.GetCurrentDispatcher().Invoke(delegate
		{
			sidebarDots.SetParent(null);
		});
		return list;
	}

	public static void OpenType(Type type)
	{
		if (instance != null && Views.Any((NotchView x) => x.GetType() == type))
		{
			instance.currentSelection = Views.IndexOf(Views.Last((NotchView x) => x.GetType() == type));
			instance.UpdateView();
		}
	}

	private void GetActivityList()
	{
		Views = new List<NotchView>();
		foreach (Type activity in ActivityManager.Activities)
		{
			object obj = Activator.CreateInstance(activity, base.RootNotch);
			if (obj != null)
			{
				Views.Add((NotchView)obj);
			}
		}
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		ActivityManager.OnViewRegistered = (Action<Type>)Delegate.Remove(ActivityManager.OnViewRegistered, new Action<Type>(ViewRegistered));
		ActivityManager.OnViewSilentRegistered = (Action<Type>)Delegate.Remove(ActivityManager.OnViewSilentRegistered, new Action<Type>(ViewRegisteredSilent));
		ActivityManager.OnViewUnregistered = (Action<Type>)Delegate.Remove(ActivityManager.OnViewUnregistered, new Action<Type>(ViewUnregistered));
		instance = null;
		sidebarDots?.Dispose();
		Views = new List<NotchView>();
		ViewPane?.Dispose();
		ViewPane = null;
	}

	private void ViewRegisteredSilent(Type type)
	{
		object obj = Activator.CreateInstance(type, base.RootNotch);
		if (obj != null)
		{
			Views.Add((NotchView)obj);
		}
	}

	private void ViewUnregistered(Type type)
	{
		Views.Remove(Views.Last((NotchView x) => x.GetType() == type));
		UpdateView();
	}

	private void ViewRegistered(Type type)
	{
		ViewRegisteredSilent(type);
		currentSelection = Views.IndexOf(Views.Last((NotchView x) => x.GetType() == type));
		UpdateView();
	}

	private void UpdateView()
	{
		if (Views.Count > currentSelection && Views[currentSelection] != null)
		{
			CurrentView = Views[currentSelection];
			if (ViewPane?.ViewModel?.GetType() != Views[currentSelection].GetType() && ViewPane != null)
			{
				ViewPane.ViewModel = Views[currentSelection];
			}
		}
		if (sidebarDots != null)
		{
			sidebarDots.CurrentDot = currentSelection;
			sidebarDots.DotsCount = Views.Count;
			sidebarDots.Invalidate(UIObject.Invalidation.SurfaceDirty);
			sidebarDots.Show();
		}
		GetActivityList();
	}

	private void MouseScroll(float x)
	{
		if (x >= 1f)
		{
			currentSelection++;
		}
		else if (x <= -1f)
		{
			currentSelection--;
		}
		if (currentSelection < 0)
		{
			currentSelection = selectionMax - 1;
		}
		currentSelection %= selectionMax;
		OnViewsChanged?.Invoke();
		UpdateView();
	}
}
