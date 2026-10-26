using FenUISharp.Mathematics;
using FenUISharp.Objects;
using SkiaSharp;

namespace FenUISharp;

public class FSeparator : UIObject
{
	public FSeparator(bool isHorizontal = true)
		: base(null, () => new Vector2(2f, 2f))
	{
		if (isHorizontal)
		{
			base.Layout.StretchHorizontal.SetStaticState(value: true);
		}
		else
		{
			base.Layout.StretchVertical.SetStaticState(value: true);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKRoundRect rect = new SKRoundRect(base.Shape.LocalBounds, 2f);
		using SKPaint paint = GetRenderPaint();
		base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, rect, this, paint);
	}
}
