using System.Collections.Generic;
using System.Linq;
using FenUISharp.Objects;
using SkiaSharp;

namespace FenUISharp.Materials;

public abstract class Material
{
	private Dictionary<string, object?> _props = new Dictionary<string, object>();

	public T GetProp<T>(string key, T defaultValue)
	{
		if (!_props.TryGetValue(key, out object value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		return (T)value;
	}

	public void SetProp(string key, object? value)
	{
		_props[key] = value;
	}

	public Material WithOverride(Dictionary<string, object> overrides)
	{
		Material material = (Material)MemberwiseClone();
		material._props = new Dictionary<string, object>(_props);
		foreach (KeyValuePair<string, object> @override in overrides)
		{
			material.SetProp(@override.Key, @override.Value);
		}
		return material;
	}

	protected SKPaint GetDefaultPaint()
	{
		return new SKPaint
		{
			IsAntialias = true,
			IsDither = true
		};
	}

	public void DrawWithMaterial(SKCanvas targetCanvas, SKRect rect, UIObject caller, SKPaint? paint = null)
	{
		using SKPath sKPath = new SKPath();
		sKPath.AddRect(rect);
		DrawWithMaterial(targetCanvas, sKPath, caller, paint);
	}

	public void DrawWithMaterial(SKCanvas targetCanvas, SKRoundRect rect, UIObject caller, SKPaint? paint = null)
	{
		using SKPath sKPath = new SKPath();
		sKPath.AddRoundRect(rect);
		DrawWithMaterial(targetCanvas, sKPath, caller, paint);
	}

	public void DrawWithMaterial(SKCanvas targetCanvas, SKPoint position, float radius, UIObject caller, SKPaint? paint = null)
	{
		using SKPath sKPath = new SKPath();
		sKPath.AddCircle(position.X, position.Y, radius);
		DrawWithMaterial(targetCanvas, sKPath, caller, paint);
	}

	public void DrawWithMaterial(SKCanvas targetCanvas, SKPath path, UIObject caller, SKPaint? paint = null)
	{
		using SKPath path2 = new SKPath(path);
		using SKPaint paint2 = paint?.Clone() ?? GetDefaultPaint();
		Draw(targetCanvas, path2, caller, paint2);
	}

	protected abstract void Draw(SKCanvas targetCanvas, SKPath path, UIObject caller, SKPaint paint);

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Material))
		{
			return false;
		}
		Material material = (Material)obj;
		if (!material._props.SequenceEqual(_props))
		{
			return false;
		}
		if (material.GetType().Name != GetType().Name)
		{
			return false;
		}
		return true;
	}
}
