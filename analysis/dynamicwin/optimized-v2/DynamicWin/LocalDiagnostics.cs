using System;
using System.Diagnostics;
using System.IO;

namespace DynamicWin;

internal static class LocalDiagnostics
{
    private static readonly object gate = new();
    internal static bool Enabled = true;
    private static System.Threading.Timer? heartbeat;
    internal static string Folder => Path.Combine(DynamicWin.AppDataPath, "diagnostics");
    internal static void Write(string text)
    {
        if (!Enabled) return;
        if (text.Length > 1000) text = text[..1000];
        lock (gate)
        {
            try
            {
                Directory.CreateDirectory(Folder);
                string path = Path.Combine(Folder, "events.log");
                if (File.Exists(path) && new FileInfo(path).Length >= 256 * 1024)
                    File.Move(path, Path.Combine(Folder, "events.previous.log"), true);
                File.AppendAllText(path, DateTimeOffset.Now.ToString("O") + " " + text + Environment.NewLine);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
    internal static void Initialize()
    {
        Write("started pid=" + Environment.ProcessId);
        heartbeat = new System.Threading.Timer(_ =>
        {
            using var process = Process.GetCurrentProcess();
            Write("alive memoryMB=" + process.WorkingSet64 / (1024 * 1024));
        }, null, 60000, 60000);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Write("process exit");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                // No exception message, titles, credentials or source file paths.
                Write("unhandled " + ex.GetType().FullName);
                foreach (var frame in new StackTrace(ex, false).GetFrames() ?? Array.Empty<StackFrame>())
                    Write("at " + frame.GetMethod()?.DeclaringType?.FullName + "." + frame.GetMethod()?.Name);
            }
        };
    }
}
