using SkiaSharp;

namespace FenUISharp.AnimatedVectors;

public static class SKPathExtensions
{
	public static float ApproximateLength(this SKPath SKPath, float precision = 1f)
	{
		SKPathMeasure sKPathMeasure = new SKPathMeasure(SKPath);
		float num = 0f;
		do
		{
			num += sKPathMeasure.Length;
		}
		while (sKPathMeasure.NextContour());
		return num;
	}
}
