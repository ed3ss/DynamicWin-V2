using System;
using System.Collections.Generic;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;

namespace FenUISharp.Objects;

public class ModelViewPane : UIObject
{
	internal List<UIObject>? _modelItems;

	private View? _model;

	private AnimatorComponent _viewTransitionComponent;

	public View? ViewModel
	{
		get
		{
			return _model;
		}
		set
		{
			SetViewAnimated(value);
		}
	}

	public bool AnimateViewModelSwap { get; set; } = true;

	public Action<float>? OnAnimationValueUpdated { get; set; }

	public float AnimOutDuration { get; set; } = 0.25f;

	public float AnimInDuration { get; set; } = 0.25f;

	public ModelViewPane(View? model, Func<Vector2>? position = null, Func<Vector2>? size = null)
		: base(position, size)
	{
		if (model != null)
		{
			SilentSetView(model);
		}
		_viewTransitionComponent = new AnimatorComponent(this, Easing.EaseInCubic, Easing.EaseOutCubic);
		AnimatorComponent viewTransitionComponent = _viewTransitionComponent;
		viewTransitionComponent.OnValueUpdate = (Action<float>)Delegate.Combine(viewTransitionComponent.OnValueUpdate, (Action<float>)delegate(float x)
		{
			OnAnimationValueUpdated?.Invoke(x);
		});
		_viewTransitionComponent.Duration = 0.25f;
		OnAnimationValueUpdated = (Action<float>)Delegate.Combine(OnAnimationValueUpdated, (Action<float>)delegate(float x)
		{
			base.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(x, 0f, 1f, 1f, 0.95f));
			base.ImageEffects.Opacity.SetStaticState(1f - x);
		});
	}

	protected override void Update()
	{
		base.Update();
		_model?.Update();
	}

	private void SetViewAnimated(View? view)
	{
		if (view == null)
		{
			return;
		}
		Action action = delegate
		{
			SilentSetView(view);
			RecursiveInvalidate(Invalidation.All);
			_viewTransitionComponent.OnComplete = delegate
			{
				_viewTransitionComponent.Duration = (AnimateViewModelSwap ? AnimInDuration : 0f);
				_viewTransitionComponent.OnComplete = null;
				RecursiveInvalidate(Invalidation.All);
			};
			_viewTransitionComponent.Inverse = true;
			_viewTransitionComponent.Restart();
		};
		if (_model == null)
		{
			action();
			return;
		}
		_viewTransitionComponent.Duration = (AnimateViewModelSwap ? AnimOutDuration : 0f);
		_viewTransitionComponent.Inverse = false;
		_viewTransitionComponent.OnComplete = action;
		_viewTransitionComponent.Restart();
	}

	protected void UpdateView()
	{
		if (_model == null)
		{
			return;
		}
		_model.PaneRoot = this;
		_modelItems = _model.Create();
		_modelItems.ForEach(delegate(UIObject x)
		{
			if (x.Parent == null || x.Parent is ModelViewPane)
			{
				x.SetParent(this);
			}
		});
		_model.OnViewShown();
		base.Layout.RecursivelyUpdateLayout();
	}

	public void SilentSetView(View view)
	{
		DisposeItems();
		_model = view;
		UpdateView();
	}

	protected void DisposeItems()
	{
		if (_model != null)
		{
			_model.OnViewDestroyed();
			_modelItems?.ForEach(delegate(UIObject x)
			{
				x.Dispose();
			});
			_modelItems?.Clear();
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		DisposeItems();
	}
}
