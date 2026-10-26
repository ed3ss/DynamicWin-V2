using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Behavior;

public class Rotation3DTransformComponent : BehaviorComponent
{
	public State<float> RotationX { get; init; }

	public State<float> RotationY { get; init; }

	public State<float> RotationZ { get; init; }

	public State<float> DepthScale { get; init; }

	public Rotation3DTransformComponent(UIObject owner)
		: base(owner)
	{
		RotationX = new State<float>(() => 0f, base.Owner, base.Owner);
		RotationY = new State<float>(() => 0f, base.Owner, base.Owner);
		RotationZ = new State<float>(() => 0f, base.Owner, base.Owner);
		DepthScale = new State<float>(() => 1f, base.Owner, base.Owner);
	}

	public override void HandleEvent(BehaviorEventType type, object? data = null)
	{
		base.HandleEvent(type, data);
		if ((!RMath.Approximately(RotationX.CachedValue, 0f) || !RMath.Approximately(RotationY.CachedValue, 0f) || !RMath.Approximately(RotationZ.CachedValue, 0f)) && type == BehaviorEventType.BeforeRender && data != null)
		{
			SKCanvas canvas = (SKCanvas)data;
			Apply3DRotationMatrix(canvas, RotationX.CachedValue, RotationY.CachedValue, RotationZ.CachedValue);
		}
	}

	public void Apply3DRotationMatrix(SKCanvas canvas, float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f, float depthScale = 1f)
	{
		float num = 3f * (0.1f / base.Owner.Layout.ApplyLayoutToSize(base.Owner.Transform.Size.CachedValue).Magnitude) / depthScale;
		SKMatrix first = SKMatrix.CreateIdentity();
		Vector2 pivot = base.Owner.Transform.Pivot;
		first = SKMatrix.Concat(first, SKMatrix.CreateTranslation(pivot.x, pivot.y));
		if (rotationZ != 0f)
		{
			first = SKMatrix.Concat(first, SKMatrix.CreateRotationDegrees(rotationZ));
		}
		if (rotationX != 0f)
		{
			float num2 = rotationX * (float)Math.PI / 180f;
			SKMatrix second = SKMatrix.CreateIdentity();
			second.ScaleY = (float)Math.Cos(num2);
			second.Persp1 = (0f - (float)Math.Sin(num2)) * num;
			first = SKMatrix.Concat(first, second);
		}
		if (rotationY != 0f)
		{
			float num3 = rotationY * (float)Math.PI / 180f;
			SKMatrix second2 = SKMatrix.CreateIdentity();
			second2.ScaleX = (float)Math.Cos(num3);
			second2.Persp0 = (float)Math.Sin(num3) * num;
			first = SKMatrix.Concat(first, second2);
		}
		canvas.Concat(SKMatrix.Concat(first, SKMatrix.CreateTranslation(0f - pivot.x, 0f - pivot.y)));
	}
}
