using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.RuntimeEffects;

public class FGradientSwipeEffect : IPostProcessEffect
{
	public State<float> GradientPosition { get; private set; }

	public State<SKColor> Primary { get; private set; }

	public State<SKColor> Secondary { get; private set; }

	public float ModuloRangeMultiplicator { get; set; } = 1f;

	public float GradientWidth { get; set; } = 1f;

	public bool RepeatGradient { get; set; }

	public bool MapGradientPosition { get; set; } = true;

	public float RotationDegrees { get; set; }

	public float GradientRotation { get; set; }

	public FGradientSwipeEffect(UIObject owner)
	{
		GradientPosition = new State<float>(() => 0f, owner, delegate
		{
			owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		});
		Primary = new State<SKColor>(() => SKColors.Gray, owner, delegate
		{
			owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		});
		Secondary = new State<SKColor>(() => SKColors.Transparent, owner, delegate
		{
			owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		});
	}

	public void OnAfterRender(PPInfo info)
	{
		using SKImage image = info.source.Snapshot();
		SKRect surfaceDrawRect = info.owner.Shape.SurfaceDrawRect;
		float num = RMath.Remap(MapGradientPosition ? WrapToRange(GradientPosition.CachedValue) : GradientPosition.CachedValue, -1f, 1f, 0f - info.owner.Shape.LocalBounds.Width, info.owner.Shape.LocalBounds.Width);
		float midX = surfaceDrawRect.MidX;
		float num2 = info.owner.Shape.LocalBounds.Width * GradientWidth / 2f;
		float x = midX - num2 + num;
		float x2 = midX + num2 + num;
		SKPoint start = RMath.RotatePoint(new SKPoint(x, 0f), new SKPoint(surfaceDrawRect.MidX, surfaceDrawRect.MidY), GradientRotation);
		SKPoint end = RMath.RotatePoint(new SKPoint(x2, 0f), new SKPoint(surfaceDrawRect.MidX, surfaceDrawRect.MidY), GradientRotation);
		using SKShader shader = SKShader.CreateLinearGradient(start, end, new SKColor[3] { Secondary.CachedValue, Primary.CachedValue, Secondary.CachedValue }, new float[3] { 0f, 0.5f, 1f }, RepeatGradient ? SKShaderTileMode.Repeat : SKShaderTileMode.Clamp);
		info.target.Canvas.SaveLayer(SKRect.Create(0f, 0f, info.sourceInfo.Width, info.sourceInfo.Height), null);
		info.target.Canvas.Save();
		info.target.Canvas.ResetMatrix();
		info.target.Canvas.DrawImage(image, 0f, 0f);
		info.target.Canvas.Restore();
		using SKPaint paint = new SKPaint
		{
			Shader = shader,
			BlendMode = SKBlendMode.SrcIn
		};
		info.target.Canvas.RotateDegrees(RotationDegrees, surfaceDrawRect.MidX, surfaceDrawRect.MidY);
		info.target.Canvas.DrawRect(SKRect.Create(0f, 0f, info.sourceInfo.Width, info.sourceInfo.Height), paint);
		info.target.Canvas.Restore();
	}

	public float WrapToRange(float value)
	{
		if (value == 0f)
		{
			return 0f;
		}
		return (((value / ModuloRangeMultiplicator + 1f) % 2f + 2f) % 2f - 1f) * ModuloRangeMultiplicator;
	}

	public void OnBeforeRender(PPInfo info)
	{
	}

	public void OnLateAfterRender(PPInfo info)
	{
	}
}
