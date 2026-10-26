using FenUISharp.Logging;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.Objects;

internal class FHueSlider : FSlider
{
	public FHueSlider()
	{
		base.KnobPositionSpring.SetValues(3f, 1.5f);
		base.InteractiveSurface.ExtendInteractionRadius.SetStaticState(-6);
		base.ClampKnob = true;
	}

	protected override void RenderBackground(SKCanvas canvas, SKRect rect)
	{
		string errors;
		SKRuntimeEffect sKRuntimeEffect = SKRuntimeEffect.CreateShader("\r\n                uniform float2 iResolution;\r\n                uniform float2 iOffset;\r\n\r\n                half4 main(float2 fragCoord) {\r\n                    float2 uv = (fragCoord) / iResolution;\r\n\r\n                    float hue = uv.x; // hue from 0 to 360\r\n\r\n                    // Convert hue to radians and then to RGB\r\n                    float c = 1;\r\n                    float h = (hue * 360) / 60.0;\r\n                    float x = c * (1.0 - abs(mod(h, 2.0) - 1.0));\r\n                    \r\n                    float3 rgb;\r\n                    if (0.0 <= h && h < 1.0) rgb = float3(c, x, 0.0);\r\n                    else if (1.0 <= h && h < 2.0) rgb = float3(x, c, 0.0);\r\n                    else if (2.0 <= h && h < 3.0) rgb = float3(0.0, c, x);\r\n                    else if (3.0 <= h && h < 4.0) rgb = float3(0.0, x, c);\r\n                    else if (4.0 <= h && h < 5.0) rgb = float3(x, 0.0, c);\r\n                    else rgb = float3(c, 0.0, x);\r\n\r\n                    return half4(rgb, 1.0);\r\n                }\r\n            ", out errors);
		if (sKRuntimeEffect == null)
		{
			FLogger.Error("Shader compilation failed: " + errors);
		}
		SKRuntimeEffectUniforms sKRuntimeEffectUniforms = new SKRuntimeEffectUniforms(sKRuntimeEffect);
		sKRuntimeEffectUniforms["iResolution"] = new float[2] { rect.Width, rect.Height };
		sKRuntimeEffectUniforms["iOffset"] = new float[2]
		{
			rect.Left - base.Shape.SurfaceDrawRect.Left,
			rect.Top - base.Shape.SurfaceDrawRect.Top
		};
		using SKPaint sKPaint = GetRenderPaint();
		using SKRoundRect rect2 = new SKRoundRect(rect, base.BarCornerRadius);
		using SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 0f, 2f, 2f, FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Shadow.AddMix(new SKColor(0, 0, 0, 25)));
		sKPaint.ImageFilter = imageFilter;
		canvas.DrawRoundRect(rect2, sKPaint);
		sKPaint.ImageFilter = null;
		sKPaint.Shader = sKRuntimeEffect?.ToShader(sKRuntimeEffectUniforms);
		canvas.DrawRoundRect(rect2, sKPaint);
		sKPaint.IsStroke = true;
		sKPaint.StrokeWidth = 2f;
		sKPaint.Color = SKColors.White.WithAlpha(35);
		sKPaint.BlendMode = SKBlendMode.Plus;
		sKPaint.Shader = null;
		canvas.DrawRoundRect(rect2, sKPaint);
	}

	protected override void RenderKnob(SKCanvas canvas, SKRect knobRect)
	{
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Color = SKColor.FromHsv(_value * 360f, 100f, 100f);
		using (SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 1f, 5f, 5f, base.KnobShadow.CachedValue))
		{
			sKPaint.ImageFilter = imageFilter;
		}
		using SKRoundRect rect = new SKRoundRect(knobRect, base.KnobCornerRadius);
		canvas.DrawRoundRect(rect, sKPaint);
		if (base.KnobBorderSize > 0f)
		{
			sKPaint.Color = base.KnobBorder.CachedValue;
			sKPaint.IsStroke = true;
			sKPaint.StrokeWidth = base.KnobBorderSize;
			canvas.DrawRoundRect(rect, sKPaint);
		}
		sKPaint.IsStroke = true;
		sKPaint.StrokeWidth = 2f;
		sKPaint.Color = SKColors.White.WithAlpha(15);
		sKPaint.BlendMode = SKBlendMode.Plus;
		sKPaint.Shader = null;
		canvas.DrawRoundRect(rect, sKPaint);
	}
}
