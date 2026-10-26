using System;
using System.Diagnostics.CodeAnalysis;

namespace FenUISharp.Mathematics;

public struct Vector2
{
	public float x;

	public float y;

	public float Magnitude => (float)Math.Sqrt(Math.Abs(x * x + y * y));

	public Vector2 Swapped => new Vector2(y, x);

	public static Vector2 One => new Vector2(1f, 1f);

	public static Vector2 Zero => new Vector2(0f, 0f);

	public Vector2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2(Vector2 v)
	{
		x = v.x;
		y = v.y;
	}

	public void Swap()
	{
		float num = x;
		x = y;
		y = num;
	}

	public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
	{
		return new Vector2(RMath.Clamp(value.x, min.x, max.x), RMath.Clamp(value.y, min.y, max.y));
	}

	public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
	{
		return new Vector2(RMath.Lerp(from.x, to.x, t), RMath.Lerp(from.y, to.y, t));
	}

	public static Vector2 operator *(Vector2 c1, Vector2 c2)
	{
		return new Vector2(c1.x * c2.x, c1.y * c2.y);
	}

	public static Vector2 operator +(Vector2 c1, Vector2 c2)
	{
		return new Vector2(c1.x + c2.x, c1.y + c2.y);
	}

	public static Vector2 operator -(Vector2 c1, Vector2 c2)
	{
		return new Vector2(c1.x - c2.x, c1.y - c2.y);
	}

	public static Vector2 operator /(Vector2 c1, Vector2 c2)
	{
		return new Vector2(c1.x / c2.x, c1.y / c2.y);
	}

	public static Vector2 operator +(Vector2 c1, float c2)
	{
		return new Vector2(c1.x + c2, c1.y + c2);
	}

	public static Vector2 operator *(Vector2 c1, float c2)
	{
		return new Vector2(c1.x * c2, c1.y * c2);
	}

	public static Vector2 operator *(float c2, Vector2 c1)
	{
		return new Vector2(c1.x * c2, c1.y * c2);
	}

	public static Vector2 operator /(Vector2 c1, float c2)
	{
		return new Vector2(c1.x / c2, c1.y / c2);
	}

	public static Vector2 operator -(Vector2 c1, float c2)
	{
		return new Vector2(c1.x - c2, c1.y - c2);
	}

	public static Vector2 operator /(float c1, Vector2 c2)
	{
		return new Vector2(c1 / c2.x, c1 / c2.y);
	}

	public static Vector2 operator -(float c1, Vector2 c2)
	{
		return new Vector2(c1 - c2.x, c1 - c2.y);
	}

	public override readonly bool Equals([AllowNull] object obj)
	{
		if (obj is Vector2)
		{
			if (x == ((Vector2)obj).x)
			{
				return y == ((Vector2)obj).y;
			}
			return false;
		}
		return false;
	}

	public static bool operator ==(Vector2 c1, Vector2 c2)
	{
		if (c1.x == c2.x)
		{
			return c1.y == c2.y;
		}
		return false;
	}

	public static bool operator !=(Vector2 c1, Vector2 c2)
	{
		if (c1.x == c2.x)
		{
			return c1.y != c2.y;
		}
		return true;
	}

	public override string ToString()
	{
		return $"({x}, {y})";
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}
}
