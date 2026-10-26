using FenUISharp;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace DynamicWin;

public class ScreenCornerRounding : UIObject
{
	public State<float> CornerRadius { get; init; }

	public ScreenCornerRounding()
	{
		CornerRadius = new State<float>(() => 10f, this, this);
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, CornerRadius.CachedValue);
		canvas.ClipPath(path, SKClipOperation.Difference, antialias: true);
		canvas.Clear(SKColors.Black);
	}
}
