using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using FenUISharp;
using Microsoft.Win32;

namespace DynamicWin;

public class DynamicWin
{
	public static Version DynamicWinVersion = new Version(2, 0, 3);

	public static string AppDataPath = Environment.GetEnvironmentVariable("DYNAMICWIN_DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DynamicWinV2-Optimized");
	private static TopmostGuard? topmostGuard;

	public static string DiscordLink = "https://discord.gg/UHFuqB9NqR";

	public static string WebsiteLink = "https://florianbutz.de";

	public static string ContributeLink = "https://github.com/FlorianButz/DynamicWin";

	public static string DonationLink = "INSERT DONATION LINK";

	public static FOverlayWindow? MainWindow { get; private set; }

	[STAThread]
	public static void Main(string[] args)
	{
		using var singleInstance = new System.Threading.Mutex(true, @"Local\DynamicWinV2Optimized", out bool firstInstance);
		if (!firstInstance) return;
		FenUI.Init();
		FenUI.SetupAppModel("florianbutz.dynamicwinv2");
		LoadResources();
		Directory.CreateDirectory(AppDataPath);
		Settings.LoadAllSettings();
		ComfortRuntime.Load();
		Settings.SetSetting("run_count", Settings.GetSetting("run_count", 0) + 1);
		SetupStartupRegKey();
		Settings.SetListenerForSettingChange("general.launch-at-login", SetupStartupRegKey);
		Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
		MainWindow = new FOverlayWindow("DynamicWin Overlay Window", "dynamicwin.overlay");
		topmostGuard = new TopmostGuard(MainWindow);
		MainWindow.Loop.PauseUpdateLoopWhenLoseFocus = true;
		MainWindow.Properties.UseSystemDarkMode = true;
		MainWindow.Properties.AlwaysOnTop = Settings.GetSetting("general.always-on-top", defaultValue: true);
		Settings.SetListenerForSettingChange("general.always-on-top", delegate
		{
			MainWindow.Properties.AlwaysOnTop = Settings.GetSetting("general.always-on-top", defaultValue: true);
		});
		MainWindow.ActiveDisplayIndex = Settings.GetSetting("general.active-display", 0);
		Settings.SetListenerForSettingChange("general.active-display", delegate
		{
			MainWindow.ActiveDisplayIndex = Settings.GetSetting("general.active-display", 0);
		});
		MainWindow.Loop.CapFrameTime = true;
		MainWindow.Loop.MaxFrameTime = 60f;
		AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;
		Action action = delegate
		{
			CreateTrayIcon();
			MainWindow.LogicDispatcher.Invoke(delegate
			{
				ComfortRuntime.Initialize(MainWindow);
				RegisterableLoader.LoadAllRegisterables();
				FContext.GetRootViewPane()?.SilentSetView(new MasterView());
				if (Array.IndexOf(args, "--settings") >= 0) Settings.CreateSettingsWindow();
			});
		};
		Console.Write(Settings.GetSetting("run_count", 0));
		if (Settings.FirstTimeOpen)
		{
			BeginOnboarding(action);
		}
		else
		{
			action();
		}
		MainWindow.Properties.IsWindowVisible = true;
		MainWindow.Loop.PauseUpdateLoopWhenLoseFocus = false;
		MainWindow.BeginWindowLoop();
	}

	private static void SetupStartupRegKey()
	{
		if (Environment.GetEnvironmentVariable("DYNAMICWIN_NO_AUTOSTART_WRITE") == "1") return;
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
		if (Settings.GetSetting("general.launch-at-login", defaultValue: false))
		{
			registryKey.SetValue("DynamicWinV2", "\"" + Application.ExecutablePath + "\"");
		}
		else
		{
			registryKey.DeleteValue("DynamicWinV2", throwOnMissingValue: false);
		}
	}

	private static void OnApplicationExit(object? sender, EventArgs e)
	{
		DiscordPresence.Instance.Dispose();
		if (topmostGuard != null && MainWindow != null)
			MainWindow.WindowDispatcher.Invoke(topmostGuard.Dispose);
		Settings.SetSetting("first_time_open", value: true);
	}

	internal static void CheckForUpdates(Action<bool>? checkComplete)
	{
		UpdateManager.CheckForUpdates(delegate(bool needsUpdate, string url)
		{
			Console.WriteLine(url);
			if (needsUpdate && !url.Contains("null"))
			{
				string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "DynamicWinV2Updater.exe");
				Process process = new Process();
				process.StartInfo.FileName = fileName;
				process.StartInfo.UseShellExecute = false;
				string text = Convert.ToBase64String(Encoding.UTF8.GetBytes(url));
				process.StartInfo.Arguments = "\"" + text + "\"";
				process.Start();
				Environment.Exit(0);
			}
			checkComplete?.Invoke(needsUpdate);
		});
	}

	private static void BeginOnboarding(Action onEnd)
	{
		if (MainWindow == null)
		{
			onEnd?.Invoke();
		}
		else
		{
			MainWindow.WithView(new OnboardingView(onEnd));
		}
	}

	internal static void OpenUrl(string url)
	{
		try
		{
			Process.Start(url);
		}
		catch
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo(url)
				{
					UseShellExecute = true
				});
			}
		}
	}

	private static void CreateTrayIcon()
	{
		if (MainWindow == null)
		{
			return;
		}
		string iconPath = Path.Combine(AppContext.BaseDirectory, "resources", "DynamicWinLogo.ico");
		MainWindow.Properties.SetWindowIcon(iconPath);
		MainWindow.Properties.CreateTrayIcon(iconPath, "DynamicWin V2");
		FWindowCallbacks callbacks = MainWindow.Callbacks;
		callbacks.TrayMouseAction = (Action<MouseInputCode>)Delegate.Combine(callbacks.TrayMouseAction, (Action<MouseInputCode>)delegate(MouseInputCode x)
		{
			if (x.state == MouseInputState.Up && x.button == MouseInputButton.Left)
			{
				Settings.CreateSettingsWindow();
			}
		});
	}

	private static void LoadResources()
	{
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "v2logo.png")), "v2logo");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "play.png")), "play");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "pause.png")), "pause");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "forward.png")), "fwd");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "rewind.png")), "rwd");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "repeat.png")), "lp");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "repeat_once.png")), "lp1");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "favorite.png")), "fvt");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "favorite_on.png")), "fvt1");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "shuffle.png")), "shf");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "testar.png")), "testar");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "albumcover-placeholder.png")), "cover-plc");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "bt-placeholder.png")), "bt-plc");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "update.png")), "update");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "archive.png")), "tray");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "share.png")), "share");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "default_settings_category_icon.png")), "default-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "extensions_settings_category_icon.png")), "extensions-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "activities_settings_category_icon.png")), "activity-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "music_settings_category_icon.png")), "music-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "tray_settings_category_icon.png")), "tray-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "about_settings_category_icon.png")), "about-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "notch_settings_category_icon.png")), "notch-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "calendar_settings_category_icon.png")), "calendar-settings-category-icon");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "folder.png")), "folder");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "refresh-ccw.png")), "refresh");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "external.png")), "external");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "unlink.png")), "unlink");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "link.png")), "link");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "bluetooth.png")), "bt");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "delete.png")), "delete");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "list.png")), "list");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "monitor.png")), "monitor");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "file-icon.png")), "file");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "arrow-left.png")), "arrow-left");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "favorite_filled.png")), "favorite-filled");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "favorite_empty.png")), "favorite-empty");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "trash.png")), "trash");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "x.png")), "x");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "notch_preview.png")), "notch-preview");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "calendar_onboarding.png")), "calendar-preview");
		Resources.RegisterImage(Resources.LoadImage(Path.Combine(AppContext.BaseDirectory, "resources", "clipboard_copy.png")), "clipboard-copy");
		Resources.RegisterFav(Resources.LoadFav(Path.Combine(AppContext.BaseDirectory, "resources", "play.fav")), "play");
		Resources.RegisterFav(Resources.LoadFav(Path.Combine(AppContext.BaseDirectory, "resources", "pause.fav")), "pause");
		Resources.RegisterFav(Resources.LoadFav(Path.Combine(AppContext.BaseDirectory, "resources", "fast-forward.fav")), "forward");
		Resources.RegisterFav(Resources.LoadFav(Path.Combine(AppContext.BaseDirectory, "resources", "fast-rewind.fav")), "rewind");
		Resources.RegisterFav(Resources.LoadFav(Path.Combine(AppContext.BaseDirectory, "resources", "hello.fav")), "hello");
	}
}
