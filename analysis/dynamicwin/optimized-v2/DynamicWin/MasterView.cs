using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace DynamicWin;

public class MasterView : View
{
	private List<(UIObject, Type?)> notches = new List<(UIObject, Type)>();

	private void OnRegisterSpring(ActivitySpring x)
	{
		Spring spring = new Spring(0.7f, 2f);
		SpringNotch springNotch = new SpringNotch();
		springNotch.NotchActive.SetResponsiveState(() => x.Condition() && Notch.MainNotch.IsBigNotch < 0.5f && !Settings.GetSetting("general.disable-spring-notches", defaultValue: false));
		springNotch.NotchActive.Subscribe(delegate
		{
			int num = ActiveCount();
			int num2 = notches.IndexOf(notches.Find(((UIObject, Type?) tuple) => tuple.Item1 == Notch.MainNotch));
			(UIObject, Type) item = notches.Last<(UIObject, Type)>(((UIObject, Type) tuple) => tuple.Item1 == springNotch);
			notches.Remove(item);
			notches.Insert(RMath.Clamp((num % 2 == 0) ? num2 : (num2 + 1), 0, notches.Count), item);
			CenterAround(notches, (Notch.MainNotch, null));
			if (springNotch.NotchActive.CachedValue)
			{
				spring.ResetVector(Notch.MainNotch.Transform.LocalPosition.CachedValue - new Vector2(0f, 5f) + new Vector2((num % 2 == 0) ? (0f - Notch.MainNotch.Transform.VisibleSize.x / 2.25f) : (Notch.MainNotch.Transform.VisibleSize.x / 2.25f), 0f));
			}
			else
			{
				springNotch.Transform.LocalPosition.SetStaticState(springNotch.Transform.LocalPosition.CachedValue - new Vector2(0f, 5f));
			}
		});
		springNotch.Transform.LocalPosition.SetStaticState(Notch.MainNotch.Transform.LocalPosition.CachedValue - new Vector2(0f, 30f));
		InteractiveSurface interactiveSurface = springNotch.InteractiveSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, (Action<MouseInputCode>)delegate(MouseInputCode y)
		{
			if (y.state == MouseInputState.Up && y.button == MouseInputButton.Left)
			{
				MainView.OpenType(x.RelatedActivity);
			}
		});
		springNotch.Transform.LocalPosition.SetProcessor((Vector2 x2) => spring.Update(FContext.DeltaTime, x2));
		x.Content().SetParent(springNotch);
		notches.Add((springNotch, x.RelatedActivity));
	}

	private int CenterAround<T>(List<T> list, T pivot, bool keepNeighbors = true)
	{
		if (list == null)
		{
			return 0;
		}
		if (list.Count == 0)
		{
			return 0;
		}
		int num = FindIndexByReferenceOrEquality(list, pivot);
		if (num < 0)
		{
			return 0;
		}
		int num2 = ((keepNeighbors && num > 0) ? (num - 1) : num);
		int num3 = ((keepNeighbors && num < list.Count - 1) ? (num + 1) : num) - num2 + 1;
		int num4 = (list.Count - num3) / 2;
		if (num4 == num2)
		{
			return num;
		}
		List<T> range = list.GetRange(num2, num3);
		list.RemoveRange(num2, num3);
		if (num2 < num4)
		{
			num4 -= num3;
		}
		list.InsertRange(num4, range);
		return num4 + (num - num2);
	}

	private static int FindIndexByReferenceOrEquality<T>(List<T> list, T pivot)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (int i = 0; i < list.Count; i++)
		{
			if (typeof(T).IsClass && (object)list[i] == (object)pivot)
			{
				return i;
			}
			if (equalityComparer.Equals(list[i], pivot))
			{
				return i;
			}
		}
		return -1;
	}

	private void OnUnregisterSpring(ActivitySpring x)
	{
		if (notches.Any<(UIObject, Type)>(((UIObject, Type) y) => y.Item2 == x.RelatedActivity))
		{
			notches.Remove(notches.Last<(UIObject, Type)>(((UIObject, Type) y) => y.Item2 == x.RelatedActivity));
		}
	}

	public override List<UIObject> Create()
	{
		Notch notch = new Notch();
		notch.Composition.LocalZIndex.SetStaticState(1);
		Spring mainNotchSpring = new Spring(0.7f, 2.35f);
		notch.Transform.LocalPosition.SetProcessor((Vector2 x) => mainNotchSpring.Update(FContext.DeltaTime, x));
		notches.Add((notch, null));
		ActivityManager.OnSpringRegistered = (Action<ActivitySpring>)Delegate.Combine(ActivityManager.OnSpringRegistered, new Action<ActivitySpring>(OnRegisterSpring));
		ActivityManager.OnSpringUnregistered = (Action<ActivitySpring>)Delegate.Combine(ActivityManager.OnSpringUnregistered, new Action<ActivitySpring>(OnUnregisterSpring));
		MainView view = new MainView(notch);
		notch.SetView(view);
		ActivityManager.ActivitySprings.ForEach(delegate(ActivitySpring x)
		{
			OnRegisterSpring(x);
		});
		return new List<UIObject>();
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		ActivityManager.OnSpringRegistered = (Action<ActivitySpring>)Delegate.Remove(ActivityManager.OnSpringRegistered, new Action<ActivitySpring>(OnRegisterSpring));
		ActivityManager.OnSpringUnregistered = (Action<ActivitySpring>)Delegate.Remove(ActivityManager.OnSpringUnregistered, new Action<ActivitySpring>(OnUnregisterSpring));
		ActivityManager.ActivitySprings.ForEach(delegate(ActivitySpring x)
		{
			OnUnregisterSpring(x);
		});
	}

	private int ActiveCount()
	{
		return ActivityManager.ActivitySprings.Count((ActivitySpring x) => x.Condition());
	}

	private List<(UIObject, Type?)> GetActiveNotches()
	{
		List<(UIObject, Type?)> list = notches.ToList();
		list.RemoveAll(((UIObject, Type) x) => !x.Item1.GlobalEnabled);
		list.RemoveAll(((UIObject, Type) x) => x.Item1 is SpringNotch && !((SpringNotch)x.Item1).NotchActive.CachedValue);
		return list;
	}

	public override void Update()
	{
		base.Update();
		if (Notch.MainNotch == null)
		{
			return;
		}
		List<(UIObject, Type)> activeNotches = GetActiveNotches();
		if (activeNotches.Count == 0)
		{
			return;
		}
		float num = RMath.Clamp((float)Notch.MainNotch.InteractiveSurface.ExtendInteractionRadius.CachedValue - 9f, 0f, 100f);
		float num2 = (0f - (activeNotches.Sum(((UIObject, Type) n) => n.Item1.Transform.VisibleSize.x) + 18f * (float)Math.Max(0, activeNotches.Count - 1) - ((activeNotches.Count == 1) ? num : 0f))) / 2f;
		int num3 = notches.IndexOf(notches.Find(((UIObject, Type?) tuple) => tuple.Item1 == Notch.MainNotch));
		int num4 = 0;
		foreach (var item in activeNotches)
		{
			float x = item.Item1.Transform.VisibleSize.x;
			num2 += x * 0.5f;
			item.Item1.Transform.LocalPosition.SetStaticState(new Vector2(num2 + ((num3 > num4) ? (0f - num) : num) / 2f, 0f));
			num2 += x * 0.5f + 18f;
			num4++;
		}
	}
}
