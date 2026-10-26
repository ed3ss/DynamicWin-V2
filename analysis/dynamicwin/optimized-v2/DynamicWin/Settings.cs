using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using FenUISharp;
using FenUISharp.Logging;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.Native;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.Themes;
using SkiaSharp;

namespace DynamicWin;

public class Settings
{
	private static bool _areSettingsOpen = false;

	private static FNativeWindow? _settingsWindow;

	private static SettingsView? _settingsView;

	private static Dictionary<string, List<Action>> onSettingChangedListeners = new Dictionary<string, List<Action>>();

	private static Dictionary<string, ISettingEntry> settings = new Dictionary<string, ISettingEntry>();

	public static List<SettingsCategory> Categories { get; private set; } = new List<SettingsCategory>();

	public static Dictionary<int, string> CategoryHeaders { get; private set; } = new Dictionary<int, string>();

	public static List<ISettingsControlGroup> Controls { get; private set; } = new List<ISettingsControlGroup>();

	public static string SettingsFilePath => Path.Combine(DynamicWin.AppDataPath, "settings.json");

	public static bool FirstTimeOpen => GetSetting("run_count", 0) <= 1;

	public static void CreateSettingsWindow()
	{
		if (_areSettingsOpen)
		{
			return;
		}
		_areSettingsOpen = true;
		Task.Run(delegate
		{
			Vector2? size = new Vector2(750f, 500f);
			FNativeWindow? fNativeWindow = (_settingsWindow = new FNativeWindow("Settings", "dynamicwin.settings", null, size));
			fNativeWindow.Properties.AllowResize = true;
			fNativeWindow.Shape.MinSize = new Vector2(600f, 400f);
			fNativeWindow.Shape.MaxSize = new Vector2(1000f, 800f);
			fNativeWindow.Properties.MicaBackdropType = MicaBackdropType.MainWindow;
			fNativeWindow.Properties.UseMica = true;
			fNativeWindow.Properties.UseSystemDarkMode = true;
			_settingsView = new SettingsView();
			fNativeWindow.WithView(_settingsView);
			fNativeWindow.Properties.SetWindowIcon(Path.Combine(AppContext.BaseDirectory, "resources", "DynamicWinLogo.ico"));
			Theme theme = Resources.GetTheme("default-dark").Clone();
			theme.Secondary = new SKColor(145, 145, 145, 150);
			theme.Primary = new SKColor(51, 204, 148);
			theme.PrimaryBorder = new SKColor(71, 230, 172);
			theme.PrimaryVariant = new SKColor(71, 158, 230);
			theme.OnPrimary = new SKColor(19, 117, 81);
			theme.Error = new SKColor(173, 54, 54);
			theme.Warning = new SKColor(179, 153, 59);
			theme.Secondary = new SKColor(125, 125, 125);
			theme.SecondaryVariant = new SKColor(100, 100, 100);
			theme.PanelMaterial = () => new DefaultPanelMaterial
			{
				BaseColor = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(15),
				BorderSize = () => 1f,
				DarkBorderColor = () => SKColors.Transparent
			};
			theme.InteractableMaterial = () => new EmptyDefaultMaterial
			{
				BaseColor = () => SKColors.White.WithAlpha(50)
			};
			fNativeWindow.WindowThemeManager.SetTheme(theme);
			fNativeWindow.Properties.IsWindowVisible = true;
			fNativeWindow.BeginWindowLoop();
			_settingsWindow = null;
			_areSettingsOpen = false;
			SaveAllSettings();
		});
	}

	public static void KillSettingsWindow()
	{
		_settingsWindow?.Dispose();
	}

	private static void RefreshSettingsWindow()
	{
		_settingsWindow?.WithView(new SettingsView());
	}

	public static void RegisterSettingsCategory(SettingsCategory category)
	{
		if (Categories.Where((SettingsCategory x) => x.CategoryID == category.CategoryID).Count() != 0)
		{
			FLogger.Error("Category " + category.CategoryID + " couldn't be registered since another category with the same ID has already been registered.");
		}
		else
		{
			Categories.Add(category);
		}
	}

	public static void RegisterSettingsCategoryHeader(string title)
	{
		if (!CategoryHeaders.Keys.Contains(Categories.Count))
		{
			CategoryHeaders.Add(Categories.Count, title);
		}
	}

	public static void RegisterSettingsControl<T>() where T : ISettingsControlGroup
	{
		SettingsControlAttribute attr = typeof(T).GetCustomAttribute<SettingsControlAttribute>();
		if (attr == null)
		{
			FLogger.Error($"Control {typeof(T)} couldn't be registered since it doesn't have the SettingsControlAttribute attribute.");
		}
		else if (Controls.Where((ISettingsControlGroup x) => x.GetType().GetCustomAttribute<SettingsControlAttribute>()?.ID == attr?.ID).Count() != 0)
		{
			FLogger.Error("Control " + attr.ID + " couldn't be registered since another control with the same ID has already been registered.");
		}
		else
		{
			object obj = Activator.CreateInstance(typeof(T));
			if (obj is ISettingsControlGroup)
			{
				Controls.Add((ISettingsControlGroup)obj);
			}
		}
	}

	public static (FText title, FText? desc) CreateSettingControlBase(List<UIObject> list, UIObject control, string controlName, string? description = null)
	{
		FPanel basePanel = new FPanel(() => Vector2.Zero, () => new Vector2(0f, (description == null) ? 40 : 75), 0f, () => SKColors.Transparent);
		basePanel.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
		basePanel.Layout.StretchHorizontal.SetStaticState(value: true);
		basePanel.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(20f, 20f));
		FText title = new FText(TextModelFactory.CreateBasic(controlName, 14f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), () => new Vector2(0f, (description != null) ? (-10) : 0), () => new Vector2(250f, 20f));
		title.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		title.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		title.SetParent(basePanel);
		title.InteractiveSurface.IgnoreInteractions.SetStaticState(value: false);
		FText desc = null;
		if (description != null)
		{
			desc = new FText(TextModelFactory.CreateBasic(description, 12f, bold: false, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(100), null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.Start,
				VerticalAlign = TextAlign.AlignType.Start
			}), () => new Vector2(0f, 15f), () => new Vector2(250f, 35f));
			desc.Layout.StretchHorizontal.SetStaticState(value: true);
			desc.Layout.MarginHorizontal.SetResponsiveState(() => control.Shape.LocalBounds.Width / 2f + 20f);
			desc.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
			desc.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
			desc.SetParent(basePanel);
			desc.InteractiveSurface.IgnoreInteractions.SetStaticState(value: false);
			Func<Vector2> getSize = delegate
			{
				SKRect cage = SKRect.Create(basePanel.Shape.LocalBounds.Width - (control.Shape.LocalBounds.Width + 20f), 100f);
				SKRect boundingRect = desc.LayoutModel.GetBoundingRect(desc.Model, cage);
				return new Vector2(boundingRect.Width, boundingRect.Height + 10f);
			};
			basePanel.Transform.Size.SetResponsiveState(() => new Vector2(basePanel.Transform.Size.CachedValue.x, 40f + getSize().y));
			desc.Transform.Size.SetResponsiveState(getSize);
			title.Transform.LocalPosition.SetResponsiveState(() => new Vector2(title.Transform.LocalPosition.CachedValue.x, (0f - getSize().y) / 2f));
		}
		control.SetParent(basePanel);
		control.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		control.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		control.Transform.LocalPosition.SetStaticState(new Vector2(-15f, 0f));
		list.Add(basePanel);
		return (title: title, desc: desc);
	}

	public static void CreateSettingsControlSeparator(List<UIObject> list)
	{
		FPanel fPanel = new FPanel(() => new Vector2(0f, 5f), () => new Vector2(0f, 0.5f), 5f, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(5));
		fPanel.Layout.Alignment.SetStaticState(new Vector2(0.5f, 0f));
		fPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		fPanel.Layout.MarginHorizontal.SetStaticState(35f);
		list.Add(fPanel);
	}

	public static void CreateSettingsControlTitle(List<UIObject> list, string title)
	{
		FText fText = new FText(TextModelFactory.CreateBasic(title, 20f, bold: true, italic: false, underlined: false, null, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.End
		}), () => Vector2.Zero, () => new Vector2(300f, 35f));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.MarginHorizontal.SetStaticState(20f);
		fText.Padding.SetStaticState(15);
		list.Add(fText);
	}

	public static void SetListenerForSettingChange(string settingKey, Action onSettingChanged)
	{
		if (!onSettingChangedListeners.TryGetValue(settingKey, out List<Action> _))
		{
			onSettingChangedListeners.Add(settingKey, new List<Action>());
		}
		onSettingChangedListeners[settingKey].Add(onSettingChanged);
	}

	public static void RemoveListenerForSettingChange(string settingKey, Action onSettingChanged)
	{
		if (onSettingChangedListeners.ContainsKey(settingKey))
		{
			onSettingChangedListeners[settingKey].Remove(onSettingChanged);
		}
	}

	public static void ResetAll()
	{
		settings = new Dictionary<string, ISettingEntry>();
		foreach (List<Action> value in onSettingChangedListeners.Values)
		{
			value?.ForEach(delegate(Action x)
			{
				x?.Invoke();
			});
		}
		SaveAllSettings();
		RefreshSettingsWindow();
	}

	public static void LoadAllSettings()
	{
		if (File.Exists(SettingsFilePath))
		{
			using (StreamReader streamReader = new StreamReader(SettingsFilePath))
			{
				settings = JsonSerializer.Deserialize(streamReader.ReadToEnd(), SerializationContext.Default.DictionaryStringISettingEntry) ?? new Dictionary<string, ISettingEntry>();
				return;
			}
		}
		settings = new Dictionary<string, ISettingEntry>();
	}

	public static void SaveAllSettings()
	{
		if (settings == null)
		{
			LoadAllSettings();
		}
		string value = JsonSerializer.Serialize(settings, SerializationContext.Default.DictionaryStringISettingEntry);
		using StreamWriter streamWriter = new StreamWriter(SettingsFilePath);
		streamWriter.Write(value);
	}

	public static T GetSetting<T>(string key, T defaultValue)
	{
		if (settings.TryGetValue(key, out ISettingEntry value) && defaultValue != null)
		{
			return (T)(value.Value ?? ((object)defaultValue));
		}
		return defaultValue;
	}

	public static void SetSetting<T>(string key, T value)
	{
		settings[key] = new SettingEntry<T>(value);
		if (onSettingChangedListeners.TryGetValue(key, out List<Action> value2))
		{
			value2?.ForEach(delegate(Action x)
			{
				x?.Invoke();
			});
		}
	}

	internal static void Refresh()
	{
		RefreshSettingsWindow();
	}

	internal static void RefreshView()
	{
		_settingsView?.ShowCategory(_settingsView.OpenCategory);
	}
}
