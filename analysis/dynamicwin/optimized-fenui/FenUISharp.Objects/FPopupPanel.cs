using System;
using System.Linq;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.States;
using FenUISharp.WinFeatures;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FPopupPanel : FPanel, IStateListener
{
	private AnimatorComponent _inAnimation;

	private StackContentComponent? layout;

	private KeyBind closeKeybind;

	private Dispatcher dispatcher;

	private SKPath tailPath;

	private SKPath tailClip;

	private Vector2 lastPos;

	public bool HasTail { get; set; } = true;

	public float TailHeight { get; set; } = 10f;

	public float TailWidth { get; set; } = 7.5f;

	public float TailCornerRadius { get; set; } = 3.5f;

	private bool ScaleAnimationFromZero { get; set; } = true;

	public int DistanceToTarget { get; set; } = 15;

	public bool DisposeOnClose { get; set; }

	public bool AllowEscapeClosing { get; set; } = true;

	public State<Vector2> GlobalTargetPoint { get; init; }

	public SKRect GlobalBounds { get; set; }

	public bool IsShowing { get; private set; }

	public FPopupPanel(Func<Vector2> size, bool addLayout = true, bool hasTail = true, bool scaleAnimationFromZero = true)
		: base(() => new Vector2(0f, 0f), size)
	{
		GlobalTargetPoint = new State<Vector2>(() => new Vector2(0f, 0f), this, this);
		HasTail = hasTail;
		ScaleAnimationFromZero = scaleAnimationFromZero;
		closeKeybind = new KeyBind
		{
			VKCode = 27,
			OnKeybindExecuted = delegate
			{
				if (AllowEscapeClosing)
				{
					Close();
				}
			}
		};
		FContext.GetKeyboardInputManager().RegisterKeybind(closeKeybind);
		if (addLayout)
		{
			layout = new StackContentComponent(this, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.Scroll);
			layout.Pad.SetStaticState(5f);
			layout.Gap.SetStaticState(15f);
			layout.ContentFade = true;
			layout.FadeLength = 20f;
		}
		_inAnimation = new AnimatorComponent(this, Easing.EaseOutLessElastic, Easing.EaseInCubic);
		AnimatorComponent inAnimation = _inAnimation;
		inAnimation.OnValueUpdate = (Action<float>)Delegate.Combine(inAnimation.OnValueUpdate, (Action<float>)delegate(float x)
		{
			base.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(x, 0f, 1f, ScaleAnimationFromZero ? 0f : 0.6f, 1f));
		});
		AnimatorComponent inAnimation2 = _inAnimation;
		inAnimation2.OnComplete = (Action)Delegate.Combine(inAnimation2.OnComplete, (Action)delegate
		{
			OnCompleteAnim();
		});
		base.Visible.SetStaticState(value: false);
		base.Enabled.SetStaticState(value: false);
		base.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
		base.InteractiveSurface.IgnoreChildInteractions.SetStaticState(value: true);
		new LayoutObject(this).IgnoreParentLayout.SetStaticState(value: true);
		base.CornerRadius.SetStaticState(10f);
		base.Composition.LocalZIndex.SetStaticState(99);
		dispatcher = FContext.GetCurrentDispatcher();
		GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
		globalHooks.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(globalHooks.OnMouseAction, new Action<MouseInputCode>(OnGlobalMouseAction));
	}

	private void OnGlobalMouseAction(MouseInputCode code)
	{
		dispatcher.Invoke(delegate
		{
			if (code.button == MouseInputButton.Left && code.state == MouseInputState.Down && !_inAnimation.IsRunning && !base.Shape.GlobalBounds.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)))
			{
				Close();
			}
		});
	}

	public void ToggleShow(Func<Vector2>? targetPoint = null)
	{
		if (IsShowing)
		{
			Close();
		}
		else
		{
			Show(targetPoint);
		}
	}

	public void Show(Func<Vector2>? targetPoint = null)
	{
		if (!_inAnimation.IsRunning && !IsShowing)
		{
			base.Enabled.SetStaticState(value: true);
			FContext.GetCurrentDispatcher().InvokeLater(delegate
			{
				base.InteractiveSurface.IgnoreInteractions.SetStaticState(value: false);
				base.InteractiveSurface.IgnoreChildInteractions.SetStaticState(value: false);
				base.Visible.SetStaticState(value: true);
			}, 5uL);
			if (targetPoint != null)
			{
				GlobalTargetPoint.SetResponsiveState(targetPoint);
			}
			IsShowing = true;
			_inAnimation.OnComplete = delegate
			{
				OnCompleteAnim();
			};
			_inAnimation.Duration = 0.75f;
			_inAnimation.Inverse = false;
			_inAnimation.Restart();
			OnStartAnim();
		}
	}

	public void Close(Action? onComplete = null)
	{
		if (!IsShowing && !_inAnimation.IsRunning)
		{
			onComplete?.Invoke();
			return;
		}
		if (_inAnimation.IsRunning)
		{
			AnimatorComponent inAnimation = _inAnimation;
			inAnimation.OnComplete = (Action)Delegate.Combine(inAnimation.OnComplete, (Action)delegate
			{
				Close(onComplete);
			});
			return;
		}
		IsShowing = false;
		_inAnimation.Inverse = true;
		_inAnimation.Duration = 0.175f;
		_inAnimation.OnComplete = delegate
		{
			base.InteractiveSurface.IgnoreInteractions.SetStaticState(value: true);
			base.InteractiveSurface.IgnoreChildInteractions.SetStaticState(value: true);
			FContext.GetCurrentDispatcher().InvokeLater(delegate
			{
				base.Visible.SetStaticState(value: false);
				base.Enabled.SetStaticState(value: false);
			}, 1uL);
			onComplete?.Invoke();
			OnCompleteAnim();
			if (DisposeOnClose)
			{
				FContext.GetCurrentDispatcher().Invoke(delegate
				{
					Dispose();
				});
			}
		};
		_inAnimation.Restart();
	}

	private void OnCompleteAnim()
	{
		base.Children.ToList().ForEach(delegate(UIObject x)
		{
			x.Invalidate(Invalidation.All);
		});
	}

	private void OnStartAnim()
	{
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			base.Children.ToList().ForEach(delegate(UIObject x)
			{
				x.Invalidate(Invalidation.All);
			});
		}, 1uL);
		_inAnimation.OnComplete = OnCompleteAnim;
		layout?.FullUpdateLayout();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		GlobalBounds = new SKRect(0f, 0f, FContext.GetCurrentWindow().Shape.Bounds.Width, FContext.GetCurrentWindow().Shape.Bounds.Height);
		base.Transform.LocalPosition.SetStaticState(GetPanelPosition(base.Transform.GlobalToLocal(GlobalTargetPoint.CachedValue), base.Transform.GlobalToLocal(GlobalBounds), DistanceToTarget));
		GetTailSpecifics(out var basePoint, out var degrees);
		tailPath = GetTailPath(new SKPoint(basePoint.x, basePoint.y), degrees);
		tailClip = GetTailClip(new SKPoint(basePoint.x, basePoint.y), degrees);
		if (lastPos != base.Transform.LocalToGlobal(base.Transform.Position))
		{
			layout?.FullUpdateLayout();
			base.Children.ForEach(delegate(UIObject x)
			{
				x.Invalidate(Invalidation.TransformDirty);
			});
		}
		lastPos = base.Transform.LocalToGlobal(base.Transform.Position);
	}

	public Vector2 LocalToAnchor(Vector2 localPos)
	{
		float x = (localPos.x - base.Shape.SurfaceDrawRect.Left) / base.Shape.SurfaceDrawRect.Width;
		float y = (localPos.y - base.Shape.SurfaceDrawRect.Top) / base.Shape.SurfaceDrawRect.Height;
		return new Vector2(x, y);
	}

	private void GetTailSpecifics(out Vector2 basePoint, out float degrees)
	{
		Vector2 vector = base.Transform.GlobalToDrawLocal(GlobalTargetPoint.CachedValue);
		if (vector.x > base.Shape.LocalBounds.Right)
		{
			basePoint = new Vector2(base.Shape.LocalBounds.Right, RMath.Clamp(vector.y, base.Shape.LocalBounds.Top + base.CornerRadius.CachedValue + 5f, base.Shape.LocalBounds.Bottom - base.CornerRadius.CachedValue - 5f));
			degrees = -90f;
			base.Transform.Anchor.SetStaticState(LocalToAnchor(new Vector2(basePoint.x + 50f, basePoint.y)));
		}
		else if (vector.x < base.Shape.LocalBounds.Left)
		{
			basePoint = new Vector2(base.Shape.LocalBounds.Left, RMath.Clamp(vector.y, base.Shape.LocalBounds.Top + base.CornerRadius.CachedValue + 5f, base.Shape.LocalBounds.Bottom - base.CornerRadius.CachedValue - 5f));
			degrees = 90f;
			base.Transform.Anchor.SetStaticState(LocalToAnchor(new Vector2(basePoint.x - 50f, basePoint.y)));
		}
		else if (vector.y < base.Shape.LocalBounds.Top)
		{
			basePoint = new Vector2(RMath.Clamp(vector.x, base.Shape.LocalBounds.Left + base.CornerRadius.CachedValue + 5f, base.Shape.LocalBounds.Right - base.CornerRadius.CachedValue - 5f), base.Shape.LocalBounds.Top);
			degrees = 180f;
			base.Transform.Anchor.SetStaticState(LocalToAnchor(new Vector2(basePoint.x, basePoint.y - 50f)));
		}
		else
		{
			basePoint = new Vector2(base.Shape.LocalBounds.MidX, base.Shape.LocalBounds.Bottom);
			degrees = 0f;
			base.Transform.Anchor.SetStaticState(LocalToAnchor(new Vector2(basePoint.x, basePoint.y + 50f)));
		}
	}

	private Vector2 GetPanelPosition(Vector2 point, SKRect bounds, int distanceToTarget, int pad = 15)
	{
		Vector2 vector = point;
		Vector2 vector2 = new Vector2(0f, (0f - base.Shape.SurfaceDrawRect.Height) / 2f - (float)distanceToTarget);
		if (point.y - base.Shape.LocalBounds.Height / 2f - (float)pad + vector2.y < bounds.Top)
		{
			vector2.y = base.Shape.SurfaceDrawRect.Height / 2f + (float)distanceToTarget;
		}
		if (point.y + base.Shape.LocalBounds.Height / 2f + (float)pad > bounds.Bottom)
		{
			vector2.y = (0f - base.Shape.SurfaceDrawRect.Height) / 2f - (float)distanceToTarget;
		}
		if (point.x + base.Shape.LocalBounds.Width / 2f + (float)pad > bounds.Right)
		{
			vector2.x = (0f - base.Shape.SurfaceDrawRect.Width) / 2f - (float)distanceToTarget;
			vector2.y = 0f;
		}
		if (point.x - base.Shape.LocalBounds.Width / 2f - (float)pad < bounds.Left)
		{
			vector2.x = base.Shape.SurfaceDrawRect.Width / 2f + (float)distanceToTarget;
			vector2.y = 0f;
		}
		return Vector2.Clamp(vector + vector2, new Vector2(bounds.Left + (float)pad + base.Shape.LocalBounds.Width / 2f, bounds.Top + (float)pad + base.Shape.LocalBounds.Height / 2f), new Vector2(bounds.Right - (float)pad - base.Shape.LocalBounds.Width / 2f, bounds.Bottom - (float)pad - base.Shape.LocalBounds.Height / 2f));
	}

	public override void Render(SKCanvas canvas)
	{
		SKPaint renderPaint = GetRenderPaint();
		if (HasTail)
		{
			base.RenderMaterial.CachedValue.DrawWithMaterial(canvas, tailPath, this, renderPaint);
			canvas.ClipPath(tailPath, SKClipOperation.Difference, antialias: true);
			canvas.ClipPath(tailClip, SKClipOperation.Difference, antialias: true);
		}
		base.Render(canvas);
	}

	public SKPath GetTailClip(SKPoint middlePoint, float rotationDegrees = 0f)
	{
		return GetTailPath(middlePoint, rotationDegrees, isClip: true);
	}

	public SKPath GetTailPath(SKPoint middlePoint, float rotationDegrees = 0f, bool isClip = false)
	{
		SKPath sKPath = new SKPath();
		SKPoint point = new SKPoint(middlePoint.X - TailWidth, middlePoint.Y);
		SKPoint point2 = new SKPoint(middlePoint.X, middlePoint.Y + TailHeight);
		SKPoint point3 = new SKPoint(middlePoint.X + TailWidth, middlePoint.Y);
		SKPoint point4 = new SKPoint(point.X - TailCornerRadius, point.Y);
		SKPoint point5 = new SKPoint(point.X + TailCornerRadius * (point2.X - point.X) / (float)Math.Sqrt(Math.Pow(point2.X - point.X, 2.0) + Math.Pow(point2.Y - point.Y, 2.0)), point.Y + TailCornerRadius * (point2.Y - point.Y) / (float)Math.Sqrt(Math.Pow(point2.X - point.X, 2.0) + Math.Pow(point2.Y - point.Y, 2.0)));
		float num = (float)Math.Sqrt(Math.Pow(point2.X - point.X, 2.0) + Math.Pow(point2.Y - point.Y, 2.0));
		float num2 = (float)Math.Sqrt(Math.Pow(point3.X - point2.X, 2.0) + Math.Pow(point3.Y - point2.Y, 2.0));
		SKPoint point6 = new SKPoint(point2.X - TailCornerRadius * (point2.X - point.X) / num, point2.Y - TailCornerRadius * (point2.Y - point.Y) / num);
		SKPoint point7 = new SKPoint(point2.X + TailCornerRadius * (point3.X - point2.X) / num2, point2.Y + TailCornerRadius * (point3.Y - point2.Y) / num2);
		SKPoint point8 = new SKPoint(point3.X - TailCornerRadius * (point3.X - point2.X) / num2, point3.Y - TailCornerRadius * (point3.Y - point2.Y) / num2);
		SKPoint point9 = new SKPoint(point3.X + TailCornerRadius, point3.Y);
		sKPath.MoveTo(point4);
		sKPath.QuadTo(point, point5);
		sKPath.LineTo(point6);
		sKPath.QuadTo(point2, point7);
		sKPath.LineTo(point8);
		sKPath.QuadTo(point3, point9);
		if (isClip)
		{
			sKPath.Reset();
			sKPath.AddRect(new SKRect(base.Shape.SurfaceDrawRect.MidX - TailWidth - 0f - TailCornerRadius, base.Shape.LocalBounds.Bottom, base.Shape.SurfaceDrawRect.MidX + TailWidth + 0f + TailCornerRadius, base.Shape.LocalBounds.Bottom + 1.5f));
		}
		if (rotationDegrees != 0f)
		{
			SKMatrix first = SKMatrix.CreateIdentity();
			first = SKMatrix.Concat(first, SKMatrix.CreateTranslation(middlePoint.X, middlePoint.Y));
			first = SKMatrix.Concat(first, SKMatrix.CreateRotationDegrees(rotationDegrees));
			sKPath.Transform(in ILSpyHelper_AsRefReadOnly(SKMatrix.Concat(first, SKMatrix.CreateTranslation(0f - middlePoint.X, 0f - middlePoint.Y))));
		}
		return sKPath;
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp)
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
		globalHooks.OnMouseAction = (Action<MouseInputCode>)Delegate.Remove(globalHooks.OnMouseAction, new Action<MouseInputCode>(OnGlobalMouseAction));
		FContext.GetKeyboardInputManager().UnregisterKeybind(closeKeybind);
	}
}
