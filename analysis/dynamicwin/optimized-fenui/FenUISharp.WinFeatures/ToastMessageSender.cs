using System;
using FenUISharp.Mathematics;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;

namespace FenUISharp.WinFeatures;

public class ToastMessageSender
{
	public Action<ToastArguments, ValueSet>? OnActionReceived { get; set; }

	public ToastMessageSender()
	{
		ToastNotificationManagerCompat.OnActivated += delegate(ToastNotificationActivatedEventArgsCompat toastArgs)
		{
			ToastArguments arg = ToastArguments.Parse(toastArgs.Argument);
			ValueSet userInput = toastArgs.UserInput;
			OnActionReceived?.Invoke(arg, userInput);
		};
	}

	public void ClearAll()
	{
		ToastNotificationManagerCompat.History.Clear();
	}

	public void SendToast(string title, string message, ToastButton[]? toastButtons = null, string? profileImageUrl = null, string? heroImage = null)
	{
		ToastContentBuilder toastContentBuilder = new ToastContentBuilder();
		toastContentBuilder.AddText(title);
		toastContentBuilder.AddText(message);
		if (toastButtons != null)
		{
			for (int i = 0; i < RMath.Clamp(toastButtons.Length, 0, 5); i++)
			{
				toastContentBuilder.AddButton(toastButtons[i]);
			}
		}
		if (profileImageUrl != "" && profileImageUrl != null)
		{
			toastContentBuilder.AddAppLogoOverride(Resources.GetUriFromPath(profileImageUrl));
		}
		if (heroImage != "" && heroImage != null)
		{
			toastContentBuilder.AddHeroImage(Resources.GetUriFromPath(heroImage));
		}
		toastContentBuilder.Show();
	}

	public void SendCustomToast(Func<ToastContentBuilder, ToastContentBuilder> toastModifier)
	{
		ToastContentBuilder toastContentBuilder = new ToastContentBuilder();
		toastContentBuilder = toastModifier?.Invoke(toastContentBuilder) ?? toastContentBuilder;
		toastContentBuilder.Show();
	}
}
