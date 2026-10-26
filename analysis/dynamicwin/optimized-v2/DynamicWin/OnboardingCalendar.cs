using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class OnboardingCalendar : View
{
	private IconButton button;

	public override List<UIObject> Create()
	{
		FText fText = new FText(TextModelFactory.CreateBasic("Google Calendar", 24f, bold: true, italic: false, underlined: false, null, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), null, () => new Vector2(300f, 20f));
		fText.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fText.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		FText fText2 = new FText(TextModelFactory.CreateBasic("Connect your google account to view upcoming events", 17f, bold: false, italic: false, underlined: false, () => SKColors.Gray, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), null, () => new Vector2(500f, 20f));
		fText2.Transform.LocalPosition.SetStaticState(new Vector2(0f, 32f));
		fText2.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fText2.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		FImage fImage = new FImage(() => Resources.GetImage("calendar-preview"), drawBackground: false, dynamicColor: false, () => new Vector2(0f, 30f), () => new Vector2(0f, 155f));
		fImage.Layout.StretchHorizontal.SetStaticState(value: true);
		fImage.ScaleMode.SetStaticState(FImage.ImageScaleMode.Contain);
		UpdateButton();
		return new List<UIObject> { fText, fText2, fImage };
	}

	private void UpdateButton()
	{
		button?.Dispose();
		Dispatcher dispatcher = FContext.GetCurrentDispatcher();
		if (!CalendarFetcher.IsUserLoggedInGoogle())
		{
			button = new IconButton("Connect", () => Resources.GetImage("link"), delegate
			{
			}, 86f);
			IconButton iconButton = button;
			iconButton.OnClick = (Action)Delegate.Combine(iconButton.OnClick, (Action)delegate
			{
				Task.Run(() => CalendarFetcher.TryLoginGoogle(delegate
				{
					dispatcher.Invoke(delegate
					{
						UpdateButton();
					});
				}));
			});
			button.Transform.LocalPosition.SetStaticState(new Vector2(0f, 90f));
		}
		else
		{
			button = new IconButton("Disconnect", () => Resources.GetImage("unlink"), delegate
			{
			}, 100f);
			IconButton iconButton2 = button;
			iconButton2.OnClick = (Action)Delegate.Combine(iconButton2.OnClick, (Action)delegate
			{
				Task.Run(() => CalendarFetcher.TryLoginGoogle(delegate
				{
					dispatcher.Invoke(delegate
					{
						UpdateButton();
					});
				}));
			});
			button.Transform.LocalPosition.SetStaticState(new Vector2(0f, 90f));
		}
		button.SetParent(base.PaneRoot);
	}
}
