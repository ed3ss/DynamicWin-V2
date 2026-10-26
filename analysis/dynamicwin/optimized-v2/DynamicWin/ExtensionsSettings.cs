using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using SkiaSharp;

namespace DynamicWin;

[SettingsControl("base.extensions", "base.extensioncg")]
public class ExtensionsSettings : ISettingsControlGroup
{
	List<UIObject> ISettingsControlGroup.Create()
	{
		List<UIObject> list = new List<UIObject>();
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SilentSetSelected(Settings.GetSetting("extensions.enable-by-default", defaultValue: false));
		fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting("extensions.enable-by-default", x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle, "Enable Extensions by Default", "Turning on this option is not recomended as unwanted extensions may execute malicious code");
		FRoundToggle fRoundToggle2 = new FRoundToggle();
		fRoundToggle2.SilentSetSelected(Settings.GetSetting("extensions.disable-on-exception", defaultValue: true));
		fRoundToggle2.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle2.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
		{
			Settings.SetSetting("extensions.disable-on-exception", x);
		});
		Settings.CreateSettingControlBase(list, fRoundToggle2, "Disable Extension on Exception", "Automatically disables an extension when an exception occurs in it");
		Settings.CreateSettingControlBase(list, new IconButton("Open Folder", () => Resources.GetImage("folder"), delegate
		{
			Process.Start("explorer.exe", Path.Combine(DynamicWin.AppDataPath, "Registerables"));
		}, 105f), "Open Extensions Directory");
		FPanel extensionsPanel = new FPanel(() => Vector2.Zero, () => new Vector2(0f, 0f), null, () => SKColors.White.WithAlpha(25));
		extensionsPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		extensionsPanel.Layout.MarginHorizontal.SetStaticState(15f);
		StackContentComponent stackContentComponent = new StackContentComponent(extensionsPanel, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent.Pad.SetStaticState(5f);
		stackContentComponent.Gap.SetStaticState(5f);
		List<UIObject> list2 = new List<UIObject>();
		Settings.CreateSettingControlBase(list2, new IconButton("Refresh List", () => Resources.GetImage("refresh"), delegate
		{
			RefreshExtensionsList(extensionsPanel);
		}, 105f), "Found Extensions");
		list2.ForEach(delegate(UIObject x)
		{
			if (x.IsParentRoot)
			{
				x.SetParent(extensionsPanel);
			}
		});
		RefreshExtensionsList(extensionsPanel);
		list.Add(extensionsPanel);
		extensionsPanel.Layout.RecursivelyUpdateLayout();
		stackContentComponent.FullUpdateLayout();
		return list;
	}

	private void RefreshExtensionsList(UIObject panel)
	{
		List<UIObject> list = panel.Children.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (i != 0)
			{
				list[i].Dispose();
			}
		}
		RegisterableLoader.RefreshRegisterables();
		int num = 0;
		foreach (KeyValuePair<string, LoadedRegisterable> registerable in RegisterableLoader.Registerables)
		{
			List<UIObject> list2 = new List<UIObject>();
			RegisterableMetadataAttribute data = registerable.Value.metadata;
			FRoundToggle fRoundToggle = new FRoundToggle();
			fRoundToggle.SilentSetSelected(Settings.GetSetting("extensions.enable." + data.RegisterableID, Settings.GetSetting("extensions.enable-by-default", defaultValue: false) || data.IsBaseExtension));
			fRoundToggle.OnSelectionChanged = (Action<bool, SelectableButton>)Delegate.Combine(fRoundToggle.OnSelectionChanged, (Action<bool, SelectableButton>)delegate(bool x, SelectableButton y)
			{
				Settings.SetSetting("extensions.enable." + data.RegisterableID, x);
			});
			Settings.CreateSettingControlBase(list2, fRoundToggle, data.Name, $"v{data.Version.ToString()} by {data.Author}. {data.Description}");
			if (data.IsBaseExtension)
			{
				fRoundToggle.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
			}
			if (num != RegisterableLoader.Registerables.Count - 1)
			{
				Settings.CreateSettingsControlSeparator(list2);
			}
			list2.ForEach(delegate(UIObject x)
			{
				if (x.IsParentRoot)
				{
					x.SetParent(panel);
				}
			});
			num++;
		}
	}
}
