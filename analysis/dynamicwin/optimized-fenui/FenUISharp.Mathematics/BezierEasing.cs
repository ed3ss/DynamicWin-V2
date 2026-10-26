using System;

namespace FenUISharp.Mathematics;

public static class BezierEasing
{
	public static readonly Func<float, float> Ease = CreateEasing(0.25f, 0.1f, 0.25f, 1f);

	public static readonly Func<float, float> EaseIn = CreateEasing(0.42f, 0f, 1f, 1f);

	public static readonly Func<float, float> EaseOut = CreateEasing(0f, 0f, 0.58f, 1f);

	public static readonly Func<float, float> EaseInOut = CreateEasing(0.42f, 0f, 0.58f, 1f);

	public static Func<float, float> CreateEasing(float x1, float y1, float x2, float y2)
	{
		return (float t) => CubicBezier(t, x1, y1, x2, y2);
	}

	public static float CubicBezier(float t, float x1, float y1, float x2, float y2)
	{
		t = Math.Max(0f, Math.Min(1f, t));
		return CalculateBezierY(SolveForT(t, x1, x2), y1, y2);
	}

	private static float SolveForT(float x, float x1, float x2)
	{
		float num = 0f;
		float num2 = 1f;
		float num3 = x;
		for (int i = 0; i < 10; i++)
		{
			float num4 = CalculateBezierX(num3, x1, x2) - x;
			if (Math.Abs(num4) < 1E-06f)
			{
				break;
			}
			if (num4 > 0f)
			{
				num2 = num3;
			}
			else
			{
				num = num3;
			}
			num3 = (num + num2) * 0.5f;
		}
		return num3;
	}

	private static float CalculateBezierX(float t, float x1, float x2)
	{
		float num = 1f - t;
		return 3f * num * num * t * x1 + 3f * num * t * t * x2 + t * t * t;
	}

	private static float CalculateBezierY(float t, float y1, float y2)
	{
		float num = 1f - t;
		return 3f * num * num * t * y1 + 3f * num * t * t * y2 + t * t * t;
	}
}
