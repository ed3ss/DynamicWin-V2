using System;
using System.Linq;
using System.Text;
using DiscordRPC;

namespace DynamicWin;

internal static class DiscordActivity
{
    internal static bool ValidApplicationId(string? value) => value is { Length: >= 17 and <= 20 }
        && value.All(c => c >= '0' && c <= '9') && ulong.TryParse(value, out var id) && id > 0;

    internal static string Text(string? value, string fallback)
    {
        value = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        var result = new StringBuilder();
        int bytes = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            if (bytes + rune.Utf8SequenceLength > 120) break;
            result.Append(rune); bytes += rune.Utf8SequenceLength;
        }
        return result.Length < 2 ? "♪ " + result : result.ToString();
    }

    internal static RichPresence? Create(bool playing, string title, string artist, string? artworkUrl = null) => !playing ? null : new RichPresence
    {
        Type = ActivityType.Listening,
        StatusDisplay = StatusDisplayType.Details,
        Details = Text(title, "Lecture multimédia"),
        State = Text(artist, "Avec DynamicWin"),
        Assets = ArtworkResolver.ValidImageUrl(artworkUrl) ? new Assets { LargeImageKey = artworkUrl, LargeImageText = Text(title, "Pochette du morceau") } : null
    };
}

