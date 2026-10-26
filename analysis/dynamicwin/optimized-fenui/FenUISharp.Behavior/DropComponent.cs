using System;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace FenUISharp.Behavior;

public class DropComponent : BehaviorComponent
{
	private bool _windowHasCompatibleActiveDragAction;

	private bool _isCurrentlyInDragAction;

	private Dispatcher dispatcher;

	public DropType DropType { get; set; }

	public DROPEFFECT DropEffect { get; set; }

	public Action<FDropData>? OnDrop { get; set; }

	public Action<FDropData>? OnDragEnter { get; set; }

	public Action? OnDragStay { get; set; }

	public Action? OnDragLeave { get; set; }

	public DropComponent(UIObject owner, DropType dType = DropType.UnicodeText, DROPEFFECT dEffect = DROPEFFECT.Copy)
		: base(owner)
	{
		DropType = dType;
		DropEffect = dEffect;
		DragDropHandler? dropTarget = FContext.GetCurrentWindow().DropTarget;
		dropTarget.dragDrop = (Action<FDropData>)Delegate.Combine(dropTarget.dragDrop, new Action<FDropData>(DragDrop));
		DragDropHandler? dropTarget2 = FContext.GetCurrentWindow().DropTarget;
		dropTarget2.dragEnter = (Action<FDropData>)Delegate.Combine(dropTarget2.dragEnter, new Action<FDropData>(DragEnter));
		DragDropHandler? dropTarget3 = FContext.GetCurrentWindow().DropTarget;
		dropTarget3.dragLeave = (Action)Delegate.Combine(dropTarget3.dragLeave, new Action(DragLeave));
		DragDropHandler? dropTarget4 = FContext.GetCurrentWindow().DropTarget;
		dropTarget4.dragOver = (Action<FDropData>)Delegate.Combine(dropTarget4.dragOver, new Action<FDropData>(DragOver));
		if (base.Owner == null)
		{
			throw new InvalidOperationException("Owner can't be null");
		}
		base.Owner.InteractiveSurface.EnableMouseActions.SetStaticState(value: true, 25u);
		InteractiveSurface interactiveSurface = base.Owner.InteractiveSurface;
		interactiveSurface.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface.OnMouseEnter, new Action(MouseEnter));
		InteractiveSurface interactiveSurface2 = base.Owner.InteractiveSurface;
		interactiveSurface2.OnMouseExit = (Action)Delegate.Combine(interactiveSurface2.OnMouseExit, new Action(MouseExit));
		InteractiveSurface interactiveSurface3 = base.Owner.InteractiveSurface;
		interactiveSurface3.OnMouseMove = (Action<Vector2>)Delegate.Combine(interactiveSurface3.OnMouseMove, new Action<Vector2>(MouseStay));
		dispatcher = FContext.GetCurrentDispatcher();
		if (FContext.GetCurrentWindow().DropTarget.IsDragDropActionInProgress)
		{
			DragEnter(FContext.GetCurrentWindow().DropTarget.lastDropData);
		}
	}

	private void MouseStay(Vector2 vector)
	{
		if (base.Owner != null && base.Owner.GlobalEnabled && _windowHasCompatibleActiveDragAction)
		{
			OnDragStay?.Invoke();
		}
	}

	private void MouseExit()
	{
		if (base.Owner != null && base.Owner.GlobalEnabled && _windowHasCompatibleActiveDragAction)
		{
			_isCurrentlyInDragAction = false;
			OnDragDropActionLeave();
		}
	}

	private void MouseEnter()
	{
		if (base.Owner != null && base.Owner.GlobalEnabled && _windowHasCompatibleActiveDragAction)
		{
			_isCurrentlyInDragAction = true;
			OnDragDropActionEnter(FContext.GetCurrentWindow().DropTarget.lastDropData);
		}
	}

	private void DragDrop(FDropData? data)
	{
		if (base.Owner == null || !base.Owner.GlobalEnabled)
		{
			return;
		}
		if (base.Owner.InteractiveSurface.IsMouseHovering && _isCurrentlyInDragAction)
		{
			dispatcher.Invoke(delegate
			{
				OnDragDropActionComplete(data);
			});
		}
		_windowHasCompatibleActiveDragAction = false;
		_isCurrentlyInDragAction = false;
	}

	private void DragOver(FDropData? data)
	{
		if (data != null && IsSameType(data.dropType))
		{
			_windowHasCompatibleActiveDragAction = true;
		}
	}

	private void DragLeave()
	{
		_windowHasCompatibleActiveDragAction = false;
		_isCurrentlyInDragAction = false;
	}

	private void DragEnter(FDropData? data)
	{
		if (data != null && IsSameType(data.dropType))
		{
			_windowHasCompatibleActiveDragAction = true;
		}
	}

	protected void OnDragDropActionEnter(FDropData? data)
	{
		FContext.GetCurrentWindow().DropTarget.dropEffect.SetValue(this, DropEffect, 5f);
		if (data != null)
		{
			OnDragEnter?.Invoke(data);
		}
	}

	protected void OnDragDropActionLeave()
	{
		FContext.GetCurrentWindow().DropTarget.dropEffect.DissolveValue(this);
		OnDragLeave?.Invoke();
	}

	protected void OnDragDropActionComplete(FDropData? data)
	{
		FContext.GetCurrentWindow().DropTarget.dropEffect.DissolveValue(this);
		if (data != null)
		{
			OnDrop?.Invoke(data);
		}
	}

	private bool IsSameType(DropType otherType)
	{
		if (otherType == DropType)
		{
			return true;
		}
		if ((otherType == DropType.AnsiText || otherType == DropType.UnicodeText) && DropType == DropType.UnicodeText)
		{
			return true;
		}
		return false;
	}

	public override void ComponentDestroy()
	{
		base.ComponentDestroy();
		DragDropHandler? dropTarget = FContext.GetCurrentWindow().DropTarget;
		dropTarget.dragDrop = (Action<FDropData>)Delegate.Remove(dropTarget.dragDrop, new Action<FDropData>(DragDrop));
		DragDropHandler? dropTarget2 = FContext.GetCurrentWindow().DropTarget;
		dropTarget2.dragEnter = (Action<FDropData>)Delegate.Remove(dropTarget2.dragEnter, new Action<FDropData>(DragEnter));
		DragDropHandler? dropTarget3 = FContext.GetCurrentWindow().DropTarget;
		dropTarget3.dragLeave = (Action)Delegate.Remove(dropTarget3.dragLeave, new Action(DragLeave));
		if (base.Owner != null)
		{
			base.Owner.InteractiveSurface.EnableMouseActions.DissolvePriority(25u);
			InteractiveSurface interactiveSurface = base.Owner.InteractiveSurface;
			interactiveSurface.OnMouseEnter = (Action)Delegate.Remove(interactiveSurface.OnMouseEnter, new Action(MouseEnter));
			InteractiveSurface interactiveSurface2 = base.Owner.InteractiveSurface;
			interactiveSurface2.OnMouseExit = (Action)Delegate.Remove(interactiveSurface2.OnMouseExit, new Action(MouseExit));
			InteractiveSurface interactiveSurface3 = base.Owner.InteractiveSurface;
			interactiveSurface3.OnMouseMove = (Action<Vector2>)Delegate.Remove(interactiveSurface3.OnMouseMove, new Action<Vector2>(MouseStay));
		}
	}
}
