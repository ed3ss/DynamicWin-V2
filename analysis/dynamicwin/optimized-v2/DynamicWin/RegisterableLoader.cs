using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FenUISharp.Logging;

namespace DynamicWin;

public class RegisterableLoader
{
	public static string[] RegisterableAssemblyPaths = new string[0];

	public static string[] RegisterableDirectoryPaths = new string[2]
	{
		Path.Combine(DynamicWin.AppDataPath, "Registerables"),
		Path.Combine(AppContext.BaseDirectory, "registerables")
	};

	public static Dictionary<string, LoadedRegisterable> Registerables { get; private set; } = new Dictionary<string, LoadedRegisterable>();

	public static void LoadAllRegisterables()
	{
		List<string> list = new List<string>();
		LoadRegisterableFromAssembly(Assembly.GetExecutingAssembly().Location, Assembly.GetExecutingAssembly());
		string[] registerableAssemblyPaths = RegisterableAssemblyPaths;
		foreach (string text in registerableAssemblyPaths)
		{
			if (File.Exists(text))
			{
				list.Add(text);
			}
		}
		registerableAssemblyPaths = RegisterableDirectoryPaths;
		foreach (string path in registerableAssemblyPaths)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string[] files = Directory.GetFiles(path);
			foreach (string item in files)
			{
				list.Add(item);
			}
		}
		foreach (string item2 in list)
		{
			if (Path.GetExtension(item2).ToLower().Equals(".dll"))
			{
				try
				{
					Assembly assembly = Assembly.LoadFile(Path.Combine(item2, item2));
					LoadRegisterableFromAssembly(item2, assembly);
				}
				catch (Exception ex)
				{
					FLogger.Error("Failed to load from '" + item2 + "': " + ex.Message);
				}
			}
		}
	}

	public static void RefreshRegisterables()
	{
		List<string> list = new List<string>();
		string[] registerableDirectoryPaths = RegisterableDirectoryPaths;
		foreach (string path in registerableDirectoryPaths)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string[] files = Directory.GetFiles(path);
			foreach (string item in files)
			{
				list.Add(item);
			}
		}
		foreach (string item2 in list)
		{
			if (!Registerables.ContainsKey(item2) && Path.GetExtension(item2).ToLower().Equals(".dll"))
			{
				try
				{
					Assembly assembly = Assembly.LoadFile(Path.Combine(item2, item2));
					LoadRegisterableFromAssembly(item2, assembly);
				}
				catch (Exception ex)
				{
					FLogger.Error("Failed to load from '" + item2 + "': " + ex.Message);
				}
			}
		}
	}

	private static void LoadRegisterableFromAssembly(string file, Assembly assembly)
	{
		List<Type> list = (from t in assembly.GetTypes()
			where typeof(IRegisterable).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract
			select t).ToList();
		if (list.Count == 0)
		{
			return;
		}
		if (list.Count > 1)
		{
			FLogger.Warn("Assembly '" + file + "' contains multiple IRegisterable types. Only the first will be used.");
		}
		Type type = list.First();
		RegisterableMetadataAttribute customAttribute = type.GetCustomAttribute<RegisterableMetadataAttribute>();
		FLogger.Log("Found extension: " + customAttribute?.Name + " by " + customAttribute?.Author);
		if (customAttribute == null)
		{
			Console.Write("[Error] Assembly '" + file + " couldn't be loaded. Invalid or no metadata.'");
			return;
		}
		string id = customAttribute.RegisterableID;
		if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
		{
			Console.Write("[Error] Assembly '" + file + " couldn't be loaded. Invalid or no ID.'");
			return;
		}
		if (Registerables.Where<KeyValuePair<string, LoadedRegisterable>>((KeyValuePair<string, LoadedRegisterable> x) => x.Value.metadata.RegisterableID == id).Count() != 0)
		{
			Console.Write("[Error] Assembly '" + file + " couldn't be loaded. ID has already been used by another assembly.'");
			return;
		}
		object obj = Activator.CreateInstance(type);
		if (obj == null || !(obj is IRegisterable))
		{
			return;
		}
		IRegisterable registerable = (IRegisterable)obj;
		if (registerable == null)
		{
			return;
		}
		Registerables.Add(file, new LoadedRegisterable
		{
			registerable = registerable,
			metadata = customAttribute
		});
		if (!Settings.GetSetting("extensions.enable." + customAttribute.RegisterableID, Settings.GetSetting("extensions.enable-by-default", defaultValue: false)) && !customAttribute.IsBaseExtension)
		{
			return;
		}
		try
		{
			registerable.OnRegister();
		}
		catch (Exception ex)
		{
			FLogger.Error($"An exception occured inside of assembly {file}. {ex.Message}; {ex.StackTrace}");
			if (Settings.GetSetting("extensions.disable-on-exception", defaultValue: true))
			{
				Settings.SetSetting("extensions.enable." + customAttribute.RegisterableID, value: false);
			}
		}
	}
}
