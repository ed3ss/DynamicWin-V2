using System;

namespace FenUISharp.Mathematics;

public class Easing
{
	public static Func<float, float> CombineInOut(Func<float, float> easeIn, Func<float, float> easeOut)
	{
		return (float x) => (x < 0.5f) ? (0.5f * easeIn(x * 2f)) : (0.5f * easeOut((x - 0.5f) * 2f) + 0.5f);
	}

	public static float EaseOutBack(float x)
	{
		return (float)(1.0 + 2.701580047607422 * Math.Pow(x - 1f, 3.0) + 1.7015800476074219 * Math.Pow(x - 1f, 2.0));
	}

	public static float EaseInBack(float x)
	{
		float num = 1.70158f;
		return (num + 1f) * x * x * x - num * x * x;
	}

	public static float EaseOutBackDramatic(float x)
	{
		return (float)(1.0 + 5.701580047607422 * Math.Pow(x - 1f, 3.0) + 4.701580047607422 * Math.Pow(x - 1f, 2.0));
	}

	public static float EaseInBackDramatic(float x)
	{
		float num = 4.70158f;
		return (num + 1f) * x * x * x - num * x * x;
	}

	public static float EaseOutIn(float x)
	{
		return 2.70158f * x * x * x - 1.70158f * x * x;
	}

	public static float EaseInQuint(float x)
	{
		return x * x * x * x * x;
	}

	public static float EaseOutQuint(float x)
	{
		return 1f - (float)Math.Pow(1f - x, 5.0);
	}

	public static float EaseOutSin(float x)
	{
		return (float)Math.Sin((double)x * Math.PI / 2.0);
	}

	public static float EaseInSin(float x)
	{
		return 1f - (float)Math.Cos((double)x * Math.PI / 2.0);
	}

	public static float EaseOutCubic(float x)
	{
		return 1f - (float)Math.Pow(1f - x, 3.0);
	}

	public static float EaseInCubic(float x)
	{
		return x * x * x;
	}

	public static float EaseOutQuad(float x)
	{
		return 1f - (1f - x) * (1f - x);
	}

	public static float EaseInQuad(float x)
	{
		return x * x;
	}

	public static float EaseInQuart(float x)
	{
		return 1f - (float)Math.Pow(1f - x, 4.0);
	}

	public static float EaseOutQuart(float x)
	{
		return x * x * x * x;
	}

	public static float EaseOutBounce(float x)
	{
		float num = 2.75f;
		float num2 = 7.5625f;
		if (x < 1f / num)
		{
			return num2 * x * x;
		}
		if (x < 2f / num)
		{
			x -= 1.5f / num;
			return num2 * x * x + 0.75f;
		}
		if ((double)x < 2.5 / (double)num)
		{
			x -= 2.25f / num;
			return num2 * x * x + 0.9375f;
		}
		x -= 2.625f / num;
		return num2 * x * x + 63f / 64f;
	}

	public static float EaseInBounce(float x)
	{
		return EaseOutBounce(1f - x);
	}

	public static float EaseOutElastic(float x)
	{
		float num = (float)Math.PI * 2f / 3f;
		return (float)((x == 0f) ? 0.0 : ((x == 1f) ? 1.0 : (Math.Pow(2.0, -10f * x) * Math.Sin(((double)(x * 10f) - 0.75) * (double)num) + 1.0)));
	}

	public static float EaseInElastic(float x)
	{
		float num = (float)Math.PI * 2f / 3f;
		return (float)((x == 0f) ? 0.0 : ((x == 1f) ? 1.0 : ((0.0 - Math.Pow(2.0, 10f * x - 10f)) * Math.Sin(((double)(x * 10f) - 10.75) * (double)num))));
	}

	public static float EaseOutLessElastic(float x)
	{
		float num = 1f;
		return (float)((x == 0f) ? 0.0 : ((x == 1f) ? 1.0 : (Math.Pow(2.0, -10f * x) * Math.Sin((((double)x - 0.1) * 10.0 - 0.75) * (double)num) + 1.0)));
	}

	public static float EaseOutExpo(float x)
	{
		if (x != 1f)
		{
			return 1f - (float)Math.Pow(2.0, -10f * x);
		}
		return 1f;
	}

	public static float EaseInExpo(float x)
	{
		if (x != 0f)
		{
			return (float)Math.Pow(2.0, 10f * x - 10f);
		}
		return 0f;
	}
}
