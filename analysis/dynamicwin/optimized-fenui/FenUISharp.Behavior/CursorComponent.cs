using System;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public class CursorComponent : BehaviorComponent
{
	public Cursor CursorOnHover { get; set; }

	public CursorComponent(UIObject owner, Cursor cursorOnHover = Cursor.ARROW)
		: base(owner)
	{
		CursorOnHover = cursorOnHover;
		owner.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		InteractiveSurface interactiveSurface = owner.InteractiveSurface;
		interactiveSurface.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface.OnMouseEnter, new Action(MouseEnter));
		InteractiveSurface interactiveSurface2 = owner.InteractiveSurface;
		interactiveSurface2.OnMouseExit = (Action)Delegate.Combine(interactiveSurface2.OnMouseExit, new Action(MouseExit));
	}

	public override void ComponentDestroy()
	{
		base.ComponentDestroy();
		if (base.Owner != null)
		{
			InteractiveSurface interactiveSurface = base.Owner.InteractiveSurface;
			interactiveSurface.OnMouseEnter = (Action)Delegate.Remove(interactiveSurface.OnMouseEnter, new Action(MouseEnter));
			InteractiveSurface interactiveSurface2 = base.Owner.InteractiveSurface;
			interactiveSurface2.OnMouseExit = (Action)Delegate.Remove(interactiveSurface2.OnMouseExit, new Action(MouseExit));
		}
	}

	private void MouseEnter()
	{
		FContext.GetCurrentWindow().ActiveCursor.SetValue(this, CursorOnHover, 50f);
	}

	private void MouseExit()
	{
		FContext.GetCurrentWindow().ActiveCursor.DissolveValue(this);
	}
}
