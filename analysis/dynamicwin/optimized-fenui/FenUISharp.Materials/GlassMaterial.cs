using System;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using SkiaSharp;

namespace FenUISharp.Materials;

public class GlassMaterial : Material
{
	private int DisplacementMapDownscale = 1;

	public Func<SKColor> BaseColor
	{
		get
		{
			return GetProp<Func<SKColor>>("BaseColor", () => SKColors.White);
		}
		set
		{
			SetProp("BaseColor", value);
		}
	}

	public Func<SKColor> Highlight
	{
		get
		{
			return GetProp<Func<SKColor>>("Highlight", () => BaseColor().Multiply(1.5f).WithAlpha(150));
		}
		set
		{
			SetProp("Highlight", value);
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

	public Func<float> Displacement
	{
		get
		{
			return GetProp<Func<float>>("Displacement", () => 65f);
		}
		set
		{
			SetProp("Displacement", value);
		}
	}

	public Func<int> Distance
	{
		get
		{
			return GetProp<Func<int>>("Distance", () => 8);
		}
		set
		{
			SetProp("Distance", value);
		}
	}

	public Func<float> Brightness
	{
		get
		{
			return GetProp<Func<float>>("Brightness", () => 1.3f);
		}
		set
		{
			SetProp("Brightness", value);
		}
	}

	public GlassMaterial(Func<SKImage?> GrabPassFunction)
	{
		this.GrabPassFunction = GrabPassFunction;
	}

	protected override void Draw(SKCanvas targetCanvas, SKPath path, UIObject caller, SKPaint paint)
	{
		using SKImage sKImage = GrabPassFunction();
		if (sKImage == null)
		{
			return;
		}
		SKImage sKImage2 = null;
		using (FAdditionalSurface fAdditionalSurface = FContext.GetCurrentWindow().SkiaDirectCompositionContext?.CreateAdditional(sKImage.Info))
		{
			if (fAdditionalSurface == null)
			{
				return;
			}
			using SKCanvas sKCanvas = fAdditionalSurface.SkiaSurface.Canvas;
			float num = BlurRadius();
			using SKImageFilter imageFilter = SKImageFilter.CreateBlur(num / 1.25f, num / 1.25f);
			using SKPaint paint2 = new SKPaint
			{
				IsAntialias = true,
				ImageFilter = imageFilter
			};
			sKCanvas.DrawImage(sKImage, 0f, 0f, paint2);
			sKImage2 = fAdditionalSurface.SkiaSurface.Snapshot();
		}
		path.GetBounds(out var rect);
		if (rect.Width == 0f || rect.Height == 0f)
		{
			return;
		}
		caller.Padding.SetStaticState(35, 1u);
		SKImageInfo info = new SKImageInfo((int)MathF.Ceiling(rect.Width / (float)DisplacementMapDownscale), (int)MathF.Ceiling(rect.Height / (float)DisplacementMapDownscale));
		using FAdditionalSurface fAdditionalSurface2 = FContext.GetCurrentWindow().SkiaDirectCompositionContext?.CreateAdditional(info);
		if (fAdditionalSurface2 == null)
		{
			return;
		}
		using SKCanvas sKCanvas2 = fAdditionalSurface2.SkiaSurface.Canvas;
		int num2 = Distance();
		Vector2 off = new Vector2(0f - rect.Left, 0f - rect.Top);
		sKCanvas2.Translate(off.x, off.y);
		using SKPaint sKPaint = new SKPaint
		{
			IsAntialias = true,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round
		};
		sKPaint.Style = SKPaintStyle.Fill;
		sKPaint.Color = SKColors.White;
		sKCanvas2.DrawPath(path, sKPaint);
		sKPaint.Style = SKPaintStyle.Stroke;
		for (int num3 = num2; num3 >= 1; num3--)
		{
			float num4 = (float)num3 / (float)num2;
			sKPaint.Color = new SKColor((byte)(num4 * 255f), (byte)(num4 * 255f), (byte)(num4 * 255f), byte.MaxValue);
			sKPaint.StrokeWidth = num3 * 2;
			sKCanvas2.DrawPath(path, sKPaint);
		}
		using SKShader displacementMap = fAdditionalSurface2.SkiaSurface.Snapshot().ToShader(SKShaderTileMode.Decal, SKShaderTileMode.Decal, new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Nearest));
		using SKShader shader = CreateShader(displacementMap, sKImage2.ToShader(SKShaderTileMode.Decal, SKShaderTileMode.Decal, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)), rect, caller, BaseColor(), off);
		int count = targetCanvas.Save();
		targetCanvas.ClipPath(path, SKClipOperation.Intersect, antialias: true);
		paint.Shader = shader;
		float num5 = BlurRadius();
		using SKImageFilter imageFilter2 = SKImageFilter.CreateBlur(num5 / 4f, num5 / 4f);
		paint.ImageFilter = imageFilter2;
		_ = caller.Shape.SurfaceDrawRect;
		targetCanvas.DrawPath(path, paint);
		SKPaint sKPaint2 = new SKPaint
		{
			Color = paint.Color,
			IsStroke = true,
			StrokeWidth = 3f,
			IsAntialias = true
		};
		using (SKShader shader2 = SKShader.CreateLinearGradient(new SKPoint(RMath.Lerp(rect.Left, rect.MidX, 0.4f), rect.Top), new SKPoint(RMath.Lerp(rect.Right, rect.MidX, 0.4f), rect.Bottom), new SKColor[3]
		{
			Highlight(),
			SKColors.White.WithAlpha(0),
			Highlight()
		}, null, SKShaderTileMode.Clamp))
		{
			sKPaint2.Shader = shader2;
		}
		targetCanvas.DrawPath(path, sKPaint2);
		targetCanvas.RestoreToCount(count);
		caller.Invalidate(UIObject.Invalidation.SurfaceDirty);
		sKImage2.Dispose();
	}

	private SKShader? CreateShader(SKShader displacementMap, SKShader grabPass, SKRect bounds, UIObject caller, SKColor baseColorMix, Vector2 off)
	{
		string errors;
		SKRuntimeEffect sKRuntimeEffect = SKRuntimeEffect.CreateShader("\r\n                uniform shader contentShader;\r\n                uniform shader grabPass; // Content behind the glass\r\n\r\n                uniform float3 iBaseMix;\r\n                uniform float2 iResolution;\r\n                uniform float2 iOff;\r\n                uniform float2 iPathCorrection;\r\n                uniform float iBright;\r\n\r\n                uniform float iDisplacement;\r\n                uniform float iDownscale;\r\n\r\n                float rand(float x) {\r\n                    return fract(sin(x * 12.9898) * 43758.5453123);\r\n                }\r\n\r\n                float4 colorLerp(float4 a, float4 b, float t) {\r\n                    return mix(a, b, t);\r\n                }\r\n\r\n                float remapOffset(float x)\r\n                {\r\n                    // return -sin(x * (3.141/0.66)) * x;\r\n                    return pow(x, 2);\r\n                }\r\n\r\n                half4 main(float2 fragCoord) {\r\n                    float2 uv = (fragCoord - iOff);\r\n                    float2 grabSampleUV = (fragCoord);\r\n\r\n                    float4 offsetAmount = contentShader.eval((fragCoord + iPathCorrection) * iDownscale);\r\n                    float4 simpleBack = float4(0, 0, 0, 0);\r\n                    float4 backColor = float4(0, 0, 0, 0);\r\n\r\n                    {\r\n                        // float offsetSA = pow((1.0 - (offsetAmount.r)), 4) * iDisplacement;\r\n                        float offsetSA = remapOffset(1.0 - (offsetAmount.r)) * iDisplacement;\r\n\r\n                        float2 noiseVec = vec2(rand(dot(grabSampleUV, vec2(12.9898, 78.233))), rand(dot(grabSampleUV, vec2(39.3468, 11.135))));\r\n                        float2 offset = float2(-offsetSA, offsetSA) + (noiseVec - 0.5) * 0.1; // subtle and centered noise\r\n\r\n                        float2 displacedUV = uv + offset;\r\n                        simpleBack += grabPass.eval(displacedUV);\r\n\r\n                        // return half4(offsetSA / iDisplacement, 0, 0, 1);\r\n                        // return half4(offset / iDisplacement, 0, 1);\r\n                    }\r\n\r\n                    if(offsetAmount.r > 0.99) \r\n                    {\r\n                        backColor = simpleBack;\r\n                    }\r\n                    else\r\n                    {\r\n                        const float samples = 6;\r\n                        for (float i = 0; i < samples; ++i) {\r\n                            float v = (i - 2) * (12 / samples);\r\n                            // float offsetSA = pow((1.0 - (offsetAmount.r)), 4) * iDisplacement + v;\r\n                            float offsetSA = remapOffset(1.0 - (offsetAmount.r)) * iDisplacement;\r\n\r\n                            float2 noiseVec = vec2(rand(dot(grabSampleUV, vec2(12.9898, 78.233))), rand(dot(grabSampleUV, vec2(39.3468, 11.135))));\r\n                            float2 offset = float2(-offsetSA, offsetSA) + (noiseVec - 0.5) * 0.1; // subtle and centered noise\r\n\r\n                            float2 displacedUV = uv + offset;\r\n                            backColor += grabPass.eval(displacedUV);\r\n                        }\r\n                        backColor /= samples;\r\n                        backColor = colorLerp(backColor, simpleBack, (offsetAmount.r));\r\n                    }\r\n\r\n                    float4 returnColor = (backColor * iBright + ((iBright - 1) / 2)) * float4(iBaseMix, 1);\r\n                    returnColor.a = 1;\r\n\r\n                    // return half4(fragCoord / iResolution + offset, 0, 1);\r\n                    return returnColor;\r\n                }\r\n            ", out errors);
		if (sKRuntimeEffect == null)
		{
			FLogger.Error("Shader compilation failed: " + errors);
		}
		SKRuntimeEffectUniforms sKRuntimeEffectUniforms = new SKRuntimeEffectUniforms(sKRuntimeEffect);
		sKRuntimeEffectUniforms["iBaseMix"] = new float[3]
		{
			(float)(int)baseColorMix.Red / 255f,
			(float)(int)baseColorMix.Green / 255f,
			(float)(int)baseColorMix.Blue / 255f
		};
		sKRuntimeEffectUniforms["iResolution"] = new float[2] { bounds.Width, bounds.Height };
		sKRuntimeEffectUniforms["iOff"] = new float[2]
		{
			-caller.Padding.CachedValue,
			-caller.Padding.CachedValue
		};
		sKRuntimeEffectUniforms["iPathCorrection"] = new float[2] { off.x, off.y };
		sKRuntimeEffectUniforms["iDisplacement"] = Displacement();
		sKRuntimeEffectUniforms["iBright"] = Brightness();
		sKRuntimeEffectUniforms["iDownscale"] = (float)DisplacementMapDownscale;
		SKRuntimeEffectChildren sKRuntimeEffectChildren = new SKRuntimeEffectChildren(sKRuntimeEffect);
		sKRuntimeEffectChildren["contentShader"] = displacementMap;
		sKRuntimeEffectChildren["grabPass"] = grabPass;
		return sKRuntimeEffect?.ToShader(sKRuntimeEffectUniforms, sKRuntimeEffectChildren) ?? null;
	}
}
