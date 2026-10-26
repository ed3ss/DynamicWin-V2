using System;
using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using FenUISharp.Objects.Text.Rendering;
using SkiaSharp;

namespace FenUISharp.Objects.Text;

public class FText : UIObject
{
	protected TextModel _model;

	protected TextRenderer _renderer;

	protected TextLayout _layout;

	public TextModel Model
	{
		get
		{
			return _model;
		}
		set
		{
			TextModel model = _model;
			_model = value;
			if (model != _model)
			{
				OnModelChanged?.Invoke();
				OnAnyChange?.Invoke();
				Invalidate(Invalidation.All);
			}
		}
	}

	public TextRenderer Renderer
	{
		get
		{
			return _renderer;
		}
		set
		{
			_renderer = value;
			OnRendererChanged?.Invoke();
			OnAnyChange?.Invoke();
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public TextLayout LayoutModel
	{
		get
		{
			return _layout;
		}
		set
		{
			_layout = value;
			OnLayoutChanged?.Invoke();
			OnAnyChange?.Invoke();
			Invalidate(Invalidation.All);
		}
	}

	public Action? OnModelChanged { get; set; }

	public Action? OnRendererChanged { get; set; }

	public Action? OnLayoutChanged { get; set; }

	public Action? OnAnyChange { get; set; }

	public FText(TextModel model, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(position, size)
	{
		_model = model;
		_renderer = new TextRenderer(this);
		_layout = new WrapLayout(this);
		base.Padding.SetStaticState(10);
	}

	public void SilentSetModel(TextModel model)
	{
		TextModel model2 = _model;
		_model = model;
		if (model2.TextParts != _model.TextParts)
		{
			Invalidate(Invalidation.All);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		List<Glyph> glyphs = _layout.ProcessModel(_model, base.Shape.LocalBounds);
		using SKPaint paint = GetRenderPaint();
		_renderer.DrawText(canvas, _model, glyphs, base.Shape.SurfaceDrawRect, paint);
	}
}
