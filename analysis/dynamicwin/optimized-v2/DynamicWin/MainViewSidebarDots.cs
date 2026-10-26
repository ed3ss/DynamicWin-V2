using System;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace DynamicWin;

public class MainViewSidebarDots : UIObject
{
	public float DotSize = 2f;

	public float DotsSpacing = 6f;

	public int DotsCount = 4;

	public int CurrentDot = 4;

	private AnimatorComponent anim;

	private float lastYPos = -1f;

	private float currentYPos = -1f;

	public MainViewSidebarDots()
	{
		anim = new AnimatorComponent(this, Easing.EaseInCubic);
		anim.Duration = 2f;
		AnimatorComponent animatorComponent = anim;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
		{
			base.ImageEffects.Opacity.SetStaticState((1f - x) / 2f, 2u);
		});
		base.ImageEffects.Opacity.SetResolver(StateResolverTemplates.SmallestFloatResolver);
		base.ImageEffects.Opacity.SetStaticState(0f, 2u);
	}

	protected override void Update()
	{
		base.Update();
		if (currentYPos == -1f)
		{
			currentYPos = lastYPos;
		}
		else
		{
			currentYPos = RMath.Lerp(currentYPos, lastYPos, FContext.DeltaTime * 10f);
		}
		if (!RMath.Approximately(currentYPos, lastYPos))
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		float num = (float)DotsCount * DotSize + (float)DotsCount * DotsSpacing;
		for (int i = 0; i < DotsCount; i++)
		{
			float cx = base.Shape.LocalBounds.Width / 2f - DotSize / 2f;
			float num2 = DotSize / 2f - num / 2f + ((float)i * DotSize + (float)i * DotsSpacing);
			if (i == CurrentDot)
			{
				lastYPos = num2;
				if (currentYPos != -1f)
				{
					num2 = currentYPos;
				}
			}
			num2 = base.Shape.LocalBounds.Height / 2f - num2;
			using SKPaint paint = new SKPaint
			{
				Color = SKColors.White.WithAlpha((byte)((i == CurrentDot) ? 200u : 75u))
			};
			canvas.DrawCircle(cx, num2, DotSize, paint);
		}
	}

	public void Show()
	{
		anim.Restart();
	}
}
