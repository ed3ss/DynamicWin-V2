using System;
using System.IO;
using System.Threading;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using FenUISharp.Objects;
using SkiaSharp;

public class SimpleVideoPlayer : FPanel
{
	private Thread videoThread;

	private SKBitmap? bitmap;

	private bool _isPlaying;

	public SimpleVideoPlayer(Stream stream)
	{
		SimpleVideoPlayer simpleVideoPlayer = this;
		_drawBasePanel = false;
		videoThread = new Thread((ThreadStart)delegate
		{
			using MediaFile mediaFile = MediaFile.Open(stream, new MediaOptions
			{
				StreamsToLoad = MediaMode.Video,
				VideoPixelFormat = ImagePixelFormat.Argb32
			});
			simpleVideoPlayer.bitmap = new SKBitmap(mediaFile.Video.Info.FrameSize.Width, mediaFile.Video.Info.FrameSize.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
			Span<byte> pixelSpan = simpleVideoPlayer.bitmap.GetPixelSpan();
			double avgFrameRate = mediaFile.Video.Info.AvgFrameRate;
			double num = 1.0 / avgFrameRate;
			DateTime now = DateTime.Now;
			do
			{
				try
				{
					while ((DateTime.Now - now).TotalSeconds < num || !simpleVideoPlayer._isPlaying)
					{
						if (!simpleVideoPlayer._isPlaying)
						{
							now = DateTime.Now;
						}
						Thread.Sleep(1);
					}
					simpleVideoPlayer.Invalidate(Invalidation.SurfaceDirty);
					now = DateTime.Now;
				}
				catch (ThreadInterruptedException)
				{
					break;
				}
			}
			while (mediaFile.Video.TryGetNextFrame(pixelSpan));
		});
		videoThread.IsBackground = true;
		videoThread.Start();
	}

	public void Play()
	{
		_isPlaying = true;
	}

	public void Stop()
	{
		_isPlaying = false;
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		canvas.ClipPath(GetPanelPath());
		using SKPaint paint = GetRenderPaint();
		if (bitmap != null)
		{
			canvas.DrawBitmap(bitmap, base.Shape.LocalBounds, paint);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		videoThread.Interrupt();
		videoThread = null;
		bitmap?.Dispose();
		bitmap = null;
	}
}
