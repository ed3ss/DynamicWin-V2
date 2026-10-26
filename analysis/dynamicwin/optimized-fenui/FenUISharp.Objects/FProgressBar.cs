using System;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FProgressBar : UIObject, IStateListener
{
	protected float _01value;

	protected float time;

	public State<float> Value { get; init; }

	public State<float> MaxValue { get; init; }

	public State<float> MinValue { get; init; }

	public bool Indeterminate { get; set; }

	public bool LeftToRight { get; set; } = true;

	public Action<float>? OnValueChanged { get; set; }

	public State<SKColor> BackgroundColor { get; set; }

	public State<SKColor> FillColor { get; set; }

	public State<SKColor> BorderColor { get; set; }

	public float IndeterminateSpeed { get; set; } = 0.6f;

	public float IndeterminateLinesRepeat { get; set; } = 1.5f;

	public FProgressBar(Func<float> value, Func<Vector2>? position = null, float width = 100f, float height = 5f)
		: base(position, () => new Vector2(width, height))
	{
		BackgroundColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Background, this, this);
		FillColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary, this, this);
		BorderColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface, this, this);
		Value = new State<float>(value, this, this);
		MinValue = new State<float>(() => 0f, this, this);
		MaxValue = new State<float>(() => 1f, this, this);
		base.Padding.SetStaticState(5);
	}

	public FProgressBar(Func<Vector2>? position = null, float width = 100f, float height = 5f)
		: base(position, () => new Vector2(width, height))
	{
		BackgroundColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Background, this, this);
		FillColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary, this, this);
		BorderColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface, this, this);
		Value = new State<float>(() => 0f, this, this);
		Indeterminate = true;
		MinValue = new State<float>(() => 0f, this, this);
		MaxValue = new State<float>(() => 1f, this, this);
		base.Padding.SetStaticState(5);
	}

	protected override void Update()
	{
		base.Update();
		if (Indeterminate && FContext.GetCurrentWindow().Properties.IsWindowFocused && FContext.GetCurrentWindow().Properties.IsWindowVisible)
		{
			time -= IndeterminateSpeed * FContext.DeltaTime;
			time %= 1f;
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void OnInternalStateChanged<T>(T value)
	{
		base.OnInternalStateChanged(value);
		float num = _01value;
		_01value = RMath.Remap(Value.CachedValue, MinValue.CachedValue, MaxValue.CachedValue, 0f, 1f);
		if (num != _01value)
		{
			OnValueChanged?.Invoke(Value.CachedValue);
		}
		Invalidate(Invalidation.SurfaceDirty);
	}

	public override void Render(SKCanvas canvas)
	{
		SKRect localBounds = base.Shape.LocalBounds;
		using (SKPaint sKPaint = GetRenderPaint())
		{
			using SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 2f, 2f, 2f, FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Shadow);
			using SKPath path = SKSquircle.CreateSquircle(localBounds, 10f);
			sKPaint.Color = BackgroundColor.CachedValue;
			sKPaint.ImageFilter = imageFilter;
			canvas.DrawPath(path, sKPaint);
			sKPaint.ImageFilter = null;
			sKPaint.Color = BorderColor.CachedValue;
			sKPaint.IsStroke = true;
			sKPaint.StrokeWidth = 1f;
			canvas.Translate(0.5f, 0.5f);
			canvas.DrawPath(path, sKPaint);
			canvas.Translate(-0.5f, -0.5f);
		}
		SKRect rect = localBounds;
		if (Indeterminate)
		{
			using SKPaint sKPaint2 = GetRenderPaint();
			using SKPath path2 = SKSquircle.CreateSquircle(rect, 10f);
			sKPaint2.Color = FillColor.CachedValue;
			canvas.DrawPath(path2, sKPaint2);
			string errors;
			SKRuntimeEffect obj = SKRuntimeEffect.CreateShader("\r\n                        uniform float2      iResolution;\r\n                        uniform float       iTime;\r\n                        uniform float       iRepeat;\r\n\r\n                        half4 main(float2 fragCoord) {\r\n                            float2 uv = fragCoord / iResolution;\r\n                            uv.x += iTime * 0.2;          // slide over time\r\n                            uv.x *= iRepeat;\r\n\r\n                            float saw = fract(uv.x * 10.0);\r\n                            float mask = step(0.5, saw);  // 0 or 1\r\n\r\n                            // unpremultiplied colors, full-alpha\r\n                            half4 white = half4(1,1,1,0.25);\r\n                            half4 blue  = half4(0,0,0,0);\r\n\r\n                            // pick white or blue\r\n                            half4 colUP = mix(white, blue, mask);\r\n\r\n                            colUP.rgb *= colUP.a;\r\n                            return colUP;\r\n                        }\r\n                    ", out errors) ?? throw new Exception(errors);
			using SKShader shader = obj.ToShader(new SKRuntimeEffectUniforms(obj)
			{
				["iResolution"] = new SKPoint(rect.Width, rect.Height),
				["iTime"] = time,
				["iRepeat"] = IndeterminateLinesRepeat
			});
			SKMatrix first = SKMatrix.CreateSkew(-1f, 0f);
			SKMatrix second = SKMatrix.CreateTranslation(6f, 0f);
			using SKImageFilter imageFilter2 = SKImageFilter.CreateMatrix(in ILSpyHelper_AsRefReadOnly(SKMatrix.Concat(first, second)));
			sKPaint2.ImageFilter = imageFilter2;
			sKPaint2.Shader = shader;
			sKPaint2.BlendMode = SKBlendMode.Screen;
			canvas.ClipPath(path2, SKClipOperation.Intersect, antialias: true);
			canvas.DrawPath(path2, sKPaint2);
		}
		else
		{
			rect = ((!LeftToRight) ? new SKRect(localBounds.Right + base.Layout.ClampSize(base.Transform.Size.CachedValue).x * (0f - RMath.Remap(_01value, 0f, 1f, 0.01f, 1f)), localBounds.Top, localBounds.Right, localBounds.Top + base.Layout.ClampSize(base.Transform.Size.CachedValue).y) : new SKRect(localBounds.Left, localBounds.Top, localBounds.Left + base.Layout.ClampSize(base.Transform.Size.CachedValue).x * RMath.Remap(_01value, 0f, 1f, 0.01f, 1f), localBounds.Top + base.Layout.ClampSize(base.Transform.Size.CachedValue).y));
			using SKPaint sKPaint3 = GetRenderPaint();
			using SKPath path3 = SKSquircle.CreateSquircle(rect, 10f);
			sKPaint3.Color = FillColor.CachedValue;
			canvas.DrawPath(path3, sKPaint3);
		}
		using (SKPaint sKPaint4 = GetRenderPaint())
		{
			using SKPath path4 = SKSquircle.CreateSquircle(rect, 10f);
			SKPoint start = (LeftToRight ? new SKPoint(localBounds.Left, localBounds.MidY) : new SKPoint(localBounds.Right, localBounds.MidY));
			SKPoint end = (LeftToRight ? new SKPoint(localBounds.Right, localBounds.MidY) : new SKPoint(localBounds.Left, localBounds.MidY));
			sKPaint4.Color = FillColor.CachedValue;
			sKPaint4.Shader = SKShader.CreateLinearGradient(start, end, new SKColor[2]
			{
				SKColors.Black.WithAlpha(65),
				SKColors.White.WithAlpha(85)
			}, new float[2] { 0f, 1f }, SKShaderTileMode.Clamp);
			sKPaint4.BlendMode = SKBlendMode.Screen;
			canvas.DrawPath(path4, sKPaint4);
		}
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp)
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}
}
