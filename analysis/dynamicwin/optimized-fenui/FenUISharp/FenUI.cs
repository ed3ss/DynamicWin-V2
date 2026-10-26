using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Native;
using FenUISharp.Objects;
using FenUISharp.WinFeatures;

namespace FenUISharp;

public class FenUI
{
	[ThreadStatic]
	private static bool _isMainThread;

	private static string appModelId = "";

	public static string ResourceLibName => "fenUI";

	public static Version FenUIVersion => new Version(0, 0, 2, 1);

	internal static List<FWindow> activeInstances { get; private set; } = new List<FWindow>();

	public static bool HasBeenInitialized { get; private set; } = false;

	public static string CrashHandlerPath { get; private set; } = "";

	public static bool IsMainThread => _isMainThread;

	internal static bool debugEnabled { get; private set; } = false;

	public static void Init(string[]? flags = null)
	{
		if (HasBeenInitialized)
		{
			return;
		}
		HasBeenInitialized = true;
		_isMainThread = true;
		flags = flags ?? new string[0];
		ConsoleCapture.StartCapture();
		AllowDebugOutput(allow: false);
		Win32APIs.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.PER_MONITOR_AWARE_V2);
		if (!flags.Contains("disable_crashhandler"))
		{
			FLogger.Log<FenUI>("Extracting crash handler...");
			CrashHandlerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Destination path null"), "fenUICrashHandler.exe");
			string destinationPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Destination path null"), "fenUICrashHandler.dll");
			string destinationPath2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Destination path null"), "fenUICrashHandler.runtimeconfig.json");
			try
			{
				ContentExtractor.ExtractToFile("fenUICrashHandler.runtimeconfig.json", destinationPath2, null, overwrite: true);
				ContentExtractor.ExtractToFile("fenUICrashHandler.dll", destinationPath, null, overwrite: true);
				ContentExtractor.ExtractToFile("fenUICrashHandler.exe", CrashHandlerPath, null, overwrite: true);
			}
			catch (Exception ex)
			{
				FLogger.Error("Error while extracting crash-handler: " + ex.Message + ", " + ex.StackTrace);
			}
		}
		AppDomain.CurrentDomain.UnhandledException += UnhandledException;
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "fenUICrashlogs");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		Resources.LoadDefault();
		WindowFeatures.TryInitialize(flags.Contains("disable_winfeatures"));
	}

	public static void AllowDebugOutput(bool allow)
	{
		Type[] array = new Type[10]
		{
			typeof(FenUI),
			typeof(FWindow),
			typeof(FWindowCallbacks),
			typeof(FWindowLoop),
			typeof(FWindowProcedure),
			typeof(FWindowProperties),
			typeof(FWindowShape),
			typeof(FWindowSurface),
			typeof(DirectCompositionContext),
			typeof(SkiaDirectCompositionContext)
		};
		foreach (Type item in array)
		{
			if (allow)
			{
				FLogger.ForbiddenTypes.Remove(item);
			}
			else
			{
				FLogger.ForbiddenTypes.Add(item);
			}
		}
	}

	private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		WindowFeatures.Uninitialize();
		string text = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "fenUICrashlogs"), appModelId + "-" + DateTime.Now.ToLongTimeString().Replace(':', '-') + "-crash-log.txt");
		Exception ex = (Exception)e.ExceptionObject;
		Console.WriteLine();
		Console.WriteLine("======== UNHANDLED EXCEPTION ========");
		Console.WriteLine(ex.ToString());
		Console.WriteLine($"-> Inner: {ex.InnerException} -> Msg: {ex.Message} -> Src: {ex.Source}");
		ConsoleCapture.SaveErrorLogToFile(text);
		if (!string.IsNullOrEmpty(CrashHandlerPath))
		{
			Process process = new Process();
			process.StartInfo.FileName = CrashHandlerPath;
			process.StartInfo.UseShellExecute = false;
			string value = Convert.ToBase64String(Encoding.UTF8.GetBytes(ex.ToString()));
			process.StartInfo.Arguments = $"\"{value}\" \"{text}\"";
			process.Start();
		}
		activeInstances.ForEach(delegate(FWindow x)
		{
			x.Dispose();
		});
	}

	public static void Shutdown()
	{
		if (HasBeenInitialized)
		{
			WindowFeatures.Uninitialize();
		}
	}

	public static void SetupAppModel(string appModelId)
	{
		if (!HasBeenInitialized)
		{
			throw new Exception("FenUI has to be initialized first.");
		}
		FenUI.appModelId = appModelId;
		Win32APIs.SetCurrentProcessExplicitAppUserModelID(appModelId);
	}

	public static void EnableDebugFunctions()
	{
		if (!HasBeenInitialized)
		{
			throw new Exception("FenUI has to be initialized first.");
		}
		if (debugEnabled)
		{
			return;
		}
		debugEnabled = true;
		AllowDebugOutput(allow: true);
		GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
		globalHooks.OnKeyPressed = (Action<int>)Delegate.Combine(globalHooks.OnKeyPressed, (Action<int>)delegate(int x)
		{
			switch (x)
			{
			case 119:
				activeInstances.ForEach(delegate(FWindow fWindow)
				{
					fWindow.DebugDisplayAreaCache = !fWindow.DebugDisplayAreaCache;
				});
				activeInstances.ForEach(delegate(FWindow fWindow)
				{
					fWindow.FullRedraw();
				});
				break;
			case 118:
				activeInstances.ForEach(delegate(FWindow fWindow)
				{
					fWindow.DebugDisplayBounds = !fWindow.DebugDisplayBounds;
				});
				activeInstances.ForEach(delegate(FWindow fWindow)
				{
					fWindow.FullRedraw();
				});
				break;
			case 117:
				activeInstances.ForEach(delegate(FWindow fWindow)
				{
					fWindow.DebugDisplayObjectIDs = !fWindow.DebugDisplayObjectIDs;
				});
				activeInstances.ForEach(delegate(FWindow fWindow)
				{
					fWindow.FullRedraw();
				});
				break;
			}
		});
	}

	public static void Demo()
	{
		Vector2? size = new Vector2(900f, 800f);
		FNativeWindow fNativeWindow = new FNativeWindow("Demo", "demoClass", null, size);
		fNativeWindow.Properties.UseSystemDarkMode = true;
		fNativeWindow.Properties.AllowResize = true;
		fNativeWindow.Properties.AllowMinimize = true;
		fNativeWindow.Properties.AllowMaximize = true;
		fNativeWindow.Properties.UseMica = true;
		string iconPath = Resources.ExtractResourceToTempFile<FenUI>(ResourceLibName + ".icons.fenui.ico");
		fNativeWindow.Properties.SetWindowIcon(iconPath);
		fNativeWindow.WithView(new DemoViewPane());
		fNativeWindow.Properties.IsWindowVisible = true;
		fNativeWindow.BeginWindowLoop();
	}
}
