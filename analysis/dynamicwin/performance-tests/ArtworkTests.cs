using System.Net;
using System.Text;
using DynamicWin;

internal static class ArtworkTests
{
    internal static async Task RunAsync()
    {
        const string fixture = """
        {"results":[
          {"trackName":"Song","artistName":"Other","collectionName":"Album","artworkUrl100":"https://a.mzstatic.com/wrong.jpg"},
          {"trackName":"Song","artistName":"Artist","collectionName":"Old","artworkUrl100":"https://a.mzstatic.com/old.jpg"},
          {"trackName":"Song","artistName":"Artist","collectionName":"Album","artworkUrl100":"https://a.mzstatic.com/right.jpg"}
        ]}
        """;
        static void Require(bool condition, string text) { if (!condition) throw new Exception(text); }
        Require(ArtworkResolver.SelectArtwork(fixture, "Song", "Artist", "Album") == "https://a.mzstatic.com/right.jpg", "Album ranking failed");
        Require(ArtworkResolver.SelectArtwork(fixture, "Another Song", "Artist", "Album") == null, "Unrelated artwork accepted");
        Require(!ArtworkResolver.ValidImageUrl("file:///private/cover.jpg"), "Local artwork accepted");
        Require(!ArtworkResolver.ValidImageUrl("https://a.mzstatic.com.attacker.example/cover.jpg"), "Untrusted host accepted");
        Require(!ArtworkResolver.ValidImageUrl("http://a.mzstatic.com/cover.jpg"), "Insecure image accepted");
        const string deezer = """
        {"data":[{"title":"CYBERTESLA","artist":{"name":"L2B "},"album":{"title":"CYBERTESLA","cover_big":"https://cdn-images.dzcdn.net/images/cover/test/500.jpg"}}]}
        """;
        Require(ArtworkResolver.SelectDeezerArtwork(deezer, "CYBERTESLA", "L2B", "") != null, "Deezer match failed");
        Require(ArtworkResolver.SelectDeezerArtwork(deezer, "CYBERTESLA", "Other", "") == null, "Wrong Deezer artist accepted");
        Require(!ArtworkResolver.ValidImageUrl("https://cdn-images.dzcdn.net.attacker.example/a.jpg"), "Deezer host spoof accepted");
        using (var fallbackHttp = new HttpClient(new FallbackHandler(deezer)))
        {
            var fallback = new ArtworkResolver(fallbackHttp);
            Require(await fallback.ResolveAsync("CYBERTESLA", "L2B", "", CancellationToken.None) != null, "Empty Apple results did not fall back");
        }
        var presence = DiscordActivity.Create(true, "Song", "Artist", "https://a.mzstatic.com/right.jpg")!;
        Require(presence.Assets?.LargeImageKey == "https://a.mzstatic.com/right.jpg", "Cover missing from presence");
        Require(DiscordActivity.Create(false, "Song", "Artist", "https://a.mzstatic.com/right.jpg") == null, "Cover persists after pause");
        var handler = new FixtureHandler(fixture);
        using var http = new HttpClient(handler);
        var resolver = new ArtworkResolver(http);
        await resolver.ResolveAsync("Song", "Artist", "Album", CancellationToken.None);
        await resolver.ResolveAsync("Song", "Artist", "Album", CancellationToken.None);
        Require(handler.Requests == 1, "Artwork cache missed");
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        try { await resolver.ResolveAsync("Other", "Artist", "", cancelled.Token); throw new Exception("Cancellation ignored"); }
        catch (OperationCanceledException) { }
        Require(handler.Requests == 1, "Cancelled request reached network");
        Console.WriteLine("PASS artwork: title/artist matching, album ranking, HTTPS host validation, pause, cache and cancellation.");
    }

    private sealed class FixtureHandler(string fixture) : HttpMessageHandler
    {
        internal int Requests;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            if (request.RequestUri?.Host != "itunes.apple.com") throw new Exception("Unexpected catalogue host");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(fixture, Encoding.UTF8, "application/json") });
        }
    }

    private sealed class FallbackHandler(string fixture) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(request.RequestUri!.Host == "api.deezer.com" ? fixture : "{\"results\":[]}", Encoding.UTF8, "application/json") });
    }
}
