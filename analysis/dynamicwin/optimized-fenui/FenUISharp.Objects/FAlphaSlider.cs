using FenUISharp.Logging;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.Objects;

internal class FAlphaSlider : FSlider
{
	public FAlphaSlider()
	{
		base.KnobPositionSpring.SetValues(3f, 1.5f);
		base.InteractiveSurface.ExtendInteractionRadius.SetStaticState(-6);
		base.ClampKnob = true;
	}

	protected override void RenderBackground(SKCanvas canvas, SKRect rect)
	{
		string errors;
		SKRuntimeEffect sKRuntimeEffect = SKRuntimeEffect.CreateShader("\r\n                uniform float2 iResolution;\r\n                uniform float2 iOff;\r\n\r\n                half4 lerp(half4 a, half4 b, half t) {\r\n                    return a + (b - a) * t;\r\n                }\r\n\r\n                half4 main(float2 fragCoord) {\r\n                    float2 uv = (fragCoord - iOff) / iResolution;\r\n\r\n                    float2 coord = fragCoord;\r\n                    \r\n                    float checker = mod(floor(coord.x / 5) + floor(coord.y / 5), 2.0);\r\n                    checker = clamp(checker, 0.6, 0.9);\r\n                    half4 colAlpha = lerp(half4(checker, checker, checker, 1), half4(1, 1, 1, 1), uv.x);\r\n                    \r\n                    return colAlpha;\r\n                }\r\n            ", out errors);
		if (sKRuntimeEffect == null)
		{
			FLogger.Error("Shader compilation failed: " + errors);
		}
		SKRuntimeEffectUniforms sKRuntimeEffectUniforms = new SKRuntimeEffectUniforms(sKRuntimeEffect);
		sKRuntimeEffectUniforms["iResolution"] = new float[2] { rect.Width, rect.Height };
		sKRuntimeEffectUniforms["iOff"] = new float[2] { rect.Left, rect.Top };
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
}
