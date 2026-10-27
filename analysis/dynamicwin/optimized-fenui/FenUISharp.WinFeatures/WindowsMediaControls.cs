using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FenUISharp.Logging;
using SkiaSharp;
using Windows.Media;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace FenUISharp.WinFeatures;

public class WindowsMediaControls
{
	private static GlobalSystemMediaTransportControlsSessionManager? globSessionManager;

	private static GlobalSystemMediaTransportControlsSession? currentSession;

	private static PlaybackInfo cachedInfo = new PlaybackInfo();

	private static bool continousPolling = true;

	private static SKImage? lastThumbnail;
	private static readonly SemaphoreSlim thumbnailGate = new(1, 1);

	private const int THUMBNAIL_SIZE = 512;

	public static PlaybackInfo CachedInfo => cachedInfo;

	public static Action? onMediaUpdated { get; set; }

	public static Action? onThumbnailUpdated { get; set; }

	public static Action? onTimelineUpdated { get; set; }

	public static Action? onSessionChanged { get; set; }

	public static bool ContinousPolling
	{
		get
		{
			return continousPolling;
		}
		set
		{
			if (globSessionManager != null && continousPolling != value)
			{
				if (value)
				{
					globSessionManager.SessionsChanged -= SessionChangedHandler;
				}
				else
				{
					globSessionManager.SessionsChanged += SessionChangedHandler;
				}
			}
			continousPolling = value;
		}
	}

	public static bool IsMediaActive()
	{
		if (globSessionManager?.GetCurrentSession() == null)
		{
			return false;
		}
		if (currentSession?.GetPlaybackInfo() == null)
		{
			return false;
		}
		return true;
	}

	public WindowsMediaControls()
	{
		InitWindowsMediaControls();
		PollMediaInfoThread();
	}

	private static void PollMediaInfoThread()
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			while (true)
			{
				if (ContinousPolling)
				{
					currentSession = globSessionManager?.GetCurrentSession();
					if (currentSession != null)
					{
						try
						{
							UpdateInfo().Wait();
						}
						catch (Exception)
						{
							Thread.Sleep(500);
							continue;
						}
					}
					Thread.Sleep(500);
				}
				else
				{
					Thread.Sleep(1000);
				}
			}
		});
		thread.Name = "Media Transport Controls Poller";
		thread.IsBackground = true;
		thread.Start();
	}

	private static async void InitWindowsMediaControls()
	{
		CreateSessionManager();
		TrySubscribeToCurrentSession();
		await UpdateInfo();
	}

	private static async void CreateSessionManager()
	{
		globSessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
		if (!ContinousPolling)
		{
			globSessionManager.SessionsChanged += SessionChangedHandler;
		}
		globSessionManager.SessionsChanged += delegate
		{
			onSessionChanged?.Invoke();
		};
	}

	private static void SessionChangedHandler(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
	{
		TrySubscribeToCurrentSession();
	}

	public static async Task UpdateInfo()
	{
		if (globSessionManager == null)
		{
			CreateSessionManager();
		}
		if (currentSession == null)
		{
			currentSession = globSessionManager?.GetCurrentSession();
		}
		if (currentSession != null)
		{
			try
			{
				GlobalSystemMediaTransportControlsSessionMediaProperties globalSystemMediaTransportControlsSessionMediaProperties = await currentSession.TryGetMediaPropertiesAsync();
				if (!(globalSystemMediaTransportControlsSessionMediaProperties == null))
				{
					GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo = currentSession.GetPlaybackInfo();
					GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties = currentSession.GetTimelineProperties();
					bool num = globalSystemMediaTransportControlsSessionMediaProperties.Title != cachedInfo.title || globalSystemMediaTransportControlsSessionMediaProperties.Artist != cachedInfo.artist;
					cachedInfo.title = globalSystemMediaTransportControlsSessionMediaProperties.Title;
					cachedInfo.artist = globalSystemMediaTransportControlsSessionMediaProperties.Artist;
					cachedInfo.album = globalSystemMediaTransportControlsSessionMediaProperties.AlbumTitle;
					cachedInfo.albumArtist = globalSystemMediaTransportControlsSessionMediaProperties.AlbumArtist;
					cachedInfo.sourceAppModelId = currentSession.SourceAppUserModelId;
					if (playbackInfo != null)
					{
						cachedInfo.isShuffling = playbackInfo.IsShuffleActive;
						cachedInfo.repeatMode = playbackInfo.AutoRepeatMode;
						cachedInfo.playbackState = playbackInfo.PlaybackStatus;
						cachedInfo.mediaType = playbackInfo.PlaybackType;
					}
					if (timelineProperties != null)
					{
						TimeSpan timeSpan = timelineProperties.EndTime - timelineProperties.StartTime;
						bool num2 = cachedInfo.duration != timeSpan.TotalSeconds || cachedInfo.position != timelineProperties.Position.TotalSeconds;
						cachedInfo.duration = timeSpan.TotalSeconds;
						cachedInfo.position = timelineProperties.Position.TotalSeconds;
						if (num2 && ContinousPolling)
						{
							onTimelineUpdated?.Invoke();
						}
					}
					if (num && ContinousPolling)
					{
						try
						{
							await UpdateThumbnailAsync();
							return;
						}
						catch (Exception)
						{
							return;
						}
					}
				}
				return;
			}
			catch (Exception ex2)
			{
				FLogger.Error("Exception in WindowsMediaControls: " + ex2.Message);
				return;
			}
		}
		cachedInfo = new PlaybackInfo();
	}

	private static async Task UpdateThumbnailAsync()
	{
		await thumbnailGate.WaitAsync();
		try
		{
			var session = currentSession;
			if (session == null) return;
			IRandomAccessStreamReference thumbnail = (await session.TryGetMediaPropertiesAsync()).Thumbnail;
			if (thumbnail != null)
			{
				using (IRandomAccessStreamWithContentType thumbnailStream = await thumbnail.OpenReadAsync())
				{
					var image = ConvertThumbnailToSkImage(thumbnailStream);
					if (session != currentSession) { image?.Dispose(); return; }
					if (!AreThumbnailsEqual(lastThumbnail, image))
					{
						cachedInfo.thumbnail = image;
						lastThumbnail = image;
						onThumbnailUpdated?.Invoke();
						onMediaUpdated?.Invoke();
					}
					else image?.Dispose();
					return;
				}
			}
			cachedInfo.thumbnail = null;
		}
		catch (Exception ex)
		{
			FLogger.Error("Thumbnail update failed: " + ex.GetType().Name);
		}
		finally { thumbnailGate.Release(); }
	}

	private static SKImage? ConvertThumbnailToSkImage(IRandomAccessStreamWithContentType thumbnailStream)
	{
		using SKManagedStream stream = new SKManagedStream(thumbnailStream.AsStream());
		SKBitmap sKBitmap = SKBitmap.Decode(stream);
		if (sKBitmap == null)
		{
			return null;
		}
		using SKBitmap sKBitmap2 = sKBitmap.Resize(new SKImageInfo(512, 512), SKFilterQuality.High);
		sKBitmap.Dispose();
		return (sKBitmap2 != null) ? SKImage.FromBitmap(sKBitmap2) : null;
	}

	private static bool AreThumbnailsEqual(SKImage? img1, SKImage? img2)
	{
		if (img1 == null && img2 == null)
		{
			return true;
		}
		if (img1 == null || img2 == null)
		{
			return false;
		}
		if (img1.Width != img2.Width || img1.Height != img2.Height)
		{
			return false;
		}
		using SKBitmap sKBitmap = SKBitmap.FromImage(img1);
		using SKBitmap sKBitmap2 = SKBitmap.FromImage(img2);
		(int, int)[] array = new(int, int)[3]
		{
			(0, 0),
			(img1.Width / 2, img1.Height / 2),
			(img1.Width - 1, img1.Height - 1)
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (x, y) = array[i];
			if (sKBitmap.GetPixel(x, y) != sKBitmap2.GetPixel(x, y))
			{
				return false;
			}
		}
		return true;
	}

	private static void TrySubscribeToCurrentSession()
	{
		GlobalSystemMediaTransportControlsSession globalSystemMediaTransportControlsSession = globSessionManager?.GetCurrentSession();
		if (!(globalSystemMediaTransportControlsSession == currentSession))
		{
			if (currentSession != null)
			{
				currentSession.MediaPropertiesChanged -= MediaPropertiesChangedHandler;
				currentSession.PlaybackInfoChanged -= PlaybackInfoChangedHandler;
			}
			currentSession = globalSystemMediaTransportControlsSession;
			if (currentSession != null)
			{
				currentSession.MediaPropertiesChanged += MediaPropertiesChangedHandler;
				currentSession.PlaybackInfoChanged += PlaybackInfoChangedHandler;
			}
		}
	}

	public static async void SetTimelineTime(TimeSpan time)
	{
		await (currentSession?.TryChangePlaybackPositionAsync(time.Ticks));
	}

	private static async void MediaPropertiesChangedHandler(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
	{
		await UpdateInfo();
		onMediaUpdated?.Invoke();
	}

	private static async void PlaybackInfoChangedHandler(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
	{
		await UpdateInfo();
		onTimelineUpdated?.Invoke();
	}

	public static void TriggerMediaControl(MediaControlTrigger trigger)
	{
		Task.Run(async delegate
		{
			if (!(currentSession == null))
			{
				GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo = currentSession.GetPlaybackInfo();
				switch (trigger)
				{
				case MediaControlTrigger.Play:
					await currentSession.TryPlayAsync();
					break;
				case MediaControlTrigger.Stop:
					await currentSession.TryStopAsync();
					break;
				case MediaControlTrigger.PlayPauseToggle:
					await currentSession.TryTogglePlayPauseAsync();
					break;
				case MediaControlTrigger.SkipNext:
					await currentSession.TrySkipNextAsync();
					break;
				case MediaControlTrigger.SkipPrevious:
					await currentSession.TrySkipPreviousAsync();
					break;
				case MediaControlTrigger.ToggleShuffle:
					if (playbackInfo != null)
					{
						await currentSession.TryChangeShuffleActiveAsync(playbackInfo.IsShuffleActive != true);
					}
					break;
				case MediaControlTrigger.SwapLoopMode:
					if (playbackInfo != null)
					{
						MediaPlaybackAutoRepeatMode? autoRepeatMode = playbackInfo.AutoRepeatMode;
						await currentSession.TryChangeAutoRepeatModeAsync(autoRepeatMode switch
						{
							MediaPlaybackAutoRepeatMode.None => MediaPlaybackAutoRepeatMode.Track, 
							MediaPlaybackAutoRepeatMode.Track => MediaPlaybackAutoRepeatMode.List, 
							_ => MediaPlaybackAutoRepeatMode.None, 
						});
					}
					break;
				}
				await UpdateInfo();
			}
		});
	}
}
