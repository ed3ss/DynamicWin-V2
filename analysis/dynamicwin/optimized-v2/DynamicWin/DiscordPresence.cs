using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using FenUISharp.WinFeatures;
using Windows.Media.Control;

namespace DynamicWin;

internal sealed class DiscordPresence : IDisposable
{
    internal static DiscordPresence Instance { get; } = new();
    private readonly object gate = new();
    private Timer? timer;
    private DiscordRpcClient? client;
    private string applicationId = "";
    private string? lastKey;
    private bool disposed;
    private bool sharing;
    private readonly ArtworkResolver artwork = new();
    private CancellationTokenSource? artworkCancellation;
    private string? artworkKey;
    private string? artworkUrl;
    private bool coversEnabled;
    private int artworkGeneration;
    private string status = "Désactivé";
    internal string Status => Volatile.Read(ref status);

    internal void Configure(bool enabled, string id, bool covers = true)
    {
        lock (gate)
        {
            if (disposed) return;
            if (coversEnabled != covers)
            {
                coversEnabled = covers;
                ResetArtwork();
                lastKey = null;
            }
            id = id.Trim();
            if (!enabled || !DiscordActivity.ValidApplicationId(id))
            {
                Disconnect();
                SetStatus(enabled ? "Application ID invalide" : "Désactivé");
                return;
            }
            if (client != null && applicationId == id) return;
            Disconnect();
            try
            {
                applicationId = id;
                client = new DiscordRpcClient(id, autoEvents: false) { SkipIdenticalPresence = true };
                client.OnReady += (_, _) => { lastKey = null; SetStatus("Connecté à Discord"); Publish(); };
                client.OnPresenceUpdate += (_, e) => SetStatus(!sharing ? "Connecté — aucune lecture partagée" : e.Presence?.Assets != null ? "Musique et pochette confirmées par Discord" : "Présence musicale confirmée par Discord");
                client.OnConnectionFailed += (_, _) => SetStatus("Discord introuvable — ouvre Discord sur ce PC");
                client.OnClose += (_, _) => { lastKey = null; SetStatus("Discord déconnecté — reconnexion automatique"); };
                client.OnError += (_, _) => { artworkUrl = null; lastKey = null; SetStatus("Présence refusée — nouvelle tentative sans pochette"); };
                SetStatus("Connexion à Discord…");
                if (!client.Initialize()) throw new InvalidOperationException("RPC initialization failed");
                timer = new Timer(Tick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }
            catch (Exception)
            {
                Disconnect();
                SetStatus("Connexion impossible — vérifie l’Application ID");
            }
        }
    }

    private void Tick(object? state)
    {
        if (!Monitor.TryEnter(gate)) return;
        try
        {
            if (disposed || client == null) return;
            client.Invoke();
            Publish();
        }
        catch (Exception) { lastKey = null; SetStatus("Connexion interrompue — nouvelle tentative automatique"); }
        finally { Monitor.Exit(gate); }
    }

    private void Publish()
    {
        if (client == null) return;
        var media = WindowsMediaControls.CachedInfo;
        bool playing = WindowsMediaControls.IsMediaActive() && media.playbackState == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        string? trackKey = playing && coversEnabled ? (media.title + "\n" + media.artist + "\n" + media.album) : null;
        if (artworkKey != trackKey)
        {
            ResetArtwork();
            artworkKey = trackKey;
            if (trackKey != null)
            {
                artworkCancellation = new CancellationTokenSource();
                _ = ResolveArtwork(media.title ?? "", media.artist ?? "", media.album ?? "", artworkGeneration, artworkCancellation.Token);
            }
        }
        var presence = DiscordActivity.Create(playing, media.title ?? "", media.artist ?? "", artworkUrl);
        string key = presence == null ? "" : presence.Details + "\n" + presence.State + "\n" + artworkUrl;
        if (key == lastKey) return;
        sharing = presence != null;
        if (presence == null) client.ClearPresence();
        else client.SetPresence(presence);
        lastKey = key;
    }

    private async Task ResolveArtwork(string title, string artist, string album, int generation, CancellationToken token)
    {
        try
        {
            string? url = await artwork.ResolveAsync(title, artist, album, token).ConfigureAwait(false);
            lock (gate)
            {
                if (disposed || client == null || token.IsCancellationRequested || generation != artworkGeneration) return;
                artworkUrl = url;
                lastKey = null; // The next one-second tick publishes the updated image.
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { /* Keep the text presence when the catalogue is unavailable. */ }
    }

    private void ResetArtwork()
    {
        artworkGeneration++;
        artworkCancellation?.Cancel();
        artworkCancellation?.Dispose();
        artworkCancellation = null;
        artworkKey = null;
        artworkUrl = null;
    }

    private void SetStatus(string value)
    {
        if (Status == value) return;
        Volatile.Write(ref status, value);
        // Keep only the latest connection result, without song titles or user data.
        try { File.WriteAllText(Path.Combine(DynamicWin.AppDataPath, "discord-status.txt"), value); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private void Disconnect()
    {
        timer?.Dispose(); timer = null;
        ResetArtwork();
        var previous = client; client = null;
        lastKey = null; applicationId = ""; sharing = false;
        previous?.Dispose();
    }

    public void Dispose()
    {
        lock (gate) { disposed = true; Disconnect(); }
    }
}
