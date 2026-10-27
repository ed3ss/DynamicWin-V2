using System;
using System.Linq;

namespace DynamicWin;

internal static class ComfortOptions
{
    internal static string[] ParseExclusions(string text) => text.Split(new[] { ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim()).Where(x => x.Length > 0)
        .Select(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? x[..^4] : x)
        .Where(x => x.Length <= 100 && x.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.' or ' '))
        .Distinct(StringComparer.OrdinalIgnoreCase).Take(50).ToArray();

    internal static bool IsExcluded(string name, string[] exclusions) => Array.Exists(exclusions, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
    internal static double FrameRate(bool economy, bool idle, bool playing) => economy && idle && !playing ? 15 : 60;
    internal static bool ValidColor(string text) => text.Length == 7 && text[0] == '#' && text[1..].All(Uri.IsHexDigit);
}
