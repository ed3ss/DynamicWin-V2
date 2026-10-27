using System;
using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.States;
using SkiaSharp;

namespace DynamicWin;

public class Notch : UIObject
{
	private Vector2 _defaultContractedSize = new Vector2(150f, 30f);

	public bool IsNotchMode = true;

	public bool ClickToOpen = true;

	public float InsideMargin = 20f;

	public float HorizontalMarginMultiplier = 1.25f;

	protected float IslandTopOffset = 5f;

	protected float NotchOutwardCornerRadius = 5f;

	protected float NotchInwardCornerRadius = 20f;

	protected float NotchInwardCornerRadiusDivisor = 1f;

	private float currentNotchInwardCornerRadius = 1f;

	private Vector2 _springValues = new Vector2(2.7f, 1.6f);

	private float topOffset;

	private NotchView? beforeTempView;

	private bool openIslandFlag;

	private bool globalInsideFlag;

	private bool isNudgedOpen;

	private bool firstUpdate = true;
	private bool? requestedInvalidationLock;

	private float dragLocalXOffset;

	private Vector2 mousePositionBeforeDrag;

	private Spring dragStretchSpring = new Spring(1f, 2f);

	private Spring dragOffsetSpring = new Spring(1f, 2f);

	private float currentDropShadow = 5f;

	public bool isDemoNotch;

	public static Notch MainNotch { get; private set; }

	public Spring Spring { get; set; }

	public Vector2 CurrentSize { get; set; } = new Vector2(150f, 35f);

	public Vector2 ExpandedSize { get; set; } = new Vector2(300f, 100f);

	public Vector2 ContractedSize { get; set; } = new Vector2(150f, 30f);

	public float CornerRadius { get; set; } = 15f;

	public ModelViewPane ViewPane { get; protected set; }

	public float MarginDivisor { get; set; } = 1f;

	public float IsBigNotch
	{
		get
		{
			if (!base.IsDisposed)
			{
				return RMath.Remap(base.Transform.Size.CachedValue.y, ContractedSize.y, ExpandedSize.y, 0f, 1f);
			}
			return 0f;
		}
	}

	public Notch(bool isDemoNotch = false)
		: base(() => new Vector2(0f, 0f), () => Vector2.Zero)
	{
		if (MainNotch == null && !isDemoNotch)
		{
			MainNotch = this;
		}
		this.isDemoNotch = isDemoNotch;
		if (isDemoNotch)
		{
			topOffset = -100f;
		}
		base.Layout.Alignment.SetStaticState(new Vector2(0.5f, 0f));
		base.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0f));
		base.Transform.Anchor.SetStaticState(new Vector2(0.5f, 0f));
		Func<List<StateEntry<Vector2>>, StateEntry<Vector2>> resolver = delegate(List<StateEntry<Vector2>> entries)
		{
			Vector2 addition = Vector2.Zero;
			entries.ForEach(delegate(StateEntry<Vector2> x)
			{
				addition += x.Value();
			});
			return new StateEntry<Vector2>
			{
				Priority = 1u,
				Value = () => addition
			};
		};
		base.Transform.LocalPosition.SetResolver(resolver);
		Spring = new Spring(2.7f, 1.6f);
		Spring.ResetVector(CurrentSize);
		currentNotchInwardCornerRadius = NotchInwardCornerRadius;
		base.Padding.SetStaticState(50);
		ViewPane = new ModelViewPane(null, () => new Vector2(0f, 0f), () => Vector2.Zero);
		ViewPane.SetParent(this);
		ViewPane.Layout.StretchHorizontal.SetStaticState(value: true);
		ViewPane.Layout.StretchVertical.SetStaticState(value: true);
		ViewPane.DisableWhenOutOfParentBounds = false;
		base.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		base.InteractiveSurface.MouseInteractionCallbackOnChildMouseInteraction = true;
		InteractiveSurface interactiveSurface = base.InteractiveSurface;
		interactiveSurface.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface.OnMouseAction, new Action<MouseInputCode>(GlobalMouseAction));
		InteractiveSurface interactiveSurface2 = base.InteractiveSurface;
		interactiveSurface2.OnMouseMove = (Action<Vector2>)Delegate.Combine(interactiveSurface2.OnMouseMove, new Action<Vector2>(MouseMove));
		base.Transform.SnapPositionToPixelGrid.SetStaticState(value: false);
		base.InteractiveSurface.ExtendInteractionRadius.SetResponsiveState(() => (int)Settings.GetSetting("notch.interaction-radius-multiplier", 20f));
		Settings.SetListenerForSettingChange("notch.open-notch-on-hover", OptionChanged);
		Settings.SetListenerForSettingChange("notch.width-finetune", OptionChanged);
		Settings.SetListenerForSettingChange("notch.spring-finetune", OptionChanged);
		Settings.SetListenerForSettingChange("notch.speed-finetune", OptionChanged);
		Settings.SetListenerForSettingChange("notch.interaction-radius-multiplier", OptionChanged);
		Settings.SetListenerForSettingChange("general.always-update", delegate
		{
			ViewPane.RecursiveInvalidate(Invalidation.SurfaceDirty);
		});
		OptionChanged();
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			Invalidate(Invalidation.SurfaceDirty);
		}, 5uL);
	}

	private void OptionChanged()
	{
		ClickToOpen = !Settings.GetSetting("notch.open-notch-on-hover", defaultValue: false);
		float setting = Settings.GetSetting("notch.width-finetune", 1f);
		ContractedSize = new Vector2(_defaultContractedSize.x * setting, _defaultContractedSize.y);
		NotchInwardCornerRadiusDivisor = setting;
		float setting2 = Settings.GetSetting("notch.spring-finetune", 1.6f);
		float setting3 = Settings.GetSetting("notch.speed-finetune", 2f);
		_springValues = new Vector2(setting3, setting2);
		Invalidate(Invalidation.SurfaceDirty);
	}

	public void NudgeOpen()
	{
		openIslandFlag = true;
		globalInsideFlag = true;
		isNudgedOpen = true;
	}

	public void SetView(NotchView view)
	{
		if (!isDemoNotch)
		{
			ViewPane.ViewModel = view;
		}
	}

	public void SetTempView(NotchView view)
	{
		if (!isDemoNotch)
		{
			if (beforeTempView == null)
			{
				beforeTempView = (NotchView)ViewPane.ViewModel;
			}
			SetView(view);
		}
	}

	public void DissolveTempView(Action? onAnimComplete = null)
	{
		if (beforeTempView != null)
		{
			SetView(beforeTempView);
		}
		beforeTempView = null;
		if (onAnimComplete != null)
		{
			FContext.GetCurrentDispatcher().InvokeLater(onAnimComplete, ViewPane.AnimInDuration + ViewPane.AnimOutDuration);
		}
	}

	protected void GlobalMouseAction(MouseInputCode inputCode)
	{
		if (inputCode.button == MouseInputButton.Left && inputCode.state == MouseInputState.Down && ClickToOpen && globalInsideFlag && IsBigNotch < 0.5f)
		{
			CurrentSize = ContractedSize;
		}
		else if (inputCode.button == MouseInputButton.Left && inputCode.state == MouseInputState.Up && ClickToOpen && globalInsideFlag)
		{
			openIslandFlag = true;
		}
	}

	protected void MouseMove(Vector2 pos)
	{
		SKRect globalInteractionRect = base.InteractiveSurface.GetGlobalInteractionRect();
		SKRect globalInteractionRect2 = base.InteractiveSurface.GetGlobalInteractionRect();
		globalInteractionRect2.Inflate(15f, 15f);
		if (globalInteractionRect.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)))
		{
			isNudgedOpen = false;
		}
		if (!globalInsideFlag && globalInteractionRect.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)) && ViewPane.ViewModel is FileDropView)
		{
			DissolveTempView();
		}
		if (ClickToOpen)
		{
			if (!globalInsideFlag && globalInteractionRect.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)))
			{
				globalInsideFlag = true;
				bool flag = false;
				if (ViewPane.ViewModel != null)
				{
					flag = ((NotchView)ViewPane.ViewModel).ForceExpanded;
				}
				if (!flag && !base.InteractiveSurface.IsMouseDown)
				{
					CurrentSize = ContractedSize * 1.4f;
				}
			}
			if (!globalInteractionRect2.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)) && !isNudgedOpen && !base.InteractiveSurface.IsMouseDown)
			{
				openIslandFlag = false;
				globalInsideFlag = false;
			}
		}
		else if (globalInteractionRect.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)) && !globalInsideFlag)
		{
			openIslandFlag = true;
			globalInsideFlag = true;
		}
		else if (!globalInteractionRect2.Contains(new SKPoint(FContext.GetCurrentWindow().ClientMousePosition.x, FContext.GetCurrentWindow().ClientMousePosition.y)) && !isNudgedOpen && !base.InteractiveSurface.IsMouseDown)
		{
			openIslandFlag = false;
			globalInsideFlag = false;
		}
	}

	protected override void EarlyUpdate()
	{
		base.EarlyUpdate();
		base.Transform.Scale.SetProcessor((Vector2 x) => dragStretchSpring.Update(FContext.DeltaTime, x * 1000f) / 1000f);
		if (base.InteractiveSurface.IsMouseDown)
		{
			Vector2 vector = FContext.GetCurrentWindow().ClientMousePosition - mousePositionBeforeDrag;
			dragLocalXOffset = dragOffsetSpring.Update(FContext.DeltaTime, vector / 65f).x;
			vector /= 10000f;
			vector.x = MathF.Abs(vector.x);
			vector.y *= 1.5f;
			base.Transform.Scale.SetStaticState(Vector2.Clamp(Vector2.One + vector, Vector2.One * 0.75f, Vector2.One * 1.225f));
			if (!base.InteractiveSurface.IsMouseHovering)
			{
				NudgeOpen();
			}
		}
		else
		{
			dragLocalXOffset = dragOffsetSpring.Update(FContext.DeltaTime, Vector2.Zero).x;
			mousePositionBeforeDrag = FContext.GetCurrentWindow().ClientMousePosition;
			base.Transform.Scale.SetStaticState(Vector2.One);
		}
		base.Transform.LocalPosition.SetStaticState(new Vector2(dragLocalXOffset + (isDemoNotch ? 0 : ComfortRuntime.OffsetX), 0f), 10u);
		ViewPane.Layout.MarginHorizontal.SetStaticState(RMath.Remap(IsBigNotch, 0f, 1f, NotchInwardCornerRadius / 2.8f, InsideMargin / MarginDivisor));
		ViewPane.Layout.MarginVertical.SetStaticState(RMath.Remap(IsBigNotch, 0f, 1f, NotchInwardCornerRadius / 2.8f, InsideMargin / MarginDivisor));
		if (isDemoNotch)
		{
			base.Padding.SetStaticState(80);
		}
		if (firstUpdate && FContext.GetCurrentWindow().DropTarget != null)
		{
			DragDropHandler? dropTarget = FContext.GetCurrentWindow().DropTarget;
			dropTarget.dragEnter = (Action<FDropData>)Delegate.Combine(dropTarget.dragEnter, (Action<FDropData>)delegate
			{
				if (!(ViewPane?.ViewModel is FileDropView))
				{
					SetTempView(new FileDropView(this));
				}
			});
			DragDropHandler? dropTarget2 = FContext.GetCurrentWindow().DropTarget;
			dropTarget2.dragLeave = (Action)Delegate.Combine(dropTarget2.dragLeave, (Action)delegate
			{
				if (ViewPane?.ViewModel is FileDropView)
				{
					DissolveTempView();
				}
			});
			firstUpdate = false;
		}
		Spring.SetValues(_springValues.x, globalInsideFlag ? _springValues.y : Math.Clamp(_springValues.y / 1.4f, 0f, 2f));
		float to = RMath.Lerp(NotchInwardCornerRadius, CornerRadius, IsBigNotch);
		currentNotchInwardCornerRadius = RMath.Lerp(currentNotchInwardCornerRadius, to, FContext.DeltaTime * 10f);
		NotchOutwardCornerRadius = currentNotchInwardCornerRadius / 4f;
		base.Transform.LocalPosition.SetStaticState(new Vector2(base.Transform.LocalPosition.CachedValue.x, (IsNotchMode ? 0f : IslandTopOffset) + topOffset + (isDemoNotch ? 0 : ComfortRuntime.OffsetY)));
		base.ImageEffects.Opacity.SetStaticState(ComfortRuntime.Opacity);
		topOffset = RMath.Lerp(topOffset, 0f, FContext.DeltaTime * 4f);
		Vector2 cachedValue = base.Transform.Size.CachedValue;
		base.Transform.Size.SetStaticState(Spring.Update(FContext.DeltaTime, CurrentSize));
		bool flag = false;
		if (ViewPane.ViewModel != null)
		{
			flag = ((NotchView)ViewPane.ViewModel).ForceExpanded;
		}
		if (openIslandFlag || flag)
		{
			CurrentSize = ExpandedSize + Vector2.One * (InsideMargin - 15f);
		}
		else
		{
			CurrentSize = ContractedSize;
		}
		if (globalInsideFlag && !openIslandFlag && !flag)
		{
			CurrentSize *= 1.05f;
		}
		if (base.InteractiveSurface.IsMouseDown && !openIslandFlag && !flag)
		{
			CurrentSize = ContractedSize;
		}
		if (!isDemoNotch)
		{
			Vector2 vector2 = ((IsBigNotch > 0.1f) ? ExpandedSize : ContractedSize);
			SKRect currentRegion = SKRect.Create((0f - vector2.x) / 2f + base.Transform.LocalPosition.CachedValue.x, 0f + base.Transform.LocalPosition.CachedValue.y, vector2.x, vector2.y);
			int num = ((IsBigNotch > 0.05f || globalInsideFlag) ? 100 : ((int)Settings.GetSetting("notch.interaction-radius-multiplier", 20f)));
			currentRegion.Inflate(num, num);
			currentRegion = base.Transform.LocalToGlobal(currentRegion);
			FContext.GetCurrentWindow().Shape.AddOrUpdateWindowRegion(this, () => currentRegion);
		}
		if (Math.Round(cachedValue.Magnitude * 10f) / 10.0 != Math.Round(base.Transform.Size.CachedValue.Magnitude * 10f) / 10.0)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
		ViewPane.ImageEffects.Opacity.SetStaticState(RMath.Clamp(MathF.Max(RMath.Remap(IsBigNotch, 0f, 1f, 0.5f, 1f), (globalInsideFlag || Settings.GetSetting("general.always-update", defaultValue: true)) ? 1f : 0f), 0f, 1f));
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		bool alwaysUpdateI = !Settings.GetSetting("general.always-update", defaultValue: true);
		bool lockInvalidation = (double)IsBigNotch < 0.0005 && alwaysUpdateI;
		if (requestedInvalidationLock == lockInvalidation) return;
		requestedInvalidationLock = lockInvalidation;
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			if (!IsDisposed && requestedInvalidationLock == lockInvalidation)
				ViewPane.LockInvalidationAll = lockInvalidation;
		}, 2uL);
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		using SKPaint sKPaint = GetRenderPaint();
		canvas.Translate(0f, -1f);
		sKPaint.Color = ComfortRuntime.Background;
		SKRect localBounds = base.Shape.LocalBounds;
		localBounds.Inflate(75f, 75f);
		localBounds.Offset(0f, 75f);
		canvas.ClipRect(localBounds);
		if (isDemoNotch)
		{
			SKPaint sKPaint2 = sKPaint.Clone();
			sKPaint2.Color = FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.WithAlpha(25);
			sKPaint2.IsStroke = true;
			sKPaint2.StrokeWidth = 2f;
			sKPaint2.PathEffect = SKPathEffect.CreateDash(new float[2] { 7f, 7f }, 0f);
			using (SKRoundRect rect = new SKRoundRect(base.Transform.GlobalToDrawLocal(base.InteractiveSurface.GetGlobalInteractionRect()), base.InteractiveSurface.ExtendInteractionRadius.CachedValue))
			{
				canvas.DrawRoundRect(rect, sKPaint2);
			}
			sKPaint2.Dispose();
		}
		currentDropShadow = RMath.Lerp(currentDropShadow, GetDropShadowValue(), 8f * FContext.DeltaTime);
		using (SKImageFilter imageFilter = SKImageFilter.CreateDropShadow(0f, 3f, currentDropShadow, currentDropShadow, SKColors.Black.WithAlpha(155)))
		{
			sKPaint.ImageFilter = imageFilter;
		}
		if (IsNotchMode)
		{
			using SKPath path = OutwardsCornerRoundingPath(base.Shape.LocalBounds.Top, base.Shape.LocalBounds.Left, base.Shape.LocalBounds.Width, base.Shape.LocalBounds.Height);
			canvas.DrawPath(path, sKPaint);
		}
		SKRect localBounds2 = base.Shape.LocalBounds;
		if (IsNotchMode)
		{
			localBounds2.Inflate(0f, currentNotchInwardCornerRadius);
			localBounds2.Offset(0f, 0f - currentNotchInwardCornerRadius);
		}
		using SKPath path2 = SKSquircle.CreateSquircle(localBounds2, currentNotchInwardCornerRadius, 0.6f);
		canvas.DrawPath(path2, sKPaint);
	}

	public override void Dispose()
	{
		base.Dispose();
		Console.WriteLine("DISPOSING NOTCH");
		if (FContext.IsValidContext())
		{
			FContext.GetCurrentWindow().Shape.DissolveWindowRegion(this);
		}
	}

	public override void DrawChildren(SKCanvas? canvas)
	{
		using (SKPath path = SKSquircle.CreateSquircle(base.Shape.LocalBounds, currentNotchInwardCornerRadius, 0.6f))
		{
			canvas?.ClipPath(path);
		}
		base.DrawChildren(canvas);
	}

	private float GetDropShadowValue()
	{
		return RMath.Remap((globalInsideFlag ? 0.25f : 0f) + IsBigNotch / 1.5f, 0f, 1f, 5f, 18f);
	}

	protected SKPath OutwardsCornerRoundingPath(float top, float left, float width, float height)
	{
		SKPath sKPath = new SKPath();
		float notchOutwardCornerRadius = NotchOutwardCornerRadius;
		float num = Math.Min(NotchOutwardCornerRadius, height / 2f);
		float num2 = 0f;
		float num3 = 8f;
		sKPath.MoveTo(left, top + num);
		sKPath.CubicTo(new SKPoint(left, top + num), new SKPoint(left - notchOutwardCornerRadius / num3, top + num2), new SKPoint(left - notchOutwardCornerRadius, top));
		sKPath.LineTo(left + width + notchOutwardCornerRadius, top);
		sKPath.CubicTo(new SKPoint(left + width + notchOutwardCornerRadius, top), new SKPoint(left + width + notchOutwardCornerRadius / num3, top + num2), new SKPoint(left + width, top + num));
		sKPath.LineTo(left, top + num);
		sKPath.Close();
		return sKPath;
	}
}
