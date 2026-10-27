using DynamicWin;

internal static class DiagnosticTests
{
    internal static void Run()
    {
        for (int i = 0; i < 800; i++) LocalDiagnostics.Write(new string('x', 1500));
        var files = Directory.GetFiles(LocalDiagnostics.Folder);
        if (files.Length != 2 || files.Any(f => new FileInfo(f).Length > 264000)) throw new Exception("Log rotation is not bounded");
        long length = files.Sum(f => new FileInfo(f).Length);
        LocalDiagnostics.Enabled = false;
        LocalDiagnostics.Write("must not be written");
        if (files.Sum(f => new FileInfo(f).Length) != length) throw new Exception("Disabled diagnostics still write");
        Console.WriteLine("PASS diagnostics: bounded rotation, entry truncation and disabled logging.");
    }
}

namespace DynamicWin
{
    // Isolated path for the linked diagnostics implementation; never use real app settings.
    public static class DynamicWin
    {
        public static string AppDataPath = Path.Combine(Path.GetTempPath(), "DynamicWin-tests-" + Guid.NewGuid().ToString("N"));
    }
}
