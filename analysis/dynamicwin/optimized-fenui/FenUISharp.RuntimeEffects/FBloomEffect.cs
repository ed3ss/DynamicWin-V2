using System;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.RuntimeEffects;

public class FBloomEffect : IPostProcessEffect
{
	public float BloomSpread { get; set; } = 10f;

	public float BloomIntensity { get; set; } = 1f;

	public float BloomThreshold { get; set; } = 0.75f;

	public int Downsampling { get; set; } = 2;

	public void OnAfterRender(PPInfo info)
	{
		GRContext gRContext = FContext.GetCurrentWindow().SkiaDirectCompositionContext?.grContext;
		SKImage sKImage = info.source.Snapshot();
		int num = RMath.Clamp(Downsampling, 1, 12);
		int width = sKImage.Width / num;
		int height = sKImage.Height / num;
		SKImageInfo info2 = new SKImageInfo(width, height);
		FAdditionalSurface fAdditionalSurface = FContext.GetCurrentWindow().SkiaDirectCompositionContext?.CreateAdditional(info2);
		if (fAdditionalSurface != null)
		{
			SKCanvas canvas = fAdditionalSurface.SkiaSurface.Canvas;
			canvas.Clear(SKColors.Transparent);
			canvas.Scale(1f / (float)num);
			SKShader sKShader = CreateShader(info, sKImage);
			SKImageFilter sKImageFilter = SKImageFilter.CreateBlur(RMath.Clamp(BloomSpread, 0f, 35f) / (float)num, RMath.Clamp(BloomSpread, 0f, 35f) / (float)num);
			SKPaint sKPaint = new SKPaint
			{
				Shader = sKShader
			};
			canvas.DrawRect(0f, 0f, info.sourceInfo.Width, info.sourceInfo.Height, sKPaint);
			SKImage sKImage2 = fAdditionalSurface.SkiaSurface.Snapshot();
			int count = info.target.Canvas.Save();
			SKPaint paint = new SKPaint
			{
				BlendMode = SKBlendMode.Plus,
				ImageFilter = sKImageFilter
			};
			info.target.Canvas.ResetMatrix();
			info.target.Canvas.Scale(num);
			info.target.Canvas.DrawImage(sKImage2, 0f, 0f, paint);
			info.target.Canvas.RestoreToCount(count);
			info.target.Canvas.Flush();
			gRContext?.Flush();
			gRContext?.Submit(synchronous: true);
			FContext.GetCurrentWindow().SkiaDirectCompositionContext?.WaitForGPU();
			sKImage2.Dispose();
			fAdditionalSurface.Dispose();
			sKPaint.Dispose();
			sKImageFilter.Dispose();
			sKShader.Dispose();
			sKImage.Dispose();
		}
	}

	private SKShader CreateShader(PPInfo info, SKImage snapshot)
	{
		string errors;
		SKRuntimeEffect sKRuntimeEffect = SKRuntimeEffect.CreateShader("\r\n                uniform shader contentShader;\r\n\r\n                uniform float downsampling;\r\n                uniform float intensity;\r\n                uniform float threshold;\r\n                uniform float2 iResolution;\r\n\r\n                half4 main(float2 fragCoord) {\r\n                    float2 uv = (fragCoord);\r\n\r\n                    float4 color = contentShader.eval(uv);\r\n                    float brightness = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));\r\n                    if(brightness < threshold) color = float4(0, 0, 0, 0);\r\n\r\n                    return color * intensity;\r\n                }\r\n            ", out errors);
		if (sKRuntimeEffect == null)
		{
			FLogger.Error("Shader compilation failed: " + errors);
		}
		SKRuntimeEffectUniforms sKRuntimeEffectUniforms = new SKRuntimeEffectUniforms(sKRuntimeEffect);
		sKRuntimeEffectUniforms["iResolution"] = new float[2]
		{
			info.sourceInfo.Width,
			info.sourceInfo.Height
		};
		sKRuntimeEffectUniforms["intensity"] = RMath.Clamp(BloomIntensity, 0f, 10f);
		sKRuntimeEffectUniforms["threshold"] = RMath.Clamp(BloomThreshold, 0f, 1f);
		sKRuntimeEffectUniforms["downsampling"] = (float)RMath.Clamp(Downsampling, 1, 12);
		SKRuntimeEffectChildren sKRuntimeEffectChildren = new SKRuntimeEffectChildren(sKRuntimeEffect);
		sKRuntimeEffectChildren["contentShader"] = snapshot.ToShader(SKShaderTileMode.Decal, SKShaderTileMode.Decal, new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Nearest));
		return sKRuntimeEffect?.ToShader(sKRuntimeEffectUniforms, sKRuntimeEffectChildren) ?? throw new Exception("Unable to create shader.");
	}

	public void OnBeforeRender(PPInfo info)
	{
	}

	public void OnLateAfterRender(PPInfo info)
	{
	}
}
