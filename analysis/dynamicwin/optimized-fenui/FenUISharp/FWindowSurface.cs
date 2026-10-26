using System;
using System.Collections.Generic;
using System.Linq;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using SkiaSharp;

namespace FenUISharp;

public class FWindowSurface : IDisposable
{
	private readonly object _renderLock = new object();

	private SKPath? _cachedDirtyPath;

	private SKPath? _lastDirtyPath;

	private WeakReference<FWindow> window { get; set; }

	public FWindow Window
	{
		get
		{
			if (!window.TryGetTarget(out FWindow target))
			{
				throw new Exception("Window not set.");
			}
			return target;
		}
	}

	public Func<SKColor> ClearColor { get; set; }

	internal ModelViewPane? RootViewPane { get; private set; }

	public FWindowSurface(FWindow window)
	{
		this.window = new WeakReference<FWindow>(window);
		ClearColor = delegate
		{
			if (!Window.Properties.UseSystemDarkMode)
			{
				return SKColor.Parse("#f3f3f3");
			}
			return Window.Properties._isFocused ? SKColor.Parse("#211f23") : SKColor.Parse("#202020");
		};
	}

	public virtual void SetupSurface()
	{
		RootViewPane = new ModelViewPane(null);
		RootViewPane.Layout.Alignment.Value = () => new Vector2(0.5f, 0.5f);
		RootViewPane.Layout.StretchHorizontal.Value = () => true;
		RootViewPane.Layout.StretchVertical.Value = () => true;
		RootViewPane.DisableWhenOutOfParentBounds = false;
		FContext.WithRootViewPane(RootViewPane);
	}

	public void Dispose()
	{
		RootViewPane?.Dispose();
		RootViewPane = null;
	}

	public void Draw(SKCanvas canvas)
	{
		lock (_renderLock)
		{
			int count = canvas.Save();
			using SKPath path = CalculateThisFramesDirtyClipPath();
			canvas.ClipPath(path);
			Window.ClearSurface(canvas);
			Window.DrawBackdrop(canvas);
			if (Window.SkiaDirectCompositionContext?.Surface != null)
			{
				Window.Callbacks.OnWindowBeforeDraw?.Invoke(Window.SkiaDirectCompositionContext.Surface);
			}
			RootViewPane?.DrawToSurface(canvas);
			if (Window.DebugDisplayBounds)
			{
				foreach (UIObject allUIObject in GetAllUIObjects())
				{
					if (allUIObject.RenderThisFrame())
					{
						using SKPaint sKPaint = new SKPaint
						{
							IsStroke = true,
							StrokeWidth = 1f,
							Color = SKColors.Blue
						};
						SKRect globalBounds = allUIObject.Shape.GlobalBounds;
						globalBounds.Inflate(2f, 2f);
						canvas.DrawRect(globalBounds, sKPaint);
						canvas.DrawCircle(allUIObject.Transform.LocalToGlobal(allUIObject.Transform.Anchor.CachedValue).x, allUIObject.Transform.LocalToGlobal(allUIObject.Transform.Anchor.CachedValue).y, 2f, sKPaint);
						sKPaint.Color = SKColors.Yellow;
						allUIObject.InteractiveSurface.GlobalSurface.CachedValue.Inflate(1f, 1f);
						canvas.DrawRect(globalBounds, sKPaint);
						canvas.DrawCircle(allUIObject.Transform.LocalToGlobal(allUIObject.Layout.AlignmentAnchor.CachedValue).x, allUIObject.Transform.LocalToGlobal(allUIObject.Layout.AlignmentAnchor.CachedValue).y, 1f, sKPaint);
					}
				}
			}
			if (Window.DebugDisplayObjectIDs)
			{
				foreach (UIObject allUIObject2 in GetAllUIObjects())
				{
					if (allUIObject2.RenderThisFrame())
					{
						using SKPaint paint = new SKPaint
						{
							Color = SKColors.Magenta
						};
						canvas.DrawText("ID: " + allUIObject2.InstanceID, new SKPoint(allUIObject2.Shape.GlobalBounds.Left, allUIObject2.Shape.GlobalBounds.Top), SKTextAlign.Left, new SKFont(SKTypeface.Default, 10f), paint);
					}
				}
			}
			canvas.RestoreToCount(count);
			if (Window.DebugDisplayAreaCache)
			{
				using SKPaint sKPaint2 = new SKPaint
				{
					Color = SKColors.Red.WithAlpha(1)
				};
				canvas.DrawRect(SKRect.Create(0f, 0f, Window.Shape.ClientSize.x, Window.Shape.ClientSize.y), sKPaint2);
				sKPaint2.Color = SKColors.Yellow.WithAlpha(25);
				sKPaint2.IsStroke = true;
				sKPaint2.StrokeWidth = 2f;
				foreach (Func<SKRect> item in Window.Shape.GetWinRegion())
				{
					SKRect rect = item();
					rect.Inflate(-2f, -2f);
					canvas.DrawRect(rect, sKPaint2);
				}
			}
			if (Window.SkiaDirectCompositionContext?.Surface != null)
			{
				Window.Callbacks.OnWindowAfterDraw?.Invoke(Window.SkiaDirectCompositionContext.Surface);
			}
		}
	}

	public bool IsNextFrameRendering()
	{
		if (RootViewPane == null)
		{
			return false;
		}
		if (!GetAllUIObjects().Any((UIObject x) => x.WindowRedrawThisObject && x.Enabled.CachedValue && x.Visible.CachedValue) && !Window.DebugDisplayAreaCache && !Window._isDirty)
		{
			return Window._fullRedraw;
		}
		return true;
	}

	internal List<UIObject> GetAllUIObjects()
	{
		List<UIObject> result = new List<UIObject>();
		if (RootViewPane == null)
		{
			return result;
		}
		return RootViewPane.Composition.GetZOrderedListOfEverything();
	}

	public SKPath GetCurrentDirtyClipPath()
	{
		if (_cachedDirtyPath == null)
		{
			return new SKPath();
		}
		return new SKPath(_cachedDirtyPath);
	}

	private SKPath CalculateThisFramesDirtyClipPath()
	{
		_cachedDirtyPath?.Dispose();
		SKPath sKPath = new SKPath();
		if (Window._isDirty)
		{
			sKPath.AddRect(SKRect.Create(0f, 0f, Window.Shape.ClientSize.x, Window.Shape.ClientSize.y));
			_cachedDirtyPath = sKPath;
			return new SKPath(sKPath);
		}
		foreach (UIObject allUIObject in GetAllUIObjects())
		{
			if (allUIObject.WindowRedrawThisObject && allUIObject.GlobalEnabled && allUIObject.GlobalVisible)
			{
				int num = 4;
				SKRect globalBounds = allUIObject.Shape.GlobalBounds;
				globalBounds.Inflate(num, num);
				sKPath.AddRect(globalBounds);
				SKRect lastGlobalBounds = allUIObject.Shape.LastGlobalBounds;
				lastGlobalBounds.Inflate(num, num);
				sKPath.AddRect(lastGlobalBounds);
			}
			allUIObject.WindowRedrawThisObject = false;
		}
		SKPath sKPath2 = null;
		if (_lastDirtyPath != null)
		{
			sKPath2 = new SKPath(_lastDirtyPath);
		}
		_lastDirtyPath?.Dispose();
		_lastDirtyPath = new SKPath(sKPath);
		if (sKPath2 != null)
		{
			sKPath.AddPath(sKPath2);
		}
		if (_cachedDirtyPath != null)
		{
			_cachedDirtyPath.Dispose();
		}
		_cachedDirtyPath = new SKPath(sKPath);
		return sKPath;
	}

	internal bool MouseHitTest(Vector2 vector2)
	{
		List<UIObject> list = RootViewPane?.Composition.GetZOrderedListOfChildren(RootViewPane);
		if (list == null)
		{
			return false;
		}
		foreach (UIObject item in list)
		{
			if (item.Shape.GlobalBounds.Contains(new SKPoint(vector2.x, vector2.y)))
			{
				return true;
			}
		}
		return false;
	}
}
