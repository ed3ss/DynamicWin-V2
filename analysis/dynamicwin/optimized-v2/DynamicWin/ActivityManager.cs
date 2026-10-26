using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Objects;

namespace DynamicWin;

public class ActivityManager
{
	internal static List<Type> Activities { get; private set; } = new List<Type>();

	internal static List<ActivitySpring> ActivitySprings { get; private set; } = new List<ActivitySpring>();

	public static Action<Type>? OnViewRegistered { get; set; }

	public static Action<Type>? OnViewSilentRegistered { get; set; }

	public static Action<Type>? OnViewUnregistered { get; set; }

	public static Action<ActivitySpring>? OnSpringRegistered { get; set; }

	public static Action<ActivitySpring>? OnSpringUnregistered { get; set; }

	public static void RegisterView<T>() where T : NotchView
	{
		if (!Activities.Contains(typeof(T)))
		{
			Activities.Add(typeof(T));
			OnViewRegistered?.Invoke(typeof(T));
		}
	}

	public static void SilentRegisterView<T>() where T : NotchView
	{
		if (!Activities.Contains(typeof(T)))
		{
			Activities.Add(typeof(T));
			OnViewSilentRegistered?.Invoke(typeof(T));
		}
	}

	public static void UnregisterView<T>() where T : NotchView
	{
		if (Activities.Contains(typeof(T)))
		{
			Activities.Remove(typeof(T));
			OnViewUnregistered?.Invoke(typeof(T));
		}
	}

	public static void RegisterSpring<T>(Func<UIObject> content, Func<bool>? condition = null) where T : NotchView
	{
		if (ActivitySprings.Any((ActivitySpring x) => x.RelatedActivity == typeof(T)))
		{
			return;
		}
		ActivitySpring activitySpring = new ActivitySpring
		{
			RelatedActivity = typeof(T),
			Condition = () => !(MainView.CurrentView is T) && (condition ?? ((Func<bool>)(() => true)))(),
			Content = content
		};
		ActivitySprings.Add(activitySpring);
		OnSpringRegistered?.Invoke(activitySpring);
	}

	public static void UnregisterSpring<T>() where T : NotchView
	{
		if (ActivitySprings.Any((ActivitySpring x) => x.RelatedActivity == typeof(T)))
		{
			ActivitySpring activitySpring = ActivitySprings.Last((ActivitySpring x) => x.RelatedActivity == typeof(T));
			ActivitySprings.Remove(activitySpring);
			OnSpringUnregistered?.Invoke(activitySpring);
		}
	}
}
