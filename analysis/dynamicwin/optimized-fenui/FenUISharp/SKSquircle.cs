using System;
using SkiaSharp;

namespace FenUISharp;

public static class SKSquircle
{
	public static SKPath CreateSquircle(SKRect rect, float cornerRadius, float? squircleness = null)
	{
		SKPath path = new SKPath();
		path.Offset(rect.Left, rect.Top);
		squircleness = Math.Clamp(squircleness ?? FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.DefaultSuperellipseRatio, 0f, 1f);
		cornerRadius = Math.Min(cornerRadius, Math.Min(rect.Width, rect.Height) * 0.5f);
		float exponent = 2f + ((1f - squircleness) ?? FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.DefaultSuperellipseRatio) * 2f;
		int segments = 30;
		float left = rect.Left;
		float top = rect.Top;
		float right = rect.Right;
		float bottom = rect.Bottom;
		_ = rect.Width;
		_ = rect.Height;
		float cornerW = cornerRadius;
		float cornerH = cornerRadius;
		float num = left + cornerW;
		float num2 = top + cornerH;
		float num3 = right - cornerW;
		float num4 = bottom - cornerH;
		DrawCorner(num, num2, (float)Math.PI, moveTo: true);
		path.LineTo(num3, num2 - cornerH);
		DrawCorner(num3, num2, 4.712389f, moveTo: false);
		path.LineTo(num3 + cornerW, num4);
		DrawCorner(num3, num4, 0f, moveTo: false);
		path.LineTo(num, num4 + cornerH);
		DrawCorner(num, num4, (float)Math.PI / 2f, moveTo: false);
		path.Close();
		return path;
		void DrawCorner(float centerX, float centerY, float angleOffset, bool moveTo)
		{
			for (int i = 0; i <= segments; i++)
			{
				float num5 = (float)i / (float)segments * (float)Math.PI / 2f;
				float num6 = (float)Math.Cos(num5 + angleOffset);
				float num7 = (float)Math.Sin(num5 + angleOffset);
				float num8 = (float)MathF.Sign(num6) * MathF.Pow(Math.Abs(num6), 2f / exponent);
				float num9 = (float)MathF.Sign(num7) * MathF.Pow(Math.Abs(num7), 2f / exponent);
				float x = centerX + num8 * cornerW;
				float y = centerY + num9 * cornerH;
				if (i == 0 && moveTo)
				{
					path.MoveTo(x, y);
				}
				else
				{
					path.LineTo(x, y);
				}
			}
		}
	}
}
