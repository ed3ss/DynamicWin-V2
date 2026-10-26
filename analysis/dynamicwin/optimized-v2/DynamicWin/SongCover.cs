using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp;
using FenUISharp.AnimatedVectors;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.WinFeatures;
using SkiaSharp;
using Windows.Media.Control;

namespace DynamicWin;

public class SongCover : FImage
{
	private AnimatorComponent coverAnimator;

	private FImage favoriteImage;

	private AnimatorComponent interactAnimator;

	private AnimatorComponent favoriteAnimator;

	private AnimatorComponent keybindAnimator;

	private bool _isSongFavorite;

	private Dispatcher? dispatcher;

	private (SKColor, SKColor) cachedPrimaries = (FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary, SKColors.White);

	public SongCover(Func<SKImage> image, bool drawBackground = false, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(image, drawBackground, dynamicColor: false, position, size)
	{
		dispatcher = FContext.GetCurrentDispatcher();
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: false);
		coverAnimator = new AnimatorComponent(this, Easing.EaseOutCubic, Easing.EaseOutCubic);
		coverAnimator.Duration = 0.85f;
		Rotation3DTransformComponent rotationTransform = new Rotation3DTransformComponent(this);
		AnimatorComponent animatorComponent = coverAnimator;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
		{
			if (Settings.GetSetting("media.disable-album-flip", defaultValue: false))
			{
				base.Image.SetStaticState(WindowsMediaControls.CachedInfo.thumbnail ?? Resources.GetImage("cover-plc"));
				Invalidate(Invalidation.SurfaceDirty);
				coverAnimator.Break();
			}
			else
			{
				float value = 0.6f;
				base.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap((float)Math.Sin(Math.PI * (double)x), 0f, 1f, 1f, 0.84f));
				float num = 180f;
				if (x >= 0.5f)
				{
					base.Image.SetStaticState(WindowsMediaControls.CachedInfo.thumbnail ?? Resources.GetImage("cover-plc"));
					Invalidate(Invalidation.SurfaceDirty);
					rotationTransform.RotationY.SetStaticState(x * num - num);
					rotationTransform.DepthScale.SetStaticState(value);
				}
				else
				{
					rotationTransform.RotationY.SetStaticState(x * num);
					rotationTransform.DepthScale.SetStaticState(value);
				}
			}
		});
		AnimatorComponent animatorComponent2 = coverAnimator;
		animatorComponent2.OnComplete = (Action)Delegate.Combine(animatorComponent2.OnComplete, (Action)delegate
		{
			rotationTransform.RotationY.SetStaticState(0f);
			rotationTransform.DepthScale.SetStaticState(1f);
		});
		WindowsMediaControls.onThumbnailUpdated = (Action)Delegate.Combine(WindowsMediaControls.onThumbnailUpdated, new Action(OnThumbnailUpdate));
		OnThumbnailUpdate();
		favoriteImage = new FImage(() => Resources.GetImage("favorite-empty"), drawBackground: false, dynamicColor: false, () => Vector2.Zero, () => new Vector2(30f, 30f));
		favoriteImage.SetParent(this);
		favoriteImage.Transform.Scale.SetStaticState(Vector2.One * 0.9f);
		favoriteImage.Layout.StretchHorizontal.SetStaticState(value: true);
		favoriteImage.Layout.StretchVertical.SetStaticState(value: true);
		favoriteImage.Layout.MarginHorizontal.SetResponsiveState(() => (!(Notch.MainNotch.IsBigNotch > 0.5f)) ? 0f : 12f);
		favoriteImage.Layout.MarginVertical.SetResponsiveState(() => (!(Notch.MainNotch.IsBigNotch > 0.5f)) ? 0f : 12f);
		favoriteImage.ImageEffects.Opacity.SetStaticState(0f);
		favoriteImage.ImageEffects.InheritFromParent.SetStaticState(value: false);
		interactAnimator = new AnimatorComponent(this, Easing.EaseOutCubic, Easing.EaseOutCubic);
		interactAnimator.Duration = 0.3f;
		AnimatorComponent animatorComponent3 = interactAnimator;
		animatorComponent3.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent3.OnValueUpdate, (Action<float>)delegate(float x)
		{
			base.ImageEffects.Opacity.SetStaticState(RMath.Remap(x, 0f, 1f, 1f, 0.4f));
			base.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(x, 0f, 1f, 1f, 0.95f));
			favoriteImage.ImageEffects.Opacity.SetStaticState(RMath.Remap(x, 0f, 1f, 0f, 1f));
			favoriteImage.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(x, 0f, 1f, 0.7f, 1f));
		});
		favoriteAnimator = new AnimatorComponent(this, (float x) => x, (float x) => x);
		AnimatorComponent animatorComponent4 = favoriteAnimator;
		animatorComponent4.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent4.OnValueUpdate, (Action<float>)delegate(float x)
		{
			if (_isSongFavorite)
			{
				float x2 = (float)Math.Sin((double)(2f * x) * Math.PI + Math.PI);
				float num = RMath.Remap(RMath.Clamp(MathF.Pow(MathF.Abs(x2), 0.36f) * (float)MathF.Sign(x2), -1f, 1f), -1f, 1f, 0.8f, 1.1f);
				favoriteImage.Transform.Scale.SetStaticState(Vector2.One * num);
			}
			else
			{
				favoriteImage.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(Easing.EaseOutCubic(x), 0f, 1f, 1f, 0.9f));
			}
		});
		RecalcTwoCols();
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface.OnMouseEnter, new Action(MouseEnter));
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnMouseExit = (Action)Delegate.Combine(interactiveSurface2.OnMouseExit, new Action(MouseExit));
		InteractiveSurface interactiveSurface3 = base.InteractiveSurface;
		interactiveSurface3.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface3.OnMouseAction, new Action<MouseInputCode>(MouseAction));
		FAVDisplay centerImage = new FAVDisplay(() => Resources.GetFAV("play"));
		centerImage.SetParent(this);
		centerImage.Layout.StretchHorizontal.SetStaticState(value: true);
		centerImage.Layout.StretchVertical.SetStaticState(value: true);
		centerImage.ImageEffects.InheritFromParent.SetStaticState(value: false);
		centerImage.ImageEffects.Opacity.SetStaticState(0f, 5u);
		keybindAnimator = new AnimatorComponent(this, (float x) => x);
		keybindAnimator.Duration = 1f;
		AnimatorComponent animatorComponent5 = keybindAnimator;
		animatorComponent5.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent5.OnValueUpdate, (Action<float>)delegate(float x)
		{
			float num = ((x < 0.5f) ? Easing.EaseOutBack(MathF.Sin(x * (float)Math.PI)) : RMath.Remap(Easing.EaseOutBack((x - 0.5f) * 2f), 0f, 1f, 1f, 0f));
			centerImage.ImageEffects.Opacity.SetStaticState(num, 5u);
			centerImage.Transform.Scale.SetStaticState(Vector2.One * (num * 1.1f));
			base.ImageEffects.Opacity.SetStaticState(1f - num + 0.25f);
			base.Transform.Scale.SetStaticState(Vector2.One + Vector2.One * RMath.Remap(num, 1f, 0f, -0.2f, 0f));
		});
		KeyBind play = new KeyBind
		{
			VKCode = 179,
			IgnoreFocus = true,
			OnKeybindExecuted = delegate
			{
				if (!((double)Notch.MainNotch.IsBigNotch > 0.1))
				{
					centerImage.SilentSetFAV((WindowsMediaControls.CachedInfo.playbackState == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing) ? Resources.GetFAV("play") : Resources.GetFAV("pause"));
					centerImage.PlayAnimation("action");
					keybindAnimator.Restart();
				}
			}
		};
		KeyBind skipFwd = new KeyBind
		{
			VKCode = 176,
			IgnoreFocus = true,
			OnKeybindExecuted = delegate
			{
				if (!((double)Notch.MainNotch.IsBigNotch > 0.1))
				{
					centerImage.SilentSetFAV(Resources.GetFAV("forward"));
					centerImage.PlayAnimation("action");
					keybindAnimator.Restart();
				}
			}
		};
		KeyBind skipRwd = new KeyBind
		{
			VKCode = 177,
			IgnoreFocus = true,
			OnKeybindExecuted = delegate
			{
				if (!((double)Notch.MainNotch.IsBigNotch > 0.1))
				{
					centerImage.SilentSetFAV(Resources.GetFAV("rewind"));
					centerImage.PlayAnimation("action");
					keybindAnimator.Restart();
				}
			}
		};
		FContext.GetCurrentWindow()?.WindowKeyboardInput?.RegisterKeybind(play);
		FContext.GetCurrentWindow()?.WindowKeyboardInput?.RegisterKeybind(skipFwd);
		FContext.GetCurrentWindow()?.WindowKeyboardInput?.RegisterKeybind(skipRwd);
		base.OnObjectDisposed = (Action)Delegate.Combine(base.OnObjectDisposed, (Action)delegate
		{
			FContext.GetCurrentWindow()?.WindowKeyboardInput?.UnregisterKeybind(play);
			FContext.GetCurrentWindow()?.WindowKeyboardInput?.UnregisterKeybind(skipFwd);
			FContext.GetCurrentWindow()?.WindowKeyboardInput?.UnregisterKeybind(skipRwd);
		});
		_isSongFavorite = MusicFavoritesView.IsSongInFavorites(WindowsMediaControls.CachedInfo.title, WindowsMediaControls.CachedInfo.artist);
		UpdateIsFavorite();
	}

	protected void MouseEnter()
	{
		interactAnimator.Inverse = false;
		interactAnimator.Restart();
	}

	protected void MouseExit()
	{
		if (!interactAnimator.Inverse)
		{
			interactAnimator.Inverse = true;
			interactAnimator.Restart();
		}
	}

	protected void MouseAction(MouseInputCode inputCode)
	{
		if (inputCode.state == MouseInputState.Down && inputCode.button == MouseInputButton.Left)
		{
			_isSongFavorite = !_isSongFavorite;
			if (_isSongFavorite)
			{
				MusicFavoritesView.AddSongToFavorites(WindowsMediaControls.CachedInfo);
			}
			else
			{
				MusicFavoritesView.RemoveSongFromFavorites(WindowsMediaControls.CachedInfo.title, WindowsMediaControls.CachedInfo.artist);
			}
			favoriteAnimator.Restart();
			favoriteAnimator.Duration = (_isSongFavorite ? 0.6f : 0.2f);
		}
		UpdateIsFavorite();
	}

	private void UpdateIsFavorite()
	{
		if (favoriteImage != null)
		{
			dispatcher?.Invoke(delegate
			{
				UpdateFavoriteContent();
			});
		}
	}

	private void UpdateFavoriteContent()
	{
		favoriteImage.Image.SetStaticState(_isSongFavorite ? Resources.GetImage("favorite-filled") : Resources.GetImage("favorite-empty"));
		favoriteImage.TintColor.SetStaticState(_isSongFavorite ? FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Error : FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface);
		favoriteImage.Invalidate(Invalidation.SurfaceDirty);
	}

	public override void Dispose()
	{
		WindowsMediaControls.onThumbnailUpdated = (Action)Delegate.Remove(WindowsMediaControls.onThumbnailUpdated, new Action(OnThumbnailUpdate));
		base.Dispose();
	}

	private void OnThumbnailUpdate()
	{
		coverAnimator.Restart();
		_isSongFavorite = MusicFavoritesView.IsSongInFavorites(WindowsMediaControls.CachedInfo.title, WindowsMediaControls.CachedInfo.artist);
		UpdateIsFavorite();
		cachedPrimaries = RecalcTwoCols();
	}

	public (SKColor, SKColor) GetTopTwoColors()
	{
		return cachedPrimaries;
	}

	private (SKColor, SKColor) RecalcTwoCols(int downscaleSize = 24)
	{
		if (WindowsMediaControls.CachedInfo.thumbnail == null)
		{
			return cachedPrimaries;
		}
		using SKImage image = SKImage.FromBitmap(SKBitmap.FromImage(WindowsMediaControls.CachedInfo.thumbnail).Resize(new SKImageInfo(downscaleSize, downscaleSize), new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Nearest)));
		SKBitmap sKBitmap = SKBitmap.FromImage(image);
		Dictionary<SKColor, float> dictionary = new Dictionary<SKColor, float>();
		for (int i = 0; i < sKBitmap.Height; i++)
		{
			for (int j = 0; j < sKBitmap.Width; j++)
			{
				SKColor pixel = sKBitmap.GetPixel(j, i);
				if (pixel.Alpha != 0)
				{
					SKColor key = ReduceColor(pixel, 32);
					key.ToHsl(out var _, out var s, out var l);
					float num = s * l;
					if (dictionary.TryGetValue(key, out var value))
					{
						dictionary[key] = value + num;
					}
					else
					{
						dictionary[key] = num;
					}
				}
			}
		}
		SKColor[] source = (from kvp in dictionary.OrderByDescending((KeyValuePair<SKColor, float> kvp) => kvp.Value).Take(2)
			select kvp.Key).ToArray();
		SKColor col = source.ElementAtOrDefault(0);
		SKColor col2 = source.ElementAtOrDefault(1);
		col = BoostVibrancy(col);
		col2 = BoostVibrancy(col2);
		col.ToHsl(out var h2, out var s2, out var l2);
		col2.ToHsl(out var h3, out var s3, out var l3);
		float num2 = (RMath.Approximately(l2, l3, -1.25f) ? 0.5f : 0.05f);
		float num3 = ((MathF.Min(l2, l3) < 40f) ? 0.5f : 0f);
		SKColor item = ((l2 < l3) ? SKColor.FromHsl(h2, s2, Math.Clamp(l2 * (1f - num2 + num3), 0f, 100f)) : SKColor.FromHsl(h3, s3, Math.Clamp(l3 * (1f - num2 + num3), 0f, 100f)));
		SKColor item2 = ((l2 < l3) ? SKColor.FromHsl(h3, s3, Math.Clamp(l3 * (1f + num2 + num3), 0f, 100f)) : SKColor.FromHsl(h2, s2, Math.Clamp(l2 * (1f + num2 + num3), 0f, 100f)));
		return (item, item2);
	}

	private static SKColor BoostVibrancy(SKColor col)
	{
		col.ToHsl(out var h, out var s, out var l);
		s = Math.Max(s, 0.5f);
		l = Math.Max(l, 0.5f);
		return SKColor.FromHsl(h, s, l);
	}

	private static SKColor ReduceColor(SKColor color, int quantizeLevel)
	{
		byte red = (byte)RMath.Clamp(10f + (float)(color.Red / quantizeLevel * quantizeLevel) * 1.25f, 0f, 255f);
		byte green = (byte)RMath.Clamp(10f + (float)(color.Green / quantizeLevel * quantizeLevel) * 1.25f, 0f, 255f);
		byte blue = (byte)RMath.Clamp(10f + (float)(color.Blue / quantizeLevel * quantizeLevel) * 1.25f, 0f, 255f);
		return new SKColor(red, green, blue);
	}
}
