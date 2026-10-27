using System;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.WinFeatures;
using Windows.Media.Control;

namespace DynamicWin;

[RegisterableMetadata("Dynamic Win Built-In", "fen4flo", "Required for all basic functionality.", "dynamicwin-builtin", "0.0.1")]
public class DynamicWinRegisterable : IRegisterable
{
	public void OnRegister()
	{
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.generalcg",
			Name = "General",
			Icon = Resources.GetImage("default-settings-category-icon")
		});
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.notchcg",
			Name = "Notch",
			Icon = Resources.GetImage("notch-settings-category-icon")
		});
		Settings.RegisterSettingsCategoryHeader("Live Activities");
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.built-in-view-settings-cg",
			Name = "Activities",
			Icon = Resources.GetImage("activity-settings-category-icon")
		});
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.mediaactivitycg",
			Name = "Media Player",
			Icon = Resources.GetImage("music-settings-category-icon")
		});
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.calendarcg",
			Name = "Calendar",
			Icon = Resources.GetImage("calendar-settings-category-icon")
		});
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.traycg",
			Name = "Files Tray",
			Icon = Resources.GetImage("tray-settings-category-icon")
		});
		Settings.RegisterSettingsCategoryHeader("Other");
		Settings.RegisterSettingsCategory(new SettingsCategory { CategoryID = "local.discordcg", Name = "Discord", Icon = Resources.GetImage("music-settings-category-icon") });
		Settings.RegisterSettingsControl<DiscordSettings>();
		Settings.RegisterSettingsCategory(new SettingsCategory { CategoryID = "local.comfortcg", Name = "Confort", Icon = Resources.GetImage("music-settings-category-icon") });
		Settings.RegisterSettingsControl<ComfortSettings>();
		DiscordSettings.Apply();
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.extensioncg",
			Name = "Extensions",
			Icon = Resources.GetImage("extensions-settings-category-icon")
		});
		Settings.RegisterSettingsCategory(new SettingsCategory
		{
			CategoryID = "base.aboutcg",
			Name = "About",
			Icon = Resources.GetImage("about-settings-category-icon")
		});
		Settings.RegisterSettingsControl<GeneralSettings>();
		Settings.RegisterSettingsControl<NotchSettings>();
		Settings.RegisterSettingsControl<BuiltInViewSettings>();
		Settings.RegisterSettingsControl<TraySettings>();
		Settings.RegisterSettingsControl<MediaSettings>();
		Settings.RegisterSettingsControl<CalendarSettings>();
		Settings.RegisterSettingsControl<ExtensionsSettings>();
		Settings.RegisterSettingsControl<AboutSettings>();
		CalendarFetcher.StartFetching(DynamicWin.MainWindow.LogicDispatcher);
		new DragDropShakeListener();
		new WindowsWorkingAreaHelper();
		new BTListenerActivity();
		if (Settings.GetSetting("about.auto-update", defaultValue: true))
		{
			ActivityManager.RegisterView<UpdatingView>();
		}
		Dispatcher dispatcher = FContext.GetCurrentDispatcher();
		Action checkForMusicActivity = delegate
		{
			if (WindowsMediaControls.IsMediaActive())
			{
				ActivityManager.RegisterView<MusicActivityView>();
			}
			else
			{
				ActivityManager.UnregisterView<MusicActivityView>();
			}
		};
		WindowsMediaControls.onSessionChanged = (Action)Delegate.Combine(WindowsMediaControls.onSessionChanged, (Action)delegate
		{
			dispatcher.Invoke(checkForMusicActivity);
		});
		WindowsMediaControls.onMediaUpdated = (Action)Delegate.Combine(WindowsMediaControls.onMediaUpdated, (Action)delegate
		{
			dispatcher.Invoke(checkForMusicActivity);
		});
		checkForMusicActivity();
		ActivityManager.RegisterSpring<MusicActivityView>(() => new SongCover(() => WindowsMediaControls.CachedInfo.thumbnail, drawBackground: false, () => new Vector2(0f, -1f), () => new Vector2(15f, 15f)), () => WindowsMediaControls.CachedInfo.playbackState == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing);
		ActivityManager.SilentRegisterView<CalendarView>();
		ActivityManager.RegisterSpring<BTConnectedView>(BTConnectedView.GetRadial, () => BTListenerActivity.ConnectedDevice != null);
		ActivityManager.RegisterView<TrayView>();
	}
}
