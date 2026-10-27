using FenUISharp;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace DynamicWin;

public class DistortionPane : FPanel
{
	private SKImage? lastCapture;

	private DXDesktopCapture? desktopCapture;
	private readonly object captureGate = new();
	private SKImage? pendingCapture;
	private bool disposed;

	public State<Vector2> BulgePosition { get; init; }

	public State<float> BulgeStrength { get; init; }

	public State<float> BulgeRadius { get; init; }

	public State<float> ChannelShiftStrength { get; init; }

	public DistortionPane()
	{
		_drawBasePanel = false;
		BulgePosition = new State<Vector2>(() => Vector2.Zero, this, this);
		BulgeStrength = new State<float>(() => 0f, this, this);
		BulgeRadius = new State<float>(() => 0f, this, this);
		ChannelShiftStrength = new State<float>(() => 0f, this, this);
		FContext.GetCurrentWindow().Properties.ExcludeFromDesktopDuplication = true;
		try
		{
			desktopCapture = new DXDesktopCapture();
			desktopCapture.FrameReady += ReceiveCapture;
			desktopCapture.Start();
		}
		catch (System.Exception ex) { FLogger.Error("Desktop effect unavailable: " + ex.Message); }
	}

	private void ReceiveCapture(SKImage image)
	{
		lock (captureGate)
		{
			if (disposed) { image.Dispose(); return; }
			pendingCapture?.Dispose();
			pendingCapture = image;
		}
	}

	protected override void Update()
	{
		base.Update();
		lock (captureGate)
		{
			if (pendingCapture == null) return;
			lastCapture?.Dispose();
			lastCapture = pendingCapture;
			pendingCapture = null;
		}
		Invalidate(Invalidation.SurfaceDirty);
	}

	public override void Dispose()
	{
		lock (captureGate) { disposed = true; pendingCapture?.Dispose(); pendingCapture = null; }
		desktopCapture?.Dispose();
		lastCapture?.Dispose();
		lastCapture = null;
		base.Dispose();
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		if (lastCapture == null || !(BulgeStrength.CachedValue > 0f))
		{
			return;
		}
		SKRect rect = SKRect.Create(BulgePosition.CachedValue.x, BulgePosition.CachedValue.y, 0f, 0f);
		rect.Inflate(BulgeRadius.CachedValue, BulgeRadius.CachedValue);
		canvas.ClipRect(rect);
		using SKPaint sKPaint = GetRenderPaint();
		using SKShader captureShader = lastCapture.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
		using SKShader shader = CreateShader(captureShader, base.Shape.GlobalBounds);
		sKPaint.Shader = shader;
		canvas.DrawRect(base.Shape.LocalBounds, sKPaint);
	}

	private SKShader? CreateShader(SKShader grabPass, SKRect bounds)
	{
		string errors;
		using SKRuntimeEffect sKRuntimeEffect = SKRuntimeEffect.CreateShader("\r\n                uniform shader grabPass;\r\n                uniform float2 iResolution;\r\n\r\n                uniform float2 bulgeCenter;\r\n                uniform float bulgeRadius;\r\n                uniform float bulgeStrength;\r\n                uniform float bulgeSoftness;\r\n                uniform float channelShift;\r\n\r\n                float2 getBUV(float2 fragCoord, float strengthMul)\r\n                {\r\n                    float2 uv = fragCoord / iResolution;\r\n                    float2 centerUV = bulgeCenter / iResolution;\r\n\r\n                    float2 delta = uv - centerUV;\r\n                    float dist = length(delta);\r\n\r\n                    float radiusNorm = bulgeRadius / max(iResolution.x, iResolution.y);\r\n\r\n                    if (dist < radiusNorm) {\r\n                        float percent = dist / radiusNorm;\r\n\r\n                        // Smooth falloff near edges\r\n                        // softness = 0 => hard edge, 1 => very soft\r\n                        float edgeFalloff = smoothstep(1.0, 1.0 - bulgeSoftness, percent);\r\n\r\n                        // Distortion curve\r\n                        float theta = percent;\r\n                        float factor = 1.0 - (bulgeStrength * strengthMul * (1.0 - theta*theta) * edgeFalloff);\r\n\r\n                        uv = centerUV + delta * factor;\r\n                    }\r\n\r\n                    return uv;\r\n                }\r\n\r\n                half4 main(float2 fragCoord) \r\n                {\r\n                    float2 uvR = getBUV(fragCoord, 1 + channelShift) * iResolution;\r\n                    float2 uvG = getBUV(fragCoord, 1) * iResolution;\r\n                    float2 uvB = getBUV(fragCoord, 1 - channelShift) * iResolution;\r\n\r\n                    half colR = grabPass.eval(uvR).r;\r\n                    half colG = grabPass.eval(uvG).g;\r\n                    half colB = grabPass.eval(uvB).b;\r\n\r\n                    half3 col = half3(colR, colG, colB);\r\n\r\n                    // Compute brightness factor\r\n                    float2 uv = fragCoord / iResolution;\r\n                    float2 centerUV = bulgeCenter / iResolution;\r\n                    float2 delta = uv - centerUV;\r\n                    float dist = length(delta);\r\n                    float radiusNorm = bulgeRadius / max(iResolution.x, iResolution.y);\r\n\r\n                    float brightness = 0.0;\r\n                    if (dist < radiusNorm)\r\n                    {\r\n                        float percent = dist / radiusNorm;\r\n                        float edgeFalloff = smoothstep(1.0, 1.0 - bulgeSoftness, percent);\r\n                        brightness = 0.2 * edgeFalloff;\r\n                    }\r\n\r\n                    if(col == half3(0, 0, 0))\r\n                        return half4(0, 0, 0, 0);\r\n\r\n                    col += half3(brightness, brightness, brightness) * bulgeStrength * 2.5;\r\n                    return half4(col, 1);\r\n                }\r\n            ", out errors);
		if (sKRuntimeEffect == null)
		{
			FLogger.Error("Shader compilation failed: " + errors); return null;
		}
		SKRuntimeEffectUniforms sKRuntimeEffectUniforms = new SKRuntimeEffectUniforms(sKRuntimeEffect);
		sKRuntimeEffectUniforms["iResolution"] = new float[2] { bounds.Width, bounds.Height };
		sKRuntimeEffectUniforms["bulgeRadius"] = BulgeRadius.CachedValue;
		sKRuntimeEffectUniforms["bulgeStrength"] = BulgeStrength.CachedValue;
		sKRuntimeEffectUniforms["bulgeCenter"] = new float[2]
		{
			BulgePosition.CachedValue.x,
			BulgePosition.CachedValue.y
		};
		sKRuntimeEffectUniforms["bulgeSoftness"] = 0.5f;
		sKRuntimeEffectUniforms["channelShift"] = ChannelShiftStrength.CachedValue;
		SKRuntimeEffectChildren sKRuntimeEffectChildren = new SKRuntimeEffectChildren(sKRuntimeEffect);
		sKRuntimeEffectChildren["grabPass"] = grabPass;
		return sKRuntimeEffect?.ToShader(sKRuntimeEffectUniforms, sKRuntimeEffectChildren) ?? null;
	}
}
