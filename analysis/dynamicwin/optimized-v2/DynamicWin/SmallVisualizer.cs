using System;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace DynamicWin;

public class SmallVisualizer : UIObject
{
	private AudioVisualizer visualizer;

	private const int barCount = 6;

	private float[] bars;

	private readonly object audioLock = new object();
	private readonly float[] pendingBars = new float[6];
	private bool hasPendingAudio;

	private float rawAvgVolume;

	private float averageVolume = 1f;

	private float restartTimer;

	public State<SKColor> Blend { get; set; }

	public State<SKColor> Primary { get; set; }

	public bool UpdateRealtime { get; set; } = true;

	public float ManualAmplitudeMultiplier { get; set; } = 1f;

	private float AdaptiveAmplifierMultiplier { get; set; } = 1f;

	public static float LastAdaptiveAmplifierMultiplier { get; private set; }

	private State<bool> useAdaptiveAmplifier { get; init; }

	public float MinSize { get; set; } = 2f;

	public Func<float> MaxSize { get; set; }

	public SmallVisualizer(Func<Vector2>? position = null)
		: base(position, () => new Vector2(25f, 12.5f))
	{
		MaxSize = () => base.Transform.Size.CachedValue.y * 4f;
		Blend = new State<SKColor>(() => new SKColor(196, 230, 253), this, this);
		Primary = new State<SKColor>(() => new SKColor(83, 135, 195), this, this);
		useAdaptiveAmplifier = new State<bool>(() => Settings.GetSetting("media.adaptive-visualizer-amplification", defaultValue: true), this, this);
		bars = new float[6];
		CreateVisualizer();
	}

	private void CreateVisualizer()
	{
		if (visualizer != null)
		{
			visualizer.Dispose();
		}
		visualizer = new AudioVisualizer(6);
		visualizer.DataAvailable += OnAudioDataAvailable;
		visualizer.StartCapture();
	}

	private void OnAudioDataAvailable(object? sender, float[] e)
	{
		lock (audioLock)
		{
			Array.Copy(e, pendingBars, pendingBars.Length);
			hasPendingAudio = true;
		}
	}

	private bool ApplyPendingAudio()
	{
		lock (audioLock)
		{
		if (!hasPendingAudio) return false;
		hasPendingAudio = false;
		float[] e = pendingBars;
		bool changed = false;
		float num = 0f;
		for (int i = 0; i < e.Length; i++)
		{
			float num2 = (float)Math.Log(e[i] * ((!useAdaptiveAmplifier.CachedValue) ? ManualAmplitudeMultiplier : AdaptiveAmplifierMultiplier) * 25f, (int)Math.Clamp(1.0 / Math.Pow(i + 1, 2.0) * 30.0, 4.0, 3.4028234663852886E+38));
			if (!float.IsFinite(num2)) num2 = 0f;
			changed |= bars[i] != num2;
			bars[i] = num2;
			float num3 = e[i];
			if (e[i] > 0.3f && AdaptiveAmplifierMultiplier >= 0.5f)
			{
				num3 = e[i] * ((e[i] > 0.5f) ? 6f : 3.5f);
			}
			num += num3;
		}
		num /= (float)e.Length;
		rawAvgVolume = num;
		return changed;
		}
	}

	public override void Dispose()
	{
		visualizer.Dispose();
		base.Dispose();
	}

	protected override void Update()
	{
		base.Update();
		if (ApplyPendingAudio() && UpdateRealtime)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
		restartTimer += FContext.DeltaTime;
		if (restartTimer > 300f)
		{
			restartTimer = 0f;
			CreateVisualizer();
		}
		averageVolume = RMath.Lerp(averageVolume, rawAvgVolume, FContext.DeltaTime * 1.4f);
		if (rawAvgVolume <= 0f)
		{
			AdaptiveAmplifierMultiplier = 0f;
		}
		else
		{
			AdaptiveAmplifierMultiplier = RMath.Clamp(1f / (averageVolume * 7f), 0f, 1.75f);
		}
		LastAdaptiveAmplifierMultiplier = AdaptiveAmplifierMultiplier;
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		int num = bars.Length - 1;
		float num2 = 2.5f;
		float width = base.Transform.Size.CachedValue.x / (float)num - num2;
		using SKPaint sKPaint = GetRenderPaint();
		for (int i = 0; i < num; i++)
		{
			float num3 = RMath.Remap(MathF.Sin(1.5f * FContext.Time * (float)Math.PI * (float)(i / 4)), -1f, 1f, 1f, 1.75f);
			float num4 = RMath.Clamp(base.Transform.Size.CachedValue.y * bars[i], MinSize, MaxSize()) * num3;
			SKRect rect = SKRect.Create(base.Shape.LocalBounds.Left + base.Shape.LocalBounds.Width / (float)num * (float)i + 0.5f, base.Shape.LocalBounds.Top + base.Shape.LocalBounds.Height / 2f - num4 / 2f, width, num4);
			using (SKShader shader = SKShader.CreateLinearGradient(new SKPoint(rect.MidX, rect.Top), new SKPoint(rect.MidX, rect.Bottom), new SKColor[2] { Blend.CachedValue, Primary.CachedValue }, null, SKShaderTileMode.Clamp))
			{
				sKPaint.Shader = shader;
			}
			using SKRoundRect rect2 = new SKRoundRect(rect, 10f);
			canvas.DrawRoundRect(rect2, sKPaint);
		}
	}
}
