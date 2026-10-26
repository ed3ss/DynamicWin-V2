using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FenUISharp;
using FenUISharp.Logging;
using SkiaSharp;
using ThumbnailGenerator;

namespace DynamicWin;

public static class ThumbnailProviderBridge
{
	private static Dictionary<string, SKImage> CachedThumbnails { get; set; } = new Dictionary<string, SKImage>();

	public static void FetchThumbnail(string filePath)
	{
		if (CachedThumbnails.ContainsKey(filePath))
		{
			return;
		}
		int tries = 0;
		bool success = false;
		Bitmap thumbnail = null;
		Task.Run(async delegate
		{
			while (!success && tries < 10)
			{
				tries++;
				try
				{
					int num = 64;
					thumbnail = WindowsThumbnailProvider.GetThumbnail(filePath, num, num, ThumbnailOptions.BiggerSizeOk);
				}
				catch (Exception ex)
				{
					_ = ex;
					FLogger.Error("Failed to fetch icon from file " + filePath);
					await Task.Delay(350);
					continue;
				}
				success = true;
			}
			if (thumbnail != null)
			{
				CachedThumbnails.Add(filePath, ToSKBitmap(thumbnail));
			}
		});
	}

	public static SKImage? GetFileThumbnail(string filePath)
	{
		if (CachedThumbnails.ToDictionary().ContainsKey(filePath))
		{
			return CachedThumbnails[filePath];
		}
		return Resources.GetImage("file");
	}

	private static SKImage ToSKBitmap(Bitmap bmp)
	{
		using MemoryStream memoryStream = new MemoryStream();
		bmp.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		return SKImage.FromBitmap(SKBitmap.Decode(memoryStream));
	}
}
