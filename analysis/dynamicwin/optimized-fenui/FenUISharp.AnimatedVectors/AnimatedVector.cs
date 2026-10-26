using System;
using System.Collections.Generic;
using SkiaSharp;

namespace FenUISharp.AnimatedVectors;

public class AnimatedVector : IDisposable
{
	public SKRect ViewBox { get; set; }

	public SKStrokeCap LineCap { get; set; }

	public SKStrokeJoin LineJoin { get; set; }

	public int ExtendBounds { get; set; }

	public List<AVPath> Paths { get; set; } = new List<AVPath>();

	public List<(string id, AVAnimation animation)> Animations { get; set; } = new List<(string, AVAnimation)>();

	public void Dispose()
	{
		Paths.ForEach(delegate(AVPath x)
		{
			x.Dispose();
		});
		Paths = new List<AVPath>();
	}
}
