using System;
using System.Collections.Generic;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;

namespace DynamicWin;

[SettingsControl("base.tray", "base.traycg")]
public class TraySettings : ISettingsControlGroup
{
	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		string option = "ft.always-copy-to-tray-folder";
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SilentSetSelected(Settings.GetSetting(option, defaultValue: false));
		fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle, "Always Copy to Tray", "Will copy the file to the tray folder instead of moving it there");
		Settings.CreateSettingsControlSeparator(list);
		string option2 = "ft.quick-drop";
		FRoundToggle fRoundToggle2 = new FRoundToggle();
		fRoundToggle2.SilentSetSelected(Settings.GetSetting(option2, defaultValue: true));
		fRoundToggle2.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle2.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option2, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle2, "Quick Drop", "Will spawn a small drop field near the mouse when shaking the mouse during an active drag operation");
		return list;
	}
}
