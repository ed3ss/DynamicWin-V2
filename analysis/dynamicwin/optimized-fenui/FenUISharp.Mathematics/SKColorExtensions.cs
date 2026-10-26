using SkiaSharp;

namespace FenUISharp.Mathematics;

public static class SKColorExtensions
{
	public static SKColor MultiplyMix(this SKColor color1, SKColor color2)
	{
		return new SKColor((byte)((float)(int)color1.Red / 255f * ((float)(int)color2.Red / 255f) * 255f), (byte)((float)(int)color1.Green / 255f * ((float)(int)color2.Green / 255f) * 255f), (byte)((float)(int)color1.Blue / 255f * ((float)(int)color2.Blue / 255f) * 255f), (byte)((float)(int)color1.Alpha / 255f * ((float)(int)color2.Alpha / 255f) * 255f));
	}

	public static SKColor Multiply(this SKColor color1, (float, float, float, float) multiplier)
	{
		return new SKColor((byte)((float)(int)color1.Red * multiplier.Item1), (byte)((float)(int)color1.Green * multiplier.Item2), (byte)((float)(int)color1.Blue * multiplier.Item3), (byte)((float)(int)color1.Alpha * multiplier.Item4));
	}

	public static SKColor Multiply(this SKColor color1, float multiplier)
	{
		return color1.Multiply((multiplier, multiplier, multiplier, 1f));
	}

	public static SKColor AddMix(this SKColor color1, SKColor color2)
	{
		return new SKColor((byte)RMath.Clamp(((float)(int)color1.Red / 255f + (float)(int)color2.Red / 255f) * 255f, 0f, 255f), (byte)RMath.Clamp(((float)(int)color1.Green / 255f + (float)(int)color2.Green / 255f) * 255f, 0f, 255f), (byte)RMath.Clamp(((float)(int)color1.Blue / 255f + (float)(int)color2.Blue / 255f) * 255f, 0f, 255f), (byte)RMath.Clamp(((float)(int)color1.Alpha / 255f + (float)(int)color2.Alpha / 255f) * 255f, 0f, 255f));
	}

	public static SKColor Saturate(this SKColor color1, float saturation)
	{
		color1.ToHsl(out var h, out var s, out var l);
		return SKColor.FromHsl(h, RMath.Clamp(s * RMath.Clamp(saturation, 0f, 999f), 0f, 100f), l);
	}
}
