using System;
using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

internal class DCalendarEvent : UIObject
{
	private CalendarEventStruct e;

	private FText text;

	public DCalendarEvent(float width, CalendarEventStruct e)
		: base(() => Vector2.Zero, () => new Vector2(width, 35f))
	{
		this.e = e;
		FPanel fPanel = new FPanel(() => new Vector2(3f, 5f), () => new Vector2(3f, 3f));
		fPanel.CornerRadius.SetStaticState(5f);
		fPanel.RenderMaterial.SetStaticState(fPanel.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object> { ["BaseColor"] = (Func<SKColor>)(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Error) }));
		fPanel.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fPanel.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		fPanel.SetParent(this);
		text = new FText(GetTextModel(), () => new Vector2(10f, 0f), () => new Vector2(width - 10f, 35f));
		text.Layout.Alignment.SetStaticState(new Vector2(0.5f, 0f));
		text.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0f));
		text.SetParent(this);
		UpdateText();
	}

	private void UpdateText()
	{
		FContext.GetCurrentDispatcher().InvokeLater(UpdateText, 60f);
		text.Model = GetTextModel();
	}

	private TextModel GetTextModel()
	{
		TextStyle style = new TextStyle
		{
			FontSize = 12f,
			Color = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(100)
		};
		TextStyle style2 = new TextStyle
		{
			FontSize = 12f,
			Color = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(50)
		};
		TextAlign align = new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.Start
		};
		return new TextModel(new List<TextSpan>
		{
			new TextSpan(e.name.Truncate(15), style),
			new TextSpan("\n" + GetDate(e), style2)
		}, align);
	}

	public string GetDate(CalendarEventStruct e)
	{
		DateTime now = DateTime.Now;
		bool flag = e.at.TimeOfDay == TimeSpan.Zero && (e.end - e.at).TotalHours >= 24.0;
		int num = (int)(e.at - now).TotalMinutes;
		int num2 = (int)(e.end - now).TotalMinutes;
		int days = (e.at.Date - now.Date).Days;
		if (flag && days == 0)
		{
			return "Today";
		}
		if (flag && days == 1)
		{
			return "Tomorrow";
		}
		if (num <= 0 && num2 > 0)
		{
			return "Now";
		}
		if (num < 30)
		{
			return $"In {num}m";
		}
		return days switch
		{
			0 => "Today, " + e.at.ToString("hh:mm tt"), 
			1 => "Tomorrow, " + e.at.ToString("hh:mm tt"), 
			_ => e.at.ToString("MMMM dd, hh:mm tt"), 
		};
	}
}
