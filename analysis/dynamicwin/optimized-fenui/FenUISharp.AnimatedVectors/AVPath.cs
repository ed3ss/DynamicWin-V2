using System;
using SkiaSharp;

namespace FenUISharp.AnimatedVectors;

public class AVPath : IDisposable
{
	public SKPath SKPath { get; init; }

	public SKColor Stroke { get; init; }

	public SKColor Fill { get; init; }

	public float StrokeWidth { get; init; }

	public void Dispose()
	{
		SKPath.Dispose();
	}
}
