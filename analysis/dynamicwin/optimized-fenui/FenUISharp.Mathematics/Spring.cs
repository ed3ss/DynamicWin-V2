using System;

namespace FenUISharp.Mathematics;

public class Spring
{
	private Vector2 xp;

	private Vector2 y;

	private Vector2 yd;

	private float k1;

	private float k2;

	private float k3;

	public Guid Guid { get; init; } = Guid.NewGuid();

	public Spring(Vector2 startValue, float speed = 2f, float springy = 0.4f, float r = 0.1f)
	{
		springy = 1f / springy;
		k1 = (float)((double)springy / (Math.PI * (double)speed));
		k2 = (float)(1.0 / (Math.PI * 2.0 * (double)speed * (Math.PI * 2.0 * (double)speed)));
		k3 = (float)((double)(r * springy) / (Math.PI * 2.0 * (double)speed));
		xp = startValue;
		y = startValue;
		yd = new Vector2(0f, 0f);
	}

	public Spring(float speed = 2f, float springy = 0.4f)
	{
		springy = 1f / springy;
		Vector2 vector = new Vector2(0f, 0f);
		float num = 0.1f;
		k1 = (float)((double)springy / (Math.PI * (double)speed));
		k2 = (float)(1.0 / (Math.PI * 2.0 * (double)speed * (Math.PI * 2.0 * (double)speed)));
		k3 = (float)((double)(num * springy) / (Math.PI * 2.0 * (double)speed));
		xp = vector;
		y = vector;
		yd = new Vector2(0f, 0f);
	}

	public void ResetVector(Vector2 resetValue)
	{
		xp = resetValue;
		yd = resetValue;
		y = resetValue;
	}

	public void SetValues(float speed = 2f, float springy = 0.4f)
	{
		springy = 1f / springy;
		float num = 0.1f;
		k1 = (float)((double)springy / (Math.PI * (double)speed));
		k2 = (float)(1.0 / (Math.PI * 2.0 * (double)speed * (Math.PI * 2.0 * (double)speed)));
		k3 = (float)((double)(num * springy) / (Math.PI * 2.0 * (double)speed));
	}

	public Vector2 GetLastValue()
	{
		return y;
	}

	public Vector2 Update(float deltaTime, Vector2 x)
	{
		int num = (int)MathF.Min(MathF.Ceiling(deltaTime / 0.016f), 5f);
		float num2 = deltaTime / (float)num;
		for (int i = 0; i < num; i++)
		{
			float num3 = MathF.Max(k2, MathF.Max(num2 * num2 / 2f + num2 * k1 / 2f, num2 * k1));
			y += yd * num2;
			yd += num2 * (x + new Vector2(k3, k3) - y - k1 * yd) / num3;
		}
		y.x = RMath.LimitDecimalPoints(y.x, 1);
		y.y = RMath.LimitDecimalPoints(y.y, 1);
		return y;
	}
}
