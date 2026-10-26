using System;
using FenUISharp.Mathematics;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects.Buttons;

public abstract class SelectableButton : Button
{
	private bool _isSelected;

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			SetSelected(value);
		}
	}

	public bool CanUnselect { get; set; } = true;

	public State<SKColor> EnabledFillColor { get; set; }

	public FButtonGroup? ButtonGroup { get; internal set; }

	public Action<bool, SelectableButton>? OnSelectionChanged { get; set; }

	public Action<bool, SelectableButton>? OnSelectionChangedSilent { get; set; }

	public Action<bool, SelectableButton>? OnUserSelectionChanged { get; set; }

	public SelectableButton(Action? onClick = null, Action<bool, SelectableButton>? onSelectionChanged = null, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(onClick, position, size)
	{
		OnSelectionChanged = onSelectionChanged;
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		EnabledFillColor = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary, this, this);
	}

	public void SetSelected(bool isSelected)
	{
		_isSelected = isSelected;
		OnSelectionChanged?.Invoke(isSelected, this);
		Invalidate(Invalidation.SurfaceDirty);
	}

	public void SilentSetSelected(bool isSelected)
	{
		_isSelected = isSelected;
		OnSelectionChangedSilent?.Invoke(isSelected, this);
		Invalidate(Invalidation.SurfaceDirty);
	}

	protected override void OnInteract()
	{
		base.OnInteract();
		IsSelected = !IsSelected || !CanUnselect || !IsSelected;
		OnUserSelectionChanged?.Invoke(IsSelected, this);
		Invalidate(Invalidation.SurfaceDirty);
	}

	public override void Render(SKCanvas canvas)
	{
		if (IsSelected)
		{
			using (SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, base.CornerRadius.CachedValue))
			{
				using SKPaint paint = GetRenderPaint();
				Func<SKColor> prop = base.RenderMaterial.CachedValue.GetProp<Func<SKColor>>("BaseColor", null);
				Func<SKColor> prop2 = base.RenderMaterial.CachedValue.GetProp<Func<SKColor>>("BorderColor", null);
				base.RenderMaterial.CachedValue.SetProp("BaseColor", (Func<SKColor>)(() => EnabledFillColor.CachedValue));
				base.RenderMaterial.CachedValue.SetProp("BorderColor", (Func<SKColor>)(() => EnabledFillColor.CachedValue.AddMix(new SKColor(25, 25, 25))));
				base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, path, this, paint);
				base.RenderMaterial.CachedValue.SetProp("BaseColor", prop);
				base.RenderMaterial.CachedValue.SetProp("BorderColor", prop2);
				return;
			}
		}
		base.Render(canvas);
	}
}
