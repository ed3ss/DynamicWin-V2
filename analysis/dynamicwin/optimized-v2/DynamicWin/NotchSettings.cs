using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

[SettingsControl("base.notch", "base.notchcg")]
public class NotchSettings : ISettingsControlGroup
{
	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		FPanel fPanel = new FPanel(() => new Vector2(0f, 0f), () => new Vector2(0f, 150f), 7.5f, () => new SKColor(100, 100, 100, 25));
		fPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		fPanel.Layout.MarginHorizontal.SetStaticState(15f);
		fPanel.CornerRadius.SetStaticState(25f);
		fPanel.Padding.SetStaticState(0);
		Notch notch = new Notch();
		notch.SetParent(fPanel);
		notch.isDemoNotch = true;
		FText fText = new FText(TextModelFactory.CreateBasic("Interactive Preview", 20f, bold: true, italic: false, underlined: false, () => new SKColor(150, 150, 150, 50)), () => new Vector2(0f, 10f), () => new Vector2(300f, 25f));
		fText.Composition.LocalZIndex.SetStaticState(-1);
		fText.SetParent(fPanel);
		list.Add(fPanel);
		string option = "notch.open-notch-on-hover";
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SilentSetSelected(Settings.GetSetting(option, defaultValue: false));
		fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle, "Open Notch on Hover");
		Settings.CreateSettingsControlSeparator(list);
		string option2 = "notch.interaction-radius-multiplier";
		FSlider fSlider = new FSlider(null, 150f);
		fSlider.MaxValue.SetStaticState(50f);
		fSlider.MinValue.SetStaticState(15f);
		fSlider.KnobSize = new Vector2(5f, 20f);
		fSlider.SnappingInterval = fSlider.MaxValue.CachedValue / 10f;
		fSlider.SilentSetValue(Settings.GetSetting(option2, 20f));
		fSlider.OnValueChanged = (Action<float>)Delegate.Combine(fSlider.OnValueChanged, (Action<float>)delegate(float x)
		{
			Settings.SetSetting(option2, x);
		});
		Settings.CreateSettingControlBase(list, fSlider, "Interaction Margin");
		Settings.CreateSettingsControlSeparator(list);
		string option3 = "notch.width-finetune";
		FSlider fSlider2 = new FSlider(null, 150f);
		fSlider2.MaxValue.SetStaticState(2.5f);
		fSlider2.MinValue.SetStaticState(5f / 6f);
		fSlider2.KnobSize = new Vector2(5f, 20f);
		fSlider2.SnappingInterval = 1f / 6f;
		fSlider2.SilentSetValue(Settings.GetSetting(option3, 1f));
		fSlider2.OnValueChanged = (Action<float>)Delegate.Combine(fSlider2.OnValueChanged, (Action<float>)delegate(float x)
		{
			Settings.SetSetting(option3, x);
		});
		Settings.CreateSettingControlBase(list, fSlider2, "Width", "You can tune the width of the Notch");
		Settings.CreateSettingsControlSeparator(list);
		string option4 = "notch.spring-finetune";
		FSlider fSlider3 = new FSlider(null, 150f);
		fSlider3.MaxValue.SetStaticState(3f);
		fSlider3.MinValue.SetStaticState(1f);
		fSlider3.SilentSetValue(Settings.GetSetting(option4, 1.6f));
		fSlider3.OnValueChanged = (Action<float>)Delegate.Combine(fSlider3.OnValueChanged, (Action<float>)delegate(float x)
		{
			Settings.SetSetting(option4, x);
		});
		fSlider3.ExtraHotspots.Add(1.6f);
		Settings.CreateSettingControlBase(list, fSlider3, "Spring", "You can change the springiness when the Notch changes size");
		string option5 = "notch.speed-finetune";
		FSlider fSlider4 = new FSlider(null, 150f);
		fSlider4.MaxValue.SetStaticState(4f);
		fSlider4.MinValue.SetStaticState(1.75f);
		fSlider4.SilentSetValue(Settings.GetSetting(option5, 2f));
		fSlider4.OnValueChanged = (Action<float>)Delegate.Combine(fSlider4.OnValueChanged, (Action<float>)delegate(float x)
		{
			Settings.SetSetting(option5, x);
		});
		fSlider4.ExtraHotspots.Add(2.7f);
		Settings.CreateSettingControlBase(list, fSlider4, "Speed", "You can change the speed when the Notch changes size");
		return list;
	}
}
