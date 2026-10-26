using System;
using FenUISharp.Objects;
using SkiaSharp;

namespace FenUISharp.Materials;

public class BlurMaterial : Material
{
	public Func<SKColor> BaseColor
	{
		get
		{
			return GetProp<Func<SKColor>>("BaseColor", () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Secondary);
		}
		set
		{
			SetProp("BaseColor", value);
		}
	}

	public Func<SKImage?> GrabPassFunction
	{
		get
		{
			return GetProp<Func<SKImage>>("GrabPassFunction", () => (SKImage?)null);
		}
		set
		{
			SetProp("GrabPassFunction", value);
		}
	}

	public Func<float> BlurRadius
	{
		get
		{
			return GetProp<Func<float>>("BlurRadius", () => 5f);
		}
		set
		{
			SetProp("BlurRadius", value);
		}
	}

	public BlurMaterial(Func<SKImage?> GrabPassFunction)
	{
		this.GrabPassFunction = GrabPassFunction;
	}

	protected override void Draw(SKCanvas targetCanvas, SKPath path, UIObject caller, SKPaint paint)
	{
		int count = targetCanvas.Save();
		using SKImage sKImage = GrabPassFunction();
		if (sKImage != null)
		{
			targetCanvas.ClipPath(path, SKClipOperation.Intersect, antialias: true);
			paint.Color = BaseColor();
			targetCanvas.DrawPath(path, paint);
			using (SKImageFilter imageFilter = SKImageFilter.CreateBlur(BlurRadius(), BlurRadius()))
			{
				paint.ImageFilter = imageFilter;
			}
			SKRect surfaceDrawRect = caller.Shape.SurfaceDrawRect;
			targetCanvas.DrawImage(sKImage, surfaceDrawRect, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
			targetCanvas.RestoreToCount(count);
			caller.Invalidate(UIObject.Invalidation.SurfaceDirty);
		}
	}
}
