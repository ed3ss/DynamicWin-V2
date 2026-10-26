using System;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.States;
using FenUISharp.WinFeatures;
using SkiaSharp;

namespace FenUISharp.Objects.Buttons;

public abstract class Button : UIObject, IStateListener
{
	protected SelectableComponent selectableComponent;

	protected AnimatorComponent animatorComponent;

	internal SKColor currentHoverMix;

	private KeyBind spaceInteract;

	public Action? OnClick { get; set; }

	public State<float> CornerRadius { get; init; }

	public State<SKColor> HoverMix { get; init; }

	public float HoverPixelAddition { get; set; } = 1f;

	public Button(Action? onClick = null, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(position, size)
	{
		OnClick = onClick;
		spaceInteract = new KeyBind
		{
			VKCode = 32,
			OnKeybindExecuted = OnInteract
		};
		selectableComponent = new SelectableComponent(this, base.InteractiveSurface);
		SelectableComponent obj = selectableComponent;
		obj.OnSelectionGained = (Action)Delegate.Combine(obj.OnSelectionGained, (Action)delegate
		{
			FContext.GetKeyboardInputManager().RegisterKeybind(spaceInteract);
		});
		SelectableComponent obj2 = selectableComponent;
		obj2.OnSelectionLost = (Action)Delegate.Combine(obj2.OnSelectionLost, (Action)delegate
		{
			FContext.GetKeyboardInputManager().UnregisterKeybind(spaceInteract);
		});
		HoverMix = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.HoveredMix, this, this);
		base.RenderMaterial.Value = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.InteractableMaterial;
		currentHoverMix = SKColors.Transparent;
		CornerRadius = new State<float>(() => 10f, this, this);
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, new Action<MouseInputCode>(MouseAction));
		base.ImageEffects.Opacity.SetResponsiveState(() => (!base.InteractiveSurface.IgnoreInteractions.CachedValue) ? 1f : 0.35f, 10u);
		animatorComponent = new AnimatorComponent(this, Easing.EaseOutCubic);
		animatorComponent.Duration = 0.15f;
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface2.OnMouseEnter, new Action(MouseEnter));
		InteractiveSurface interactiveSurface3 = base.InteractiveSurface;
		interactiveSurface3.OnMouseExit = (Action)Delegate.Combine(interactiveSurface3.OnMouseExit, new Action(MouseExit));
		AnimatorComponent obj3 = animatorComponent;
		obj3.OnValueUpdate = (Action<float>)Delegate.Combine(obj3.OnValueUpdate, (Action<float>)delegate(float t)
		{
			SKColor to = RMath.Lerp(SKColors.Transparent, HoverMix.CachedValue, 0.1f);
			currentHoverMix = RMath.Lerp(SKColors.Transparent, to, t);
			float hoverPixelAddition = HoverPixelAddition;
			float x = (base.Layout.ClampSize(base.Transform.Size.CachedValue).x + hoverPixelAddition) / base.Layout.ClampSize(base.Transform.Size.CachedValue).x;
			float y = (base.Layout.ClampSize(base.Transform.Size.CachedValue).y + hoverPixelAddition / 2f) / base.Layout.ClampSize(base.Transform.Size.CachedValue).y;
			base.Transform.Scale.SetStaticState(Vector2.Lerp(new Vector2(1f, 1f), new Vector2(x, y), t));
			Invalidate(Invalidation.SurfaceDirty);
		});
		base.Padding.SetStaticState(10);
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: true);
		UpdateColors();
	}

	public override void OnInternalStateChanged<T>(T value)
	{
		base.OnInternalStateChanged(value);
		UpdateColors();
		Invalidate(Invalidation.SurfaceDirty);
	}

	protected void UpdateColors()
	{
		if (!animatorComponent.IsRunning)
		{
			SKColor to = RMath.Lerp(SKColors.Transparent, HoverMix.CachedValue, 0.1f);
			currentHoverMix = RMath.Lerp(SKColors.Transparent, to, base.InteractiveSurface.IsMouseHovering ? 1 : 0);
		}
	}

	protected virtual void MouseEnter()
	{
		if (!GlobalHooks.MouseDown)
		{
			animatorComponent.Inverse = false;
			animatorComponent.Start();
		}
	}

	protected virtual void MouseExit()
	{
		if (!GlobalHooks.MouseDown)
		{
			animatorComponent.Inverse = true;
			animatorComponent.Start();
		}
	}

	protected virtual void MouseAction(MouseInputCode inputCode)
	{
		if (inputCode.button == MouseInputButton.Left && inputCode.state == MouseInputState.Down)
		{
			animatorComponent.Inverse = true;
			animatorComponent.Restart();
		}
		else if (inputCode.button == MouseInputButton.Left && inputCode.state == MouseInputState.Up)
		{
			OnInteract();
			animatorComponent.Inverse = false;
			animatorComponent.Restart();
		}
	}

	protected virtual void OnInteract()
	{
		animatorComponent.Inverse = true;
		animatorComponent.Restart();
		OnClick?.Invoke();
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, CornerRadius.CachedValue);
		using SKPaint paint = GetRenderPaint();
		base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, path, this, paint);
	}

	public override void AfterRender(SKCanvas canvas)
	{
		base.AfterRender(canvas);
		using SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, CornerRadius.CachedValue);
		using SKPaint sKPaint = GetRenderPaint();
		sKPaint.Color = currentHoverMix;
		canvas.DrawPath(path, sKPaint);
	}
}
