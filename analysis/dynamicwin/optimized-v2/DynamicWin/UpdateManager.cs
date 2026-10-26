using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FenUISharp;
using FenUISharp.Logging;

namespace DynamicWin;

public class UpdateManager
{
	public class GitHubRelease
	{
		[JsonPropertyName("tag_name")]
		public string TagName { get; set; }

		[JsonPropertyName("assets")]
		public GitHubAsset[] Assets { get; set; }

		public GitHubRelease(string tagName, GitHubAsset[] assets)
		{
			TagName = tagName;
			Assets = assets;
		}
	}

	public class GitHubAsset
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("browser_download_url")]
		public string BrowserDownloadUrl { get; set; }

		public GitHubAsset(string name, string browserDownloadUrl)
		{
			Name = name;
			BrowserDownloadUrl = browserDownloadUrl;
		}
	}

	private static string owner = "FlorianButz";

	private static string repo = "DynamicWin";

	private static (DateTime fetchTime, GitHubRelease release)? lastFetchedRelease;

	public static async Task<GitHubRelease> GetLatestReleaseAsync()
	{
		if (lastFetchedRelease.HasValue && (DateTime.Now - lastFetchedRelease.Value.fetchTime).TotalSeconds <= 10.0)
		{
			return lastFetchedRelease.Value.release;
		}
		FLogger.Log("Fetching latest GitHub release...");
		using HttpClient http = new HttpClient();
		http.DefaultRequestHeaders.UserAgent.ParseAdd("DynamicWinAutoUpdater");
		string requestUri = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
		string json = await http.GetStringAsync(requestUri);
		try
		{
			GitHubRelease gitHubRelease = JsonSerializer.Deserialize(json, GitHubSerializationContext.Default.GitHubRelease);
			lastFetchedRelease = (DateTime.Now, gitHubRelease);
			return gitHubRelease;
		}
		catch (Exception ex)
		{
			FLogger.Error(ex.StackTrace);
		}
		return new GitHubRelease("", null);
	}

	public static async Task<(bool, Version, GitHubAsset[])> IsUpdateAvailable()
	{
		GitHubRelease gitHubRelease = await GetLatestReleaseAsync();
		Version version = new Version(gitHubRelease.TagName.Replace("v", "").Replace("R", "").Replace("B", "")
			.Replace("A", ""));
		Version dynamicWinVersion = DynamicWin.DynamicWinVersion;
		FLogger.Log("Current DynamicWin version: " + dynamicWinVersion);
		return (!(version <= dynamicWinVersion), version, gitHubRelease.Assets);
	}

	public static void CheckForUpdates(Action<bool, string>? checkComplete)
	{
		Dispatcher dispatcher = FContext.GetCurrentDispatcher();
		Task.Run(async delegate
		{
			(bool, Version, GitHubAsset[]) tuple = await IsUpdateAvailable();
			var (isUpdateAvailable, _, _) = tuple;
			FLogger.Log(isUpdateAvailable ? $"Update Available - Current Ver: {DynamicWin.DynamicWinVersion.ToString()} Latest Ver: {tuple.Item2}" : $"No Updates Available - Current Ver: {DynamicWin.DynamicWinVersion.ToString()} Latest Ver: {tuple.Item2}");
			string downloadUrl = ((!isUpdateAvailable) ? "null" : (tuple.Item3.Where((GitHubAsset x) => x.Name.Contains("Portable") && x.Name.Contains(".zip")).FirstOrDefault()?.BrowserDownloadUrl ?? "null"));
			dispatcher.Invoke(delegate
			{
				checkComplete?.Invoke(isUpdateAvailable, downloadUrl);
			});
		});
	}
}
