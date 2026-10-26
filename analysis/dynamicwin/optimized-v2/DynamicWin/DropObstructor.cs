using FenUISharp.Objects;
using SkiaSharp;

namespace DynamicWin;

public class DropObstructor : UIObject
{
	public DropObstructor()
	{
		base.Layout.StretchHorizontal.SetStaticState(value: true);
		base.Layout.StretchVertical.SetStaticState(value: true);
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Color = SKColors.Black.WithAlpha(1);
		canvas.DrawRect(base.Shape.LocalBounds, sKPaint);
	}
}
