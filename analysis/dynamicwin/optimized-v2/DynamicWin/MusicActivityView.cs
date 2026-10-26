using System;
using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using FenUISharp.WinFeatures;
using SkiaSharp;
using Windows.Media;
using Windows.Media.Control;

namespace DynamicWin;

public class MusicActivityView : NotchView
{
	private Dispatcher dispatcher;

	private TimeSpan? lastTimeStillLeft;

	private TimeSpan? lastDuration;

	private float time_FakeAddition;

	private SongCover songCover;

	private SmallVisualizer visualizer;

	private FText title;

	private FText artist;

	private FAVControlButton playPause;

	private FAVControlButton forward;

	private FAVControlButton backward;

	private ControlButton loop;

	private ControlButton shuffle;

	private FSlider progress;

	private FText timeDuration;

	private FText timeStillLeft;

	private ControlButton listView;

	private bool isInitialized;

	private float lastBigNotch;

	private string lastTitle = "";

	private string lastArtist = "";

	private bool timelineValueLocked;

	public override Vector2 ExpandedIslandSize => new Vector2(351f, 170f);

	public override float ExpandedCornerRadius => 60f;

	public MusicActivityView(Notch island)
		: base(island)
	{
		dispatcher = FContext.GetCurrentDispatcher();
		WindowsMediaControls.onMediaUpdated = (Action)Delegate.Combine(WindowsMediaControls.onMediaUpdated, new Action(OnMediaUpdate));
		WindowsMediaControls.onTimelineUpdated = (Action)Delegate.Combine(WindowsMediaControls.onTimelineUpdated, new Action(OnTimelineUpdate));
	}

	private void OnTimelineUpdate()
	{
		dispatcher.Invoke(delegate
		{
			UpdateRealTime();
		});
	}

	private void UpdateRealTime()
	{
		time_FakeAddition = 0f;
	}

	private void UpdateTime()
	{
		float num = Math.Clamp((float)WindowsMediaControls.CachedInfo.position + time_FakeAddition, 0f, 9999f);
		TimeSpan value = TimeSpan.FromSeconds(Math.Clamp(WindowsMediaControls.CachedInfo.duration - (double)num, 0.0, 99999.0));
		string text = $"{value.Minutes:D2}:{value.Seconds:D2}";
		if ((int)value.TotalSeconds != (int)(lastTimeStillLeft?.TotalSeconds ?? 0.0))
		{
			timeStillLeft.Model = TextModelFactory.CreateBasic("-" + text, 10f, bold: false, italic: false, underlined: false, () => new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 100), null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.End,
				VerticalAlign = TextAlign.AlignType.Middle
			});
		}
		TimeSpan value2 = TimeSpan.FromSeconds(WindowsMediaControls.CachedInfo.duration);
		string text2 = $"{value2.Minutes:D2}:{value2.Seconds:D2}";
		if (lastDuration != value2)
		{
			timeDuration.Model = TextModelFactory.CreateBasic(text2, 10f, bold: false, italic: false, underlined: false, () => new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, 100), null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.Start,
				VerticalAlign = TextAlign.AlignType.Middle
			});
		}
		if (!progress.InteractiveSurface.IsMouseDown && !timelineValueLocked)
		{
			progress.Value = num;
		}
		progress.MaxValue.SetStaticState((float)WindowsMediaControls.CachedInfo.duration);
		lastTimeStillLeft = value;
		lastDuration = value2;
	}

	private void OnMediaUpdate()
	{
		if (isInitialized)
		{
			dispatcher.Invoke(delegate
			{
				UpdateText();
			});
		}
	}

	public override void OnViewShown()
	{
		base.OnViewShown();
		WindowsMediaControls.ContinousPolling = true;
	}

	public override List<UIObject> Create()
	{
		List<UIObject> list = new List<UIObject>();
		SKImage capturedImage = Resources.GetImage("cover-plc");
		songCover = new SongCover(() => capturedImage, drawBackground: false, () => Vector2.Zero, () => new Vector2(75f, 75f));
		songCover.CornerRadius.SetStaticState(20f);
		songCover.UseSquircle.SetStaticState(value: true);
		visualizer = new SmallVisualizer();
		visualizer.ManualAmplitudeMultiplier = Settings.GetSetting("media.visualizer-amplification", 1.25f);
		SKColor visualizerPrimary = songCover.GetTopTwoColors().Item1;
		SKColor visualizerBlend = songCover.GetTopTwoColors().Item2;
		visualizer.Primary.SetResponsiveState(delegate
		{
			visualizerPrimary = RMath.Lerp(visualizerPrimary, songCover.GetTopTwoColors().Item1, FContext.DeltaTime * 0.75f);
			return visualizerPrimary;
		});
		visualizer.Blend.SetResponsiveState(delegate
		{
			visualizerBlend = RMath.Lerp(visualizerBlend, songCover.GetTopTwoColors().Item2, FContext.DeltaTime * 0.75f);
			return visualizerBlend;
		});
		Settings.SetListenerForSettingChange("media.visualizer-amplification", delegate
		{
			visualizer.ManualAmplitudeMultiplier = Settings.GetSetting("media.visualizer-amplification", 1.25f);
		});
		title = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(20f, -10f), () => new Vector2(165f, 18f));
		title.Transform.LocalPosition.SetResponsiveState(() => (Settings.GetSetting("media.peek-song-title", defaultValue: true) && !Settings.GetSetting("notch.open-notch-on-hover", defaultValue: false)) ? new Vector2(10f * base.RootNotch.IsBigNotch + 10f, -10f * base.RootNotch.IsBigNotch) : new Vector2(20f, -10f));
		title.Transform.Anchor.SetStaticState(new Vector2(0f, 0.5f));
		title.SetParent(songCover);
		title.DisableWhenOutOfParentBounds = false;
		title.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		title.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		float smoothIsHovering = 0f;
		title.ImageEffects.Opacity.SetResponsiveState(delegate
		{
			if (Settings.GetSetting("media.peek-song-title", defaultValue: true) && !Settings.GetSetting("notch.open-notch-on-hover", defaultValue: false))
			{
				smoothIsHovering = (1f - base.RootNotch.IsBigNotch) * RMath.Lerp(smoothIsHovering, base.RootNotch.InteractiveSurface.IsMouseHovering ? 1 : 0, FContext.DeltaTime * 2f);
				return smoothIsHovering / 2f;
			}
			smoothIsHovering = 0f;
			return 0f;
		}, 1u);
		title.ImageEffects.Opacity.SetResolver(StateResolverTemplates.BiggestFloatResolver);
		title.Enabled.SetResponsiveState(() => !RMath.Approximately(title.ImageEffects.Opacity.CachedValue, 0f));
		title.Transform.Scale.SetResponsiveState(() => (Settings.GetSetting("media.peek-song-title", defaultValue: true) && !Settings.GetSetting("notch.open-notch-on-hover", defaultValue: false)) ? (Vector2.One * RMath.Remap(base.RootNotch.IsBigNotch, 1f, 0f, 1f, 0.65f) + smoothIsHovering * 0.125f) : Vector2.One);
		title.LayoutModel = new NumericTextLayoutProcessor(title, new WrapLayout(title));
		artist = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(0f, 4f), () => new Vector2(165f, 18f));
		artist.SetParent(title);
		artist.DisableWhenOutOfParentBounds = false;
		artist.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		artist.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0f));
		artist.LayoutModel = new NumericTextLayoutProcessor(artist, new WrapLayout(artist))
		{
			VerticalOffset = -6f,
			Duration = 0.5f
		};
		playPause = new FAVControlButton(() => (WindowsMediaControls.CachedInfo.playbackState != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing) ? Resources.GetFAV("play") : Resources.GetFAV("pause"), delegate
		{
			WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.PlayPauseToggle);
		}, () => new Vector2(0f, -15f), () => new Vector2(37.5f, 37.5f));
		playPause.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		forward = new FAVControlButton(() => Resources.GetFAV("forward"), delegate
		{
			WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.SkipNext);
		}, () => new Vector2(60f, -15f), () => new Vector2(35f, 35f));
		forward.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		backward = new FAVControlButton(() => Resources.GetFAV("rewind"), delegate
		{
			WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.SkipPrevious);
		}, () => new Vector2(-60f, -15f), () => new Vector2(35f, 35f));
		backward.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		listView = new ControlButton(() => Resources.GetImage("list"), 0f, delegate
		{
			Notch.MainNotch.SetTempView(new MusicFavoritesView(Notch.MainNotch));
		}, () => new Vector2(120f, -15f), () => new Vector2(30f, 30f));
		listView.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		loop = new ControlButton(() => Resources.GetImage("lp"), 0f, delegate
		{
			WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.SwapLoopMode);
		}, () => new Vector2(120f, -15f), () => new Vector2(25f, 25f));
		loop.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		shuffle = new ControlButton(() => Resources.GetImage("shf"), 0f, delegate
		{
			WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.ToggleShuffle);
		}, () => new Vector2(-120f, -15f), () => new Vector2(25f, 25f));
		shuffle.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		progress = new FSlider(() => new Vector2(0f, 13f), 230f);
		InteractiveSurface interactiveSurface = progress.InteractiveSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, (Action<MouseInputCode>)delegate(MouseInputCode x)
		{
			if (x.button == MouseInputButton.Left)
			{
				timelineValueLocked = true;
			}
			if (x.state == MouseInputState.Up)
			{
				UserScrubTimeline(progress.Value);
				FContext.GetCurrentDispatcher().InvokeLater(delegate
				{
					timelineValueLocked = false;
				}, 1f);
			}
		});
		progress.KnobSize = Vector2.Zero;
		progress.BarFill.SetResponsiveState(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface.Multiply(1.5f));
		progress.Bar.SetResponsiveState(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface.WithAlpha(50));
		progress.BarBorder.SetStaticState(SKColors.Transparent);
		progress.Value = 0f;
		progress.BarHeight = 5f;
		timeDuration = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(10f, -1f), () => new Vector2(50f, 25f));
		timeDuration.SetParent(progress);
		timeDuration.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		timeDuration.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		timeStillLeft = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(-10f, -1f), () => new Vector2(50f, 25f));
		timeStillLeft.SetParent(progress);
		timeStillLeft.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		timeStillLeft.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		list.Add(timeDuration);
		list.Add(timeStillLeft);
		list.Add(progress);
		if (loop != null)
		{
			list.Add(loop);
		}
		if (listView != null)
		{
			list.Add(listView);
		}
		list.Add(shuffle);
		list.Add(title);
		list.Add(artist);
		list.Add(playPause);
		list.Add(forward);
		list.Add(backward);
		list.Add(visualizer);
		list.Add(songCover);
		isInitialized = true;
		OnMediaUpdate();
		Settings.SetListenerForSettingChange("media.show-visualizer", delegate
		{
			base.RootNotch.Invalidate(UIObject.Invalidation.SurfaceDirty);
		});
		return list;
	}

	private void UserScrubTimeline(float value)
	{
		WindowsMediaControls.SetTimelineTime(TimeSpan.FromSeconds(value));
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		Settings.RemoveListenerForSettingChange("media.show-visualizer", delegate
		{
			base.RootNotch.Invalidate(UIObject.Invalidation.SurfaceDirty);
		});
		lastTitle = "";
		lastArtist = "";
		lastDuration = default(TimeSpan);
		lastTimeStillLeft = default(TimeSpan);
	}

	private void UpdateText()
	{
		if (lastTitle != WindowsMediaControls.CachedInfo.title)
		{
			title.Model = TextModelFactory.CreateBasic(WindowsMediaControls.CachedInfo.title, 14f, bold: true, italic: false, underlined: false, () => new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue), null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.Start
			});
		}
		if (lastArtist != WindowsMediaControls.CachedInfo.artist)
		{
			artist.Model = TextModelFactory.CreateBasic(WindowsMediaControls.CachedInfo.artist, 12f, bold: false, italic: false, underlined: false, () => new SKColor(100, 100, 100), null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.Start
			});
		}
		lastTitle = WindowsMediaControls.CachedInfo.title;
		lastArtist = WindowsMediaControls.CachedInfo.artist;
	}

	public override void Update()
	{
		base.Update();
		if (visualizer == null)
		{
			return;
		}
		visualizer.Layout.Alignment.SetStaticState(new Vector2(1f, 0f));
		songCover.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		State<Vector2> size = songCover.Transform.Size;
		songCover.Transform.Size.SetStaticState(Vector2.Lerp(new Vector2(15f, 15f), new Vector2(55f, 55f), base.RootNotch.IsBigNotch));
		songCover.CornerRadius.SetStaticState(RMath.Lerp(12f, 40f, base.RootNotch.IsBigNotch));
		songCover.Transform.LocalPosition.SetStaticState(new Vector2(songCover.Transform.Size.CachedValue.x / 2f, songCover.Transform.Size.CachedValue.y / 2f));
		visualizer.Transform.Scale.SetStaticState(Vector2.One * RMath.Lerp(0.85f, 1f, base.RootNotch.IsBigNotch));
		visualizer.Transform.LocalPosition.SetStaticState(new Vector2((0f - songCover.Transform.Size.CachedValue.x) / 2f - 5f, songCover.Transform.Size.CachedValue.y / 2f));
		if (loop != null)
		{
			SKImage sKImage = ((WindowsMediaControls.CachedInfo.repeatMode == MediaPlaybackAutoRepeatMode.Track) ? Resources.GetImage("lp1") : Resources.GetImage("lp"));
			if (sKImage != loop.Image.CachedValue)
			{
				loop.Image.SetStaticState(sKImage);
				loop.Invalidate(UIObject.Invalidation.SurfaceDirty);
			}
		}
		float num = (float)Math.Pow(base.RootNotch.IsBigNotch, 6.0);
		title.ImageEffects.Opacity.SetStaticState(num);
		artist.ImageEffects.Opacity.SetStaticState(num);
		playPause.ImageEffects.Opacity.SetStaticState(num);
		forward.ImageEffects.Opacity.SetStaticState(num);
		backward.ImageEffects.Opacity.SetStaticState(num);
		progress.ImageEffects.Opacity.SetStaticState(num);
		timeDuration.ImageEffects.Opacity.SetStaticState(num);
		timeStillLeft.ImageEffects.Opacity.SetStaticState(num);
		shuffle.ImageEffects.Opacity.SetStaticState(num / (float)((WindowsMediaControls.CachedInfo.isShuffling != false) ? 1 : 3));
		if (loop != null)
		{
			loop.ImageEffects.Opacity.SetStaticState(num / (float)((WindowsMediaControls.CachedInfo.repeatMode != MediaPlaybackAutoRepeatMode.None) ? 1 : 3));
		}
		if (listView != null)
		{
			listView.ImageEffects.Opacity.SetStaticState(num / 3f);
		}
		songCover.Enabled.SetStaticState(WindowsMediaControls.IsMediaActive());
		visualizer.Enabled.SetStaticState(WindowsMediaControls.IsMediaActive() && Settings.GetSetting("media.show-visualizer", defaultValue: true));
		lastBigNotch = RMath.Lerp(lastBigNotch, base.RootNotch.IsBigNotch, FContext.DeltaTime * 15f);
		playPause.InteractiveSurface.IgnoreInteractions.SetStaticState(lastBigNotch < 0.9f, 100u);
		forward.InteractiveSurface.IgnoreInteractions.SetStaticState(lastBigNotch < 0.9f, 100u);
		backward.InteractiveSurface.IgnoreInteractions.SetStaticState(lastBigNotch < 0.9f, 100u);
		shuffle.InteractiveSurface.IgnoreInteractions.SetStaticState(lastBigNotch < 0.9f, 100u);
		if (loop != null)
		{
			loop.InteractiveSurface.IgnoreInteractions.SetStaticState(!(base.RootNotch.IsBigNotch > 0.9f));
		}
		if (listView != null)
		{
			listView.InteractiveSurface.IgnoreInteractions.SetStaticState(!(base.RootNotch.IsBigNotch > 0.9f));
		}
		bool flag = base.RootNotch.IsBigNotch > 0.1f && WindowsMediaControls.IsMediaActive();
		artist.Enabled.SetStaticState(flag);
		playPause.Enabled.SetStaticState(flag);
		forward.Enabled.SetStaticState(flag);
		backward.Enabled.SetStaticState(flag);
		shuffle.Enabled.SetStaticState(flag);
		if (loop != null)
		{
			loop.Enabled.SetStaticState(flag && Settings.GetSetting("media.show-loop-button", defaultValue: false));
		}
		if (listView != null)
		{
			listView.Enabled.SetStaticState(flag && !Settings.GetSetting("media.show-loop-button", defaultValue: false));
		}
		timeDuration.Enabled.SetStaticState(flag);
		timeStillLeft.Enabled.SetStaticState(flag);
		progress.Enabled.SetStaticState(flag);
		if (WindowsMediaControls.CachedInfo.playbackState == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
		{
			time_FakeAddition += FContext.DeltaTime;
		}
		if (base.RootNotch.IsBigNotch > 0.5f)
		{
			UpdateTime();
		}
		if (size != songCover.Transform.Size)
		{
			songCover.Invalidate(UIObject.Invalidation.SurfaceDirty);
		}
	}
}
