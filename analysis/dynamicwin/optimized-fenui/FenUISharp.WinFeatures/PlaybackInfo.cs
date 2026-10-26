using SkiaSharp;
using Windows.Media;
using Windows.Media.Control;

namespace FenUISharp.WinFeatures;

public struct PlaybackInfo
{
	public bool? isShuffling;

	public GlobalSystemMediaTransportControlsSessionPlaybackStatus? playbackState;

	public MediaPlaybackAutoRepeatMode? repeatMode;

	public MediaPlaybackType? mediaType;

	public string title;

	public string album;

	public string artist;

	public string albumArtist;

	public string? sourceAppModelId;

	public double duration;

	public double position;

	public SKImage? thumbnail;

	public PlaybackInfo()
		: this(init: false)
	{
	}

	public PlaybackInfo(bool init)
	{
		isShuffling = false;
		playbackState = GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped;
		repeatMode = MediaPlaybackAutoRepeatMode.None;
		mediaType = MediaPlaybackType.Unknown;
		title = "";
		album = "";
		artist = "";
		albumArtist = "";
		sourceAppModelId = null;
		duration = 0.0;
		position = 0.0;
		thumbnail = null;
	}
}
