using System;
using FenUISharp;
using FenUISharp.AnimatedVectors;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace DynamicWin;

public class FAVControlButton : FAVDisplay
{
	private AnimatorComponent anim;

	public Action? OnClick { get; set; }

	public FAVControlButton(Func<AnimatedVector> fav, Action? onClick = null, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(fav, dynamicColor: false, position, size)
	{
		OnClick = onClick;
		anim = new AnimatorComponent(this, Easing.EaseOutCubic, Easing.EaseOutCubic);
		anim.Duration = 0.45f;
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: false);
		AnimatorComponent animatorComponent = anim;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
		{
			base.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(x, 0f, 1f, 0.725f, 1f));
		});
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, new Action<MouseInputCode>(MouseAction));
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnMouseExit = (Action)Delegate.Combine(interactiveSurface2.OnMouseExit, (Action)delegate
		{
			if (anim.Inverse)
			{
				anim.Inverse = false;
				anim.Restart();
			}
		});
		base.Padding.SetStaticState(0);
	}

	protected void MouseAction(MouseInputCode inputCode)
	{
		if (inputCode.button == MouseInputButton.Left && inputCode.state == MouseInputState.Up)
		{
			OnClick?.Invoke();
			PlayAnimation("action");
		}
		if (inputCode.button == MouseInputButton.Left && inputCode.state == MouseInputState.Up)
		{
			anim.Inverse = false;
			anim.Restart();
		}
		if (inputCode.button == MouseInputButton.Left && inputCode.state == MouseInputState.Down)
		{
			anim.Inverse = true;
			anim.Restart();
		}
	}
}
