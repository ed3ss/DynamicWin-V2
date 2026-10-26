using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class CalendarView : NotchView
{
	private int dayLines = 5;

	private float daysStartY = 25f;

	private float lineHeight = 20f;

	private float maxCalendarViewWidth = 0.6f;

	private FPanel? eventsPane;

	private List<UIObject> listBig = new List<UIObject>();

	private List<UIObject> listSmall = new List<UIObject>();

	public override Vector2 ExpandedIslandSize => new Vector2(380f, 60f + daysStartY + (float)dayLines * lineHeight);

	public override float ExpandedCornerRadius => 40f;

	public CalendarView(Notch island)
		: base(island)
	{
	}

	public override List<UIObject> Create()
	{
		int year = DateTime.Now.Year;
		int month = DateTime.Now.Month;
		int num = DateTime.DaysInMonth(year, month);
		int day = DateTime.Now.Day;
		int num2 = (int)(new DateTime(year, month, 1).DayOfWeek + 6) % 7;
		string text = DateTime.Now.ToString("MMMM");
		int startY = -5;
		FText fText = new FText(TextModelFactory.CreateBasic(text, 14f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(75), null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), () => new Vector2(0f, startY), () => new Vector2(0f, 10f));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.MarginHorizontal.SetStaticState(0f);
		fText.Layout.Alignment.SetStaticState(new Vector2(0.5f, 0f));
		fText.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0f));
		listBig.Add(fText);
		string[] array = new string[7] { "M", "T", "W", "T", "F", "S", "S" };
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			FText fText2 = new FText(TextModelFactory.CreateBasic(array[num3], 14f, bold: true, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(byte.MaxValue)), () => new Vector2(0f, daysStartY + (float)startY), () => new Vector2(20f, 15f));
			fText2.Quality.SetStaticState(0.8f);
			float x = (float)num3 / ((float)array.Length - 1f) * maxCalendarViewWidth;
			fText2.Layout.Alignment.SetStaticState(new Vector2(x, 0f));
			fText2.Layout.AlignmentAnchor.SetStaticState(new Vector2(x, 0f));
			listBig.Add(fText2);
		}
		int num4 = num2;
		for (int num5 = 0; num5 < 6; num5++)
		{
			for (int num6 = 0; num6 < array.Length; num6++)
			{
				if (num4 > 0)
				{
					num4--;
					continue;
				}
				int num7 = num6 + num5 * array.Length + 1 - num2;
				if (num7 > num)
				{
					break;
				}
				float x2 = (float)num6 / ((float)array.Length - 1f) * maxCalendarViewWidth;
				float absPosY = daysStartY + lineHeight * (float)(num5 + 1) + (float)startY;
				if (num7 == day)
				{
					FPanel fPanel = new FPanel(() => new Vector2(-1.5f, absPosY - 1f), () => new Vector2(23f, 17f), 15f, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary);
					fPanel.UseSquircle.SetStaticState(value: false);
					fPanel.Layout.Alignment.SetStaticState(new Vector2(x2, 0f));
					fPanel.Layout.AlignmentAnchor.SetStaticState(new Vector2(x2, 0f));
					listBig.Add(fPanel);
				}
				FText fText3 = new FText(TextModelFactory.CreateBasic(num7.ToString(), 12f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(125)), () => new Vector2(0f, absPosY), () => new Vector2(20f, 15f));
				fText3.Quality.SetStaticState(0.8f);
				fText3.Layout.Alignment.SetStaticState(new Vector2(x2, 0f));
				fText3.Layout.AlignmentAnchor.SetStaticState(new Vector2(x2, 0f));
				listBig.Add(fText3);
				dayLines = num5;
			}
		}
		float num8 = ExpandedIslandSize.x * maxCalendarViewWidth / 2f;
		float eventsPaneWidth = ExpandedIslandSize.x - num8 - base.RootNotch.InsideMargin - 10f;
		FText fText4 = new FText(TextModelFactory.CreateBasic("Upcoming", 14f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(75), null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), () => new Vector2(0f, startY), () => new Vector2(eventsPaneWidth / 2f, 15f));
		fText4.Layout.Alignment.SetStaticState(new Vector2(1f, 0f));
		fText4.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0f));
		listBig.Add(fText4);
		eventsPane = new FPanel(() => new Vector2(0f, (float)startY + lineHeight * 1.25f - 5f), () => new Vector2(eventsPaneWidth / 2f, 130f), 0f, () => SKColors.Transparent);
		eventsPane.Layout.Alignment.SetStaticState(new Vector2(1f, 0f));
		eventsPane.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0f));
		listBig.Add(eventsPane);
		FPanel? owner = eventsPane;
		Vector2? alignInside = new Vector2(0.5f, 0f);
		StackContentComponent stackContentComponent = new StackContentComponent(owner, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.Scroll, null, alignInside);
		stackContentComponent.FadeLength = 30f;
		stackContentComponent.ContentFade = true;
		stackContentComponent.Pad.SetStaticState(15f);
		stackContentComponent.Gap.SetStaticState(2.5f);
		eventsPane.Layout.RecursivelyUpdateLayout();
		CalendarFetcher.OnEventsUpdated = (Action)Delegate.Combine(CalendarFetcher.OnEventsUpdated, new Action(UpdateEventsAsync));
		UpdateEvents();
		FText fText5 = new FText(TextModelFactory.CreateBasic(DateTime.Now.ToString("MMMM dd, yyyy"), 14f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(125)), () => new Vector2(0f, 0f), () => new Vector2(125f, 15f));
		fText5.Layout.Alignment.SetStaticState(new Vector2(0.5f, 0.5f));
		fText5.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0.5f));
		listSmall.Add(fText5);
		stackContentComponent.FullUpdateLayout();
		List<UIObject> list = new List<UIObject>();
		list.AddRange(listSmall);
		list.AddRange(listBig);
		return list;
	}

	public override void Update()
	{
		base.Update();
		listBig.ForEach(delegate(UIObject x)
		{
			x.Enabled.SetStaticState(base.RootNotch.IsBigNotch > 0.9f);
		});
		listSmall.ForEach(delegate(UIObject x)
		{
			x.Enabled.SetStaticState(base.RootNotch.IsBigNotch < 0.2f);
		});
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		CalendarFetcher.OnEventsUpdated = (Action)Delegate.Remove(CalendarFetcher.OnEventsUpdated, new Action(UpdateEvents));
		listBig.Clear();
		listSmall.Clear();
		eventsPane?.Dispose();
		eventsPane = null;
	}

	private void UpdateEventsAsync()
	{
		FContext.GetCurrentDispatcher().Invoke(UpdateEvents);
	}

	private void UpdateEvents()
	{
		float num = ExpandedIslandSize.x * maxCalendarViewWidth / 2f;
		float eventsPaneWidth = ExpandedIslandSize.x - num - base.RootNotch.InsideMargin - 10f;
		eventsPane?.Children.ToList().ForEach(delegate(UIObject x)
		{
			if (x is DCalendarEvent || x is FText)
			{
				x.Dispose();
			}
		});
		if (CalendarFetcher.Events.Count == 0)
		{
			FText fText = new FText(TextModelFactory.CreateBasic("No Upcoming Events", 11f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(125)), () => Vector2.Zero, () => Vector2.Zero);
			new LayoutObject(fText).IgnoreParentLayout.SetStaticState(value: true);
			fText.Layout.StretchHorizontal.SetStaticState(value: true);
			fText.Layout.StretchVertical.SetStaticState(value: true);
			fText.SetParent(eventsPane);
		}
		CalendarFetcher.Events.ToList().ForEach(delegate(CalendarEventStruct x)
		{
			new DCalendarEvent(eventsPaneWidth / 2f, x).SetParent(eventsPane);
		});
		eventsPane?.Layout.RecursivelyUpdateLayout();
	}
}
