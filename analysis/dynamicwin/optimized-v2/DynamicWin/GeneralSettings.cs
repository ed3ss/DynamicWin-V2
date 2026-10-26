using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

[SettingsControl("base.general", "base.generalcg")]
public class GeneralSettings : ISettingsControlGroup
{
	private FPopupPanel? panel;

	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		Settings.CreateSettingsControlTitle(list, "Active Display");
		FPanel fPanel = new FPanel(() => new Vector2(0f, 0f), () => new Vector2(0f, 125f), 7.5f, () => new SKColor(100, 100, 100, 25));
		fPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		fPanel.Layout.MarginHorizontal.SetStaticState(15f);
		fPanel.CornerRadius.SetStaticState(25f);
		fPanel.Padding.SetStaticState(0);
		StackContentComponent stackContentComponent = new StackContentComponent(fPanel, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.Scroll);
		stackContentComponent.Gap.SetStaticState(20f);
		stackContentComponent.Pad.SetStaticState(25f);
		FButtonGroup screenButtonGroup = new FButtonGroup();
		screenButtonGroup.AllowMultiSelect = false;
		screenButtonGroup.AlwaysMustSelectOne = false;
		int setting = Settings.GetSetting("general.active-display", 0);
		for (int num = 0; num < Screen.AllScreens.Length; num++)
		{
			_ = Screen.AllScreens[num];
			FImage fImage = new FImage(() => Resources.GetImage("monitor"));
			fImage.TintColor.SetResponsiveState(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.Saturate(0.3f).Multiply(1.35f));
			FLargeSelectButton fLargeSelectButton = new FLargeSelectButton(fImage, new FText(TextModelFactory.CreateBasic((num == 0) ? "Primary" : (num + 1).ToString())), null, null, () => new Vector2(0f, -25f));
			fLargeSelectButton.SetParent(fPanel);
			screenButtonGroup.Add(fLargeSelectButton);
			if (num == setting)
			{
				fLargeSelectButton.SetSelected(isSelected: true);
			}
		}
		FButtonGroup fButtonGroup = screenButtonGroup;
		fButtonGroup.OnUserSelectionChanged = (Action<bool[]>)Delegate.Combine(fButtonGroup.OnUserSelectionChanged, (Action<bool[]>)delegate
		{
			Settings.SetSetting("general.active-display", screenButtonGroup.LatestSelection);
		});
		screenButtonGroup.AlwaysMustSelectOne = true;
		stackContentComponent.FullUpdateLayout();
		list.Add(fPanel);
		string option = "general.always-on-top";
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SilentSetSelected(Settings.GetSetting(option, defaultValue: true));
		fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle, "Always On Top");
		Settings.CreateSettingsControlSeparator(list);
		string option2 = "general.always-update";
		FRoundToggle fRoundToggle2 = new FRoundToggle();
		fRoundToggle2.SilentSetSelected(Settings.GetSetting(option2, defaultValue: true));
		fRoundToggle2.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle2.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option2, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle2, "Always Refresh", "Controls whether the notch should always render updates, or only if it is focused");
		Settings.CreateSettingsControlSeparator(list);
		string option3 = "general.launch-at-login";
		FRoundToggle fRoundToggle3 = new FRoundToggle();
		fRoundToggle3.SilentSetSelected(Settings.GetSetting(option3, defaultValue: false));
		fRoundToggle3.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle3.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option3, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle3, "Start on Login");
		Settings.CreateSettingsControlSeparator(list);
		string option4 = "general.shorten-work-area";
		FRoundToggle fRoundToggle4 = new FRoundToggle();
		fRoundToggle4.SilentSetSelected(Settings.GetSetting(option4, defaultValue: false));
		fRoundToggle4.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle4.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option4, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle4, "Shorten Windows Working Area", "Limits the space other windows have to exclude the notch to avoid the notch covering important elements of the windows beneath");
		Settings.CreateSettingsControlSeparator(list);
		FSimpleButton resetBtn = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Reset")));
		FSimpleButton fSimpleButton = resetBtn;
		fSimpleButton.OnClick = (Action)Delegate.Combine(fSimpleButton.OnClick, (Action)delegate
		{
			AskToReset(resetBtn);
		});
		resetBtn.RenderMaterial.SetStaticState(new InteractableDefaultMaterial
		{
			BaseColor = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Error
		});
		Settings.CreateSettingControlBase(list, resetBtn, "Reset All Settings");
		FSimpleButton fSimpleButton2 = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Exit")));
		fSimpleButton2.OnClick = (Action)Delegate.Combine(fSimpleButton2.OnClick, (Action)delegate
		{
			Environment.Exit(0);
		});
		fSimpleButton2.RenderMaterial.SetStaticState(new InteractableDefaultMaterial
		{
			BaseColor = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Error
		});
		Settings.CreateSettingControlBase(list, fSimpleButton2, "Fully Exit Dynamic Win");
		return list;
	}

	private void AskToReset(UIObject? button)
	{
		if (panel != null)
		{
			panel.Dispose();
			panel = null;
		}
		if (button != null && !button.IsDisposed)
		{
			panel = new FPopupPanel(() => new Vector2(210f, 55f), addLayout: false);
			panel.DistanceToTarget = 45;
			panel.CornerRadius.SetStaticState(20f);
			FPopupPanel? fPopupPanel = panel;
			fPopupPanel.OnObjectDisposed = (Action)Delegate.Combine(fPopupPanel.OnObjectDisposed, (Action)delegate
			{
				panel = null;
			});
			panel.RenderMaterial.SetStaticState(new DefaultPanelMaterial());
			FText fText = new FText(TextModelFactory.CreateBasic("Are you sure you want to reset all settings?"));
			fText.Layout.StretchHorizontal.SetStaticState(value: true);
			fText.Layout.MarginHorizontal.SetStaticState(25f);
			fText.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
			fText.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
			fText.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(10f, 0f));
			fText.Layout.StretchVertical.SetStaticState(value: true);
			fText.SetParent(panel);
			FSimpleButton fSimpleButton = new FSimpleButton(new FText(TextModelFactory.CreateBasic("YES")), delegate
			{
				Settings.ResetAll();
				panel.ToggleShow();
				panel = null;
			});
			fSimpleButton.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
			fSimpleButton.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(0f, 10f));
			fSimpleButton.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
			fSimpleButton.SetParent(panel);
			panel.ToggleShow(() => (button == null || button.IsDisposed) ? default(Vector2) : button.Transform.LocalToGlobal(button.Transform.LocalPosition.CachedValue));
		}
	}
}
