using System;
using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.RuntimeEffects;
using SkiaSharp;

namespace DynamicWin;

[SettingsControl("base.about", "base.aboutcg")]
public class AboutSettings : ISettingsControlGroup
{
	private float updateTextGradientStartOffset;

	private FText? updateText;

	private FText? stateText;

	private bool isChecking;

	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		FText fText = new FText(TextModelFactory.CreateBasic("This project was made possible with fenUI#"), () => Vector2.Zero, () => new Vector2(250f, 20f));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		list.Add(fText);
		FText fText2 = new FText(TextModelFactory.CreateBasic("Made with ♥ by fen4flo"), () => Vector2.Zero, () => new Vector2(250f, 20f));
		fText2.Layout.StretchHorizontal.SetStaticState(value: true);
		list.Add(fText2);
		Settings.CreateSettingControlBase(list, new IconButton("Open", () => Resources.GetImage("external"), delegate
		{
			DynamicWin.OpenUrl(DynamicWin.DiscordLink);
		}, 75f), "Check out our Discord");
		Settings.CreateSettingControlBase(list, new IconButton("Open", () => Resources.GetImage("external"), delegate
		{
			DynamicWin.OpenUrl(DynamicWin.WebsiteLink);
		}, 75f), "Visit my website");
		Settings.CreateSettingControlBase(list, new IconButton("Open", () => Resources.GetImage("external"), delegate
		{
			DynamicWin.OpenUrl(DynamicWin.ContributeLink);
		}, 75f), "Contribute to the project");
		Settings.CreateSettingsControlSeparator(list);
		FText fText3 = new FText(TextModelFactory.CreateBasic("v" + DynamicWin.DynamicWinVersion.ToString(), 14f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.End
		}), () => Vector2.Zero, () => new Vector2(250f, 20f));
		fText3.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
		Settings.CreateSettingControlBase(list, fText3, "Dynamic Win Version");
		Settings.CreateSettingsControlSeparator(list);
		FText fText4 = new FText(TextModelFactory.CreateBasic("v" + FenUI.FenUIVersion.ToString(), 14f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.End
		}), () => Vector2.Zero, () => new Vector2(250f, 20f));
		fText4.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
		Settings.CreateSettingControlBase(list, fText4, "FenUISharp Version");
		Settings.CreateSettingsControlSeparator(list);
		string option = "about.auto-update";
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SilentSetSelected(Settings.GetSetting(option, defaultValue: true));
		fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle, "Automatic Updates");
		updateText = Settings.CreateSettingControlBase(list, new IconButton("Check for Updates", () => Resources.GetImage("update"), TestForUpdate, 140f), "Check for Updates").title;
		updateText.LocalVisible.SetStaticState(value: false);
		stateText = new FText(TextModelFactory.CreateBasic("No Updates Available"));
		stateText.SetParent(updateText);
		stateText.Transform.Size.SetStaticState(new Vector2(250f, 25f));
		stateText.LocalVisible.SetStaticState(value: false);
		updateText.SilentSetModel(TextModelFactory.CopyBasicNew("Checking for updates...", TextModelFactory.CopyBasic(updateText.Model, bold: true, textSize: 14f, italic: null, underlined: null, textColor: () => SKColors.DarkGray.Multiply(0.9f).WithAlpha(200))));
		updateText.Padding.SetStaticState(20);
		FGradientSwipeEffect fGradientSwipeEffect = new FGradientSwipeEffect(updateText);
		fGradientSwipeEffect.Primary.SetStaticState(new SKColor(byte.MaxValue, 202, 105));
		fGradientSwipeEffect.Secondary.SetStaticState(SKColors.Transparent);
		fGradientSwipeEffect.GradientPosition.SetResponsiveState(() => (FContext.Time - updateTextGradientStartOffset) / 1.2f - 0.5f);
		fGradientSwipeEffect.GradientWidth = 0.5f;
		fGradientSwipeEffect.GradientRotation = -45f;
		FBloomEffect fBloomEffect = new FBloomEffect();
		fBloomEffect.Downsampling = 1;
		fBloomEffect.BloomIntensity = 0.8f;
		fBloomEffect.BloomThreshold = 0.25f;
		fBloomEffect.BloomSpread = 12.5f;
		updateText.PostProcessChain.Attach(fGradientSwipeEffect);
		updateText.PostProcessChain.Attach(fBloomEffect);
		return list;
	}

	private void TestForUpdate()
	{
		if (updateText == null || stateText == null || isChecking)
		{
			return;
		}
		isChecking = true;
		stateText.LocalVisible.SetStaticState(value: false);
		updateText.LocalVisible.SetStaticState(value: true);
		updateTextGradientStartOffset = FContext.Time + 0.5f;
		Dispatcher dispatcher = FContext.GetCurrentDispatcher();
		Action action = delegate
		{
			DynamicWin.CheckForUpdates(delegate(bool x)
			{
				dispatcher.InvokeLater(delegate
				{
					updateText.LocalVisible.SetStaticState(value: false);
					isChecking = false;
					stateText.LocalVisible.SetStaticState(value: true);
					stateText.Model = TextModelFactory.CreateBasic((!x) ? "No Updates Available" : "Updating to latest version...", 14f, bold: false, italic: false, underlined: false, null, null, new TextAlign
					{
						HorizontalAlign = TextAlign.AlignType.Start,
						VerticalAlign = TextAlign.AlignType.Middle
					});
				}, 0.15f);
			});
		};
		FContext.GetCurrentDispatcher().InvokeLater(action, 4f);
	}
}
