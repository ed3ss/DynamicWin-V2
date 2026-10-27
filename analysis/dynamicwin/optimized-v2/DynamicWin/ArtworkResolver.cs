using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicWin;

// Resolve public artwork URLs only. Local artwork is never uploaded.
internal sealed class ArtworkResolver
{
    private static readonly HttpClient sharedClient = new() { Timeout = TimeSpan.FromSeconds(6) };
    private readonly HttpClient http;
    private readonly SemaphoreSlim serial = new(1, 1);
    private readonly Dictionary<string, (DateTime Expires, string? Url)> cache = new();

    internal ArtworkResolver(HttpClient? http = null) => this.http = http ?? sharedClient;

    internal async Task<string?> ResolveAsync(string title, string artist, string album, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist)) return null;
        string key = title + "\n" + artist + "\n" + album;
        await serial.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (cache.TryGetValue(key, out var saved) && saved.Expires > DateTime.UtcNow) return saved.Url;
            string term = Uri.EscapeDataString(artist + " " + title);
            string? result = null;
            try
            {
                string json = await http.GetStringAsync("https://itunes.apple.com/search?media=music&entity=song&limit=10&term=" + term, token).ConfigureAwait(false);
                result = SelectArtwork(json, title, artist, album);
            }
            catch (HttpRequestException) { }
            catch (OperationCanceledException) when (!token.IsCancellationRequested) { }
            if (result == null)
            {
                string json = await http.GetStringAsync("https://api.deezer.com/search?limit=25&q=" + Uri.EscapeDataString(title), token).ConfigureAwait(false);
                result = SelectDeezerArtwork(json, title, artist, album);
            }
            if (cache.Count >= 128) cache.Clear();
            cache[key] = (DateTime.UtcNow.AddMinutes(result == null ? 5 : 60), result);
            return result;
        }
        finally { serial.Release(); }
    }

    internal static string? SelectArtwork(string json, string title, string artist, string album)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array) return null;
        string? fallback = null;
        foreach (var entry in results.EnumerateArray())
        {
            if (Normalize(Value(entry, "trackName")) != Normalize(title) || Normalize(Value(entry, "artistName")) != Normalize(artist)) continue;
            string url = Value(entry, "artworkUrl100");
            if (!ValidImageUrl(url)) continue;
            if (!string.IsNullOrWhiteSpace(album) && Normalize(Value(entry, "collectionName")) == Normalize(album)) return url;
            fallback ??= url;
        }
        return fallback;
    }

    internal static string? SelectDeezerArtwork(string json, string title, string artist, string album)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var results) || results.ValueKind != JsonValueKind.Array) return null;
        string? fallback = null;
        foreach (var entry in results.EnumerateArray())
        {
            if (!entry.TryGetProperty("artist", out var author) || !entry.TryGetProperty("album", out var record)) continue;
            if (Normalize(Value(entry, "title")) != Normalize(title) || Normalize(Value(author, "name")) != Normalize(artist)) continue;
            string url = Value(record, "cover_big");
            if (!ValidImageUrl(url)) continue;
            if (!string.IsNullOrWhiteSpace(album) && Normalize(Value(record, "title")) == Normalize(album)) return url;
            fallback ??= url;
        }
        return fallback;
    }

    internal static bool ValidImageUrl(string? url) => url is { Length: <= 256 }
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == "https" && (uri.Host.EndsWith(".mzstatic.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("cdn-images.dzcdn.net", StringComparison.OrdinalIgnoreCase));

    private static string Value(JsonElement element, string key) => element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
    private static string Normalize(string value) => new(value.Normalize(NormalizationForm.FormD).Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}
