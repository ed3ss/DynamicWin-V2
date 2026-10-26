using System;
using SkiaSharp;

namespace FenUISharp.Mathematics;

public static class RMath
{
	public static float Clamp(float value, float min, float max)
	{
		if (!(value < min))
		{
			if (!(value > max))
			{
				return value;
			}
			return max;
		}
		return min;
	}

	public static int Clamp(int value, int min, int max)
	{
		if (value >= min)
		{
			if (value <= max)
			{
				return value;
			}
			return max;
		}
		return min;
	}

	public static float Lerp(float from, float to, float t)
	{
		return from * (1f - t) + to * t;
	}

	public static byte Lerp(byte from, byte to, float t)
	{
		return (byte)((float)(int)from * (1f - t) + (float)(int)to * t);
	}

	public static SKColor Lerp(SKColor from, SKColor to, float t)
	{
		return new SKColor(Lerp(from.Red, to.Red, t), Lerp(from.Green, to.Green, t), Lerp(from.Blue, to.Blue, t), Lerp(from.Alpha, to.Alpha, t));
	}

	public static Vector2 RotateVector2(Vector2 point, Vector2 anchor, float d)
	{
		float num = d * (float)Math.PI / 180f;
		float num2 = point.x - anchor.x;
		float num3 = point.y - anchor.y;
		float num4 = num2 * (float)Math.Cos(num) - num3 * (float)Math.Sin(num);
		float num5 = num2 * (float)Math.Sin(num) + num3 * (float)Math.Cos(num);
		num4 += anchor.x;
		num5 += anchor.y;
		return new Vector2(num4, num5);
	}

	public static Vector2 ScaleVector2(Vector2 point, Vector2 anchor, float s)
	{
		return new Vector2((point.x - anchor.x) * s + anchor.x, (point.y - anchor.y) * s + anchor.y);
	}

	public static Vector2 ScaleVector2(Vector2 point, Vector2 anchor, Vector2 s)
	{
		return new Vector2((point.x - anchor.x) * s.x + anchor.x, (point.y - anchor.y) * s.y + anchor.y);
	}

	public static bool ContainsPoint(SKRect rect, Vector2 point)
	{
		if (rect.Left < point.x && rect.Top < point.y && rect.Left + rect.Width > point.x)
		{
			return rect.Top + rect.Height > point.y;
		}
		return false;
	}

	internal static float LimitDecimalPoints(float x, int v)
	{
		return (float)Math.Round(x, 2);
	}

	public static SKImage? CreateLowResImage(SKImage sourceImage, float scaleFactor, SKSamplingOptions samplingOptions)
	{
		if (sourceImage == null)
		{
			return null;
		}
		int num = (int)((float)sourceImage.Width * scaleFactor);
		int num2 = (int)((float)sourceImage.Height * scaleFactor);
		using SKSurface sKSurface = SKSurface.Create(new SKImageInfo(num, num2));
		if (sKSurface == null)
		{
			return sourceImage;
		}
		SKCanvas canvas = sKSurface.Canvas;
		if (canvas == null)
		{
			return sourceImage;
		}
		canvas.DrawImage(sourceImage, SKRect.Create(0f, 0f, sourceImage.Width, sourceImage.Height), SKRect.Create(0f, 0f, num, num2), samplingOptions);
		return sKSurface.Snapshot();
	}

	public static SKImage? Combine(SKImage sourceImage1, SKImage sourceImage2, SKSamplingOptions samplingOptions)
	{
		if (sourceImage1 == null && sourceImage2 == null)
		{
			return null;
		}
		if (sourceImage1 == null)
		{
			return sourceImage2;
		}
		if (sourceImage2 == null)
		{
			return sourceImage1;
		}
		bool flag = sourceImage1 != null && sourceImage1.Width > 0 && sourceImage1.Height > 0;
		bool flag2 = sourceImage2 != null && sourceImage2.Width > 0 && sourceImage2.Height > 0;
		if (!flag && !flag2)
		{
			return null;
		}
		if (!flag)
		{
			return sourceImage2;
		}
		if (!flag2)
		{
			return sourceImage1;
		}
		using SKSurface sKSurface = SKSurface.Create(new SKImageInfo(Clamp(sourceImage1.Width, 1, 99999), Clamp(sourceImage1.Height, 1, 99999)));
		SKCanvas canvas = sKSurface.Canvas;
		canvas.DrawImage(sourceImage1, SKRect.Create(0f, 0f, sourceImage1.Width, sourceImage1.Height), samplingOptions);
		canvas.DrawImage(sourceImage2, SKRect.Create(0f, 0f, sourceImage1.Width, sourceImage1.Height), samplingOptions);
		return sKSurface.Snapshot();
	}

	public static bool IsImageValid(SKImage image)
	{
		if (image != null && image.Handle != IntPtr.Zero && image.Width > 0)
		{
			return image.Height > 0;
		}
		return false;
	}

	public static bool IsRectFullyInside(SKRect outer, SKRect inner)
	{
		if (inner.Left >= outer.Left && inner.Top >= outer.Top && inner.Right <= outer.Right)
		{
			return inner.Bottom <= outer.Bottom;
		}
		return false;
	}

	public static bool IsRectPartiallyInside(SKRect outer, SKRect inner)
	{
		float num = Math.Max(outer.Left, inner.Left);
		float num2 = Math.Max(outer.Top, inner.Top);
		float num3 = Math.Min(outer.Right, inner.Right);
		float num4 = Math.Min(outer.Bottom, inner.Bottom);
		if (num < num3)
		{
			return num2 < num4;
		}
		return false;
	}

	public static bool IsRectPartiallyInside(SKRect rect, SKPath path)
	{
		SKPoint[] array = new SKPoint[9]
		{
			new SKPoint(rect.Left, rect.Top),
			new SKPoint(rect.Right, rect.Top),
			new SKPoint(rect.Right, rect.Bottom),
			new SKPoint(rect.Left, rect.Bottom),
			new SKPoint(rect.MidX, rect.Top),
			new SKPoint(rect.Right, rect.MidY),
			new SKPoint(rect.MidX, rect.Bottom),
			new SKPoint(rect.Left, rect.MidY),
			new SKPoint(rect.MidX, rect.MidY)
		};
		for (int i = 0; i < array.Length; i++)
		{
			SKPoint sKPoint = array[i];
			if (path.Contains(sKPoint.X, sKPoint.Y))
			{
				return true;
			}
		}
		using SKPath sKPath = new SKPath();
		sKPath.AddRect(rect);
		using SKPath sKPath2 = new SKPath();
		if (path.Op(sKPath, SKPathOp.Intersect, sKPath2) && !sKPath2.IsEmpty)
		{
			return true;
		}
		return false;
	}

	public static SKPoint RotatePoint(SKPoint point, SKPoint pivot, float degrees)
	{
		float num = degrees * ((float)Math.PI / 180f);
		float num2 = point.X - pivot.X;
		float num3 = point.Y - pivot.Y;
		float num4 = (float)Math.Cos(num);
		float num5 = (float)Math.Sin(num);
		float num6 = num2 * num4 - num3 * num5;
		float num7 = num2 * num5 + num3 * num4;
		return new SKPoint(num6 + pivot.X, num7 + pivot.Y);
	}

	public static float Remap(float t, float oldMin, float oldMax, float newMin, float newMax)
	{
		if (Math.Abs(oldMax - oldMin) < float.Epsilon)
		{
			return newMin;
		}
		float num = (t - oldMin) / (oldMax - oldMin);
		return newMin + num * (newMax - newMin);
	}

	public static bool Approximately(float value, float other)
	{
		return Approximately(value, other, 2f);
	}

	public static bool Approximately(float value, float other, float exponent)
	{
		return Math.Round(value * MathF.Pow(10f, exponent)) == Math.Round(other * MathF.Pow(10f, exponent));
	}

	public static float InverseLerp(float a, float b, float value)
	{
		if (a == b)
		{
			return 0f;
		}
		return (value - a) / (b - a);
	}
}
