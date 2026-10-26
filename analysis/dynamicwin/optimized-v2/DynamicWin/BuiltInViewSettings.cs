using System;
using System.Collections.Generic;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;

namespace DynamicWin;

[SettingsControl("base.built-in-view-settings", "base.built-in-view-settings-cg")]
public class BuiltInViewSettings : ISettingsControlGroup
{
	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		string option = "general.disable-spring-notches";
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SilentSetSelected(Settings.GetSetting(option, defaultValue: false));
		fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle, "Disable Spring Notches", "Disables the splitting of the notch when an activity is running but a different view is currently selected");
		return list;
	}
}
