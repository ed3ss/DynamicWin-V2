using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DynamicWin.Cryptography;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.WinFeatures;
using SkiaSharp;

namespace DynamicWin;

public class MusicFavoritesView : NotchView
{
	private const char SEPARATOR = '@';

	private List<(string title, string artist, SKImage? thumbnail)> songList = new List<(string, string, SKImage)>();

	private const string IMAGE_EXTENSION = ".png";

	private const string TEXT_EXTENSION = ".txt";

	public static string FavoritesLocation => Path.Combine(DynamicWin.AppDataPath, "Favorites");

	public override float ExpandedCornerRadius => 60f;

	public override Vector2 ExpandedIslandSize => new Vector2(351f, 170f);

	public MusicFavoritesView(Notch island)
		: base(island)
	{
	}

	public override void Update()
	{
		base.Update();
		if (base.RootNotch.IsBigNotch < 0.9f)
		{
			base.RootNotch.DissolveTempView();
		}
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		if (songList == null)
		{
			return;
		}
		foreach (var song in songList)
		{
			song.thumbnail?.Dispose();
		}
	}

	public override List<UIObject> Create()
	{
		List<UIObject> list = new List<UIObject>();
		songList = GetFavoritesList();
		FPanel panel = new FPanel(() => new Vector2(0f, -10f), () => Vector2.Zero, 10f, () => SKColors.Transparent);
		panel.Layout.StretchHorizontal.SetStaticState(value: true);
		panel.Layout.StretchVertical.SetStaticState(value: true);
		panel.Layout.MarginHorizontal.SetStaticState(-5f);
		panel.Layout.MarginVertical.SetStaticState(-10f);
		FPanel owner = panel;
		Vector2? alignInside = new Vector2(0.5f, 0f);
		StackContentComponent stackContentComponent = new StackContentComponent(owner, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.Scroll, null, alignInside);
		panel.Layout.RecursivelyUpdateLayout();
		stackContentComponent.Pad.SetStaticState(0f);
		FText fText = new FText(TextModelFactory.CreateBasic("Favorites", 14f, bold: true, italic: false, underlined: false, () => new SKColor(100, 100, 100), null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.End
		}), () => Vector2.Zero, () => new Vector2(0f, 35f));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.SetParent(panel);
		ControlButton controlButton = new ControlButton(() => Resources.GetImage("arrow-left"), 0f, delegate
		{
			Notch.MainNotch.DissolveTempView();
		}, () => Vector2.Zero, () => new Vector2(15f, 15f));
		controlButton.SetParent(fText);
		controlButton.Layout.Alignment.SetStaticState(new Vector2(1f, 1f));
		controlButton.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 1f));
		controlButton.TintColor.SetStaticState(new SKColor(100, 100, 100));
		foreach (var song in songList)
		{
			FPanel fPanel = new FPanel(() => new Vector2(0f, 0f), () => new Vector2(0f, 80f), 20f, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface.WithAlpha(25));
			fPanel.Layout.StretchHorizontal.SetStaticState(value: true);
			fPanel.Layout.MarginHorizontal.SetStaticState(0f);
			fPanel.SetParent(panel);
			fPanel.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
			FImage fImage = new FImage(() => song.thumbnail ?? Resources.GetImage("cover-plc"), drawBackground: false, dynamicColor: false, () => new Vector2(15f, 0f), () => new Vector2(55f, 55f));
			fImage.CornerRadius.SetStaticState(40f);
			fImage.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
			fImage.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
			fImage.UseSquircle.SetStaticState(value: true);
			fImage.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
			FText fText2 = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(20f, -10f), () => new Vector2(165f, 18f));
			fText2.SetParent(fImage);
			fText2.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
			fText2.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
			fText2.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
			FText fText3 = new FText(TextModelFactory.CreateBasic(""), () => new Vector2(20f, 10f), () => new Vector2(165f, 18f));
			fText3.SetParent(fImage);
			fText3.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
			fText3.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
			fText3.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
			fText2.DisableWhenOutOfParentBounds = false;
			fText2.Model = TextModelFactory.CreateBasic(song.title, 14f, bold: true, italic: false, underlined: false, () => new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue), null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.Start
			});
			fText3.DisableWhenOutOfParentBounds = false;
			fText3.Model = TextModelFactory.CreateBasic(song.artist, 12f, bold: false, italic: false, underlined: false, () => new SKColor(100, 100, 100), null, new TextAlign
			{
				HorizontalAlign = TextAlign.AlignType.Start
			});
			FPanel currentSubPanel = fPanel;
			ControlButton controlButton2 = new ControlButton(() => Resources.GetImage("delete"), 0f, delegate
			{
				RemoveSongFromFavorites(song.title, song.artist);
				FContext.GetCurrentDispatcher().Invoke(delegate
				{
					currentSubPanel.Dispose();
					panel.RecursiveInvalidate(UIObject.Invalidation.SurfaceDirty);
					panel.Layout.RecursivelyUpdateLayout();
				});
			}, () => new Vector2(-15f, 0f), () => new Vector2(20f, 20f));
			controlButton2.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
			controlButton2.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
			controlButton2.TintColor.SetStaticState(FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(75));
			controlButton2.SetParent(fPanel);
			ControlButton controlButton3 = new ControlButton(() => Resources.GetImage("external"), 0f, delegate
			{
				string text = WebUtility.UrlEncode(song.title + " by " + song.artist);
				DynamicWin.OpenUrl("https://www.google.com/search?q=" + text);
			}, () => new Vector2(-45f, 0f), () => new Vector2(20f, 20f));
			controlButton3.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
			controlButton3.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
			controlButton3.TintColor.SetStaticState(FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface.WithAlpha(75));
			controlButton3.SetParent(fPanel);
			fImage.SetParent(fPanel);
		}
		if (songList.Count == 0)
		{
			FText fText4 = new FText(TextModelFactory.CreateBasic("Click on the cover of a playing song to add it to your favorites", 14.5f, bold: false, italic: false, underlined: false, () => new SKColor(175, 175, 175)), () => Vector2.Zero, () => new Vector2(0f, 100f));
			fText4.Layout.StretchHorizontal.SetStaticState(value: true);
			fText4.Layout.MarginHorizontal.SetStaticState(25f);
			fText4.SetParent(panel);
		}
		list.Add(panel);
		stackContentComponent.ContentFade = true;
		return list;
	}

	public static void CheckDir()
	{
		if (!Directory.Exists(FavoritesLocation))
		{
			Directory.CreateDirectory(FavoritesLocation);
		}
	}

	public static void AddSongToFavorites(PlaybackInfo info)
	{
		CheckDir();
		string filenameForSong = GetFilenameForSong(info.title, info.artist);
		string text = Path.Combine(FavoritesLocation, filenameForSong);
		if (info.thumbnail != null)
		{
			using SKData sKData = info.thumbnail.Encode(SKEncodedImageFormat.Png, 80);
			using FileStream target = File.OpenWrite(text + ".png");
			sKData.SaveTo(target);
		}
		File.Create(text + ".txt").Dispose();
		File.WriteAllText(text + ".txt", info.title + "\r\n" + info.artist);
	}

	public static void RemoveSongFromFavorites(string title, string artist)
	{
		CheckDir();
		string filenameForSong = GetFilenameForSong(title, artist);
		string[] files = Directory.GetFiles(FavoritesLocation);
		foreach (string text in files)
		{
			if (text.Contains(filenameForSong))
			{
				File.Delete(text);
			}
		}
	}

	public static bool IsSongInFavorites(string title, string artist)
	{
		CheckDir();
		string filenameForSong = GetFilenameForSong(title, artist);
		string[] files = Directory.GetFiles(FavoritesLocation);
		for (int i = 0; i < files.Length; i++)
		{
			if (files[i].Contains(filenameForSong))
			{
				return true;
			}
		}
		return false;
	}

	public static string GetFilenameForSong(string title, string artist)
	{
		return Hashing.GetHashString(title + artist);
	}

	public static List<(string title, string artist, SKImage? thumbnail)> GetFavoritesList()
	{
		CheckDir();
		List<(string, string, SKImage)> list = new List<(string, string, SKImage)>();
		string[] array = (from x in Directory.GetFiles(FavoritesLocation)
			orderby File.GetLastWriteTime(x) descending
			select x).ToArray();
		foreach (string text in array)
		{
			string item = "";
			string item2 = "";
			SKImage item3 = null;
			if (!text.Contains(".txt"))
			{
				continue;
			}
			try
			{
				string[] array2 = File.ReadAllLines(text);
				if (array2.Length < 2)
				{
					continue;
				}
				item = array2[0];
				item2 = array2[1];
				goto IL_009a;
			}
			catch (Exception)
			{
				FLogger.Error("Error trying to read favorite file " + text);
				goto IL_009a;
			}
			IL_009a:
			string text2 = Path.Combine(FavoritesLocation, Path.GetFileNameWithoutExtension(text) + ".png");
			if (File.Exists(text2))
			{
				item3 = SKImage.FromEncodedData(text2);
			}
			Console.WriteLine(text2);
			list.Add((item, item2, item3));
		}
		return list;
	}
}
