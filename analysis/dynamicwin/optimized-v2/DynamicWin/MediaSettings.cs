using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;

namespace DynamicWin;

[SettingsControl("base.mediaactivity", "base.mediaactivitycg")]
public class MediaSettings : ISettingsControlGroup
{
	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		string option = "media.disable-album-flip";
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SilentSetSelected(Settings.GetSetting(option, defaultValue: false));
		fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle, "Disable Cover Art Flip", "Disables the animation of the cover when the title was changed");
		Settings.CreateSettingsControlSeparator(list);
		string option2 = "media.peek-song-title";
		FRoundToggle fRoundToggle2 = new FRoundToggle();
		fRoundToggle2.InteractiveSurface.IgnoreInteractions.SetResponsiveState(() => Settings.GetSetting("notch.open-notch-on-hover", defaultValue: false));
		fRoundToggle2.SilentSetSelected(Settings.GetSetting(option2, defaultValue: true));
		fRoundToggle2.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle2.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option2, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle2, "Enable Peek", "Shows the title of the song when hovering over the notch");
		Settings.CreateSettingsControlSeparator(list);
		string option3 = "media.show-loop-button";
		FRoundToggle fRoundToggle3 = new FRoundToggle();
		fRoundToggle3.SilentSetSelected(Settings.GetSetting(option3, defaultValue: false));
		fRoundToggle3.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle3.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option3, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle3, "Show Loop Button", "Swaps out the favorites list button with a loop button");
		Settings.CreateSettingsControlSeparator(list);
		string option4 = "media.show-visualizer";
		FRoundToggle fRoundToggle4 = new FRoundToggle();
		fRoundToggle4.SilentSetSelected(Settings.GetSetting(option4, defaultValue: true));
		fRoundToggle4.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle4.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option4, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle4, "Show Visualizer");
		Settings.CreateSettingsControlSeparator(list);
		string option5 = "media.adaptive-visualizer-amplification";
		FRoundToggle fRoundToggle5 = new FRoundToggle();
		fRoundToggle5.SilentSetSelected(Settings.GetSetting(option5, defaultValue: true));
		fRoundToggle5.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle5.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting(option5, x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle5, "Use Adaptive Amplification", "Will try to always use the fit the amplification to the currently playing song");
		string option6 = "media.visualizer-amplification";
		FSlider fSlider = new FSlider(null, 150f);
		fSlider.InteractiveSurface.IgnoreInteractions.SetResponsiveState(() => Settings.GetSetting("media.adaptive-visualizer-amplification", defaultValue: true));
		fSlider.LocalVisible.SetResponsiveState(() => !Settings.GetSetting("media.adaptive-visualizer-amplification", defaultValue: true));
		fSlider.MaxValue.SetStaticState(4f);
		fSlider.MinValue.SetStaticState(0.1f);
		FProgressBar fProgressBar = new FProgressBar(() => RMath.Clamp(SmallVisualizer.LastAdaptiveAmplifierMultiplier, 0f, 1.25f), null, 150f);
		fProgressBar.MaxValue.SetStaticState(1.25f);
		fProgressBar.MinValue.SetStaticState(0f);
		fProgressBar.Visible.SetResponsiveState(() => Settings.GetSetting("media.adaptive-visualizer-amplification", defaultValue: true));
		fProgressBar.SetParent(fSlider);
		fSlider.SilentSetValue(Settings.GetSetting(option6, 1.25f));
		fSlider.OnValueChanged = (Action<float>)Delegate.Combine(fSlider.OnValueChanged, (Action<float>)delegate(float x)
		{
			Settings.SetSetting(option6, x);
		});
		Settings.CreateSettingControlBase(list, fSlider, "Visualizer Amplification", "You can fine tune the amplification of the visualizer if it's cut off or not moving at all");
		return list;
	}
}
