using System;
using System.Collections.Generic;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;
using FenUISharp.States;
using SkiaSharp;

namespace FenUISharp.Objects;

public class FSegmentedControl : FPanel
{
	private SelectPaneModel _model;

	private Dictionary<FSegmentedSelectionPaneSelectableButton, Action<int>> instantiatedTextButtons = new Dictionary<FSegmentedSelectionPaneSelectableButton, Action<int>>();

	private StackContentComponent layout;

	private FButtonGroup buttonGroup;

	private SKPath? selectionPath;

	private Spring rectSpringXY;

	private Spring rectSpringWH;

	private int lastSelection = -1;

	private int lastIndex;

	private Func<SKRect> lastGlobalRect;

	public SelectPaneModel Model
	{
		get
		{
			return _model;
		}
		set
		{
			SetModel(value);
		}
	}

	public State<SKColor> Selection { get; init; }

	public State<SKColor> Background { get; init; }

	public State<SKColor> Text { get; init; }

	public State<SKColor> TextSelected { get; init; }

	public Action<int>? OnUserSelectionChanged { get; set; }

	public Action<int>? OnSelectionChanged { get; set; }

	public FSegmentedControl(SelectPaneModel model, int initiallySelected)
	{
		layout = new StackContentComponent(this, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.SizeToFitAll);
		layout.Pad.SetStaticState(4f);
		layout.Gap.SetStaticState(2f);
		rectSpringXY = new Spring(3f, 2f);
		rectSpringWH = new Spring(3f, 2f);
		buttonGroup = new FButtonGroup();
		buttonGroup.AlwaysMustSelectOne = true;
		buttonGroup.AllowMultiSelect = false;
		FButtonGroup fButtonGroup = buttonGroup;
		fButtonGroup.OnUserSelectionChanged = (Action<bool[]>)Delegate.Combine(fButtonGroup.OnUserSelectionChanged, (Action<bool[]>)delegate
		{
			OnSlcChanged(buttonGroup.LatestSelection);
		});
		FButtonGroup fButtonGroup2 = buttonGroup;
		fButtonGroup2.OnUserSelectionChanged = (Action<bool[]>)Delegate.Combine(fButtonGroup2.OnUserSelectionChanged, (Action<bool[]>)delegate
		{
			OnAnySlcChanged(buttonGroup.LatestSelection);
		});
		FButtonGroup fButtonGroup3 = buttonGroup;
		fButtonGroup3.OnSelectionChanged = (Action<bool[]>)Delegate.Combine(fButtonGroup3.OnSelectionChanged, (Action<bool[]>)delegate
		{
			OnSelectionChanged?.Invoke(buttonGroup.LatestSelection);
		});
		FButtonGroup fButtonGroup4 = buttonGroup;
		fButtonGroup4.OnSelectionChanged = (Action<bool[]>)Delegate.Combine(fButtonGroup4.OnSelectionChanged, (Action<bool[]>)delegate
		{
			OnAnySlcChanged(buttonGroup.LatestSelection);
		});
		base.InteractiveSurface.MouseInteractionCallbackOnChildMouseInteraction = true;
		Model = model;
		buttonGroup.Select(initiallySelected);
		lastGlobalRect = () => buttonGroup.Buttons[buttonGroup.LatestSelection].Shape.GlobalBounds;
		Selection = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Secondary, this, this);
		Background = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Surface, this, this);
		Text = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, this, this);
		TextSelected = new State<SKColor>(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, this, this);
		base.RenderMaterial.Value = () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.InteractableMaterial().WithOverride(new Dictionary<string, object> { ["BaseColor"] = (Func<SKColor>)(() => Background.CachedValue) });
	}

	public void SetSelected(int index)
	{
		buttonGroup.Buttons[index].SetSelected(isSelected: true);
	}

	public void SilentSetSelected(int index)
	{
		buttonGroup.Buttons[index].SilentSetSelected(isSelected: true);
		SKRect globalBounds = buttonGroup.Buttons[buttonGroup.LatestSelection].Shape.GlobalBounds;
		globalBounds.Inflate(-buttonGroup.Buttons[buttonGroup.LatestSelection].Padding.CachedValue, -buttonGroup.Buttons[buttonGroup.LatestSelection].Padding.CachedValue);
		SKRect sKRect = base.Transform.GlobalToDrawLocal(globalBounds);
		rectSpringXY.ResetVector(new Vector2(sKRect.Left, sKRect.Top));
		rectSpringWH.ResetVector(new Vector2(sKRect.Width, sKRect.Height));
		Invalidate(Invalidation.SurfaceDirty);
	}

	private void OnSlcChanged(int index)
	{
		_ = lastSelection;
		FSegmentedSelectionPaneSelectableButton key = (FSegmentedSelectionPaneSelectableButton)buttonGroup.Buttons[index];
		instantiatedTextButtons.TryGetValue(key, out Action<int> value);
		if (lastSelection != index)
		{
			value?.Invoke(index);
			OnSelectionChanged?.Invoke(index);
			lastSelection = index;
		}
	}

	private void OnAnySlcChanged(int index)
	{
		lastIndex = index;
		UpdateText();
	}

	private void UpdateText()
	{
		foreach (KeyValuePair<FSegmentedSelectionPaneSelectableButton, Action<int>> instantiatedTextButton in instantiatedTextButtons)
		{
			instantiatedTextButton.Key.Label.Invalidate(Invalidation.SurfaceDirty);
		}
	}

	private void OnMouseDownSubControl(int index)
	{
		lastIndex = index;
		UpdateText();
	}

	private void OnMouseEnterSubControl(FSegmentedSelectionPaneSelectableButton button)
	{
		lastGlobalRect = () => button.Shape.GlobalBounds;
		if (base.InteractiveSurface.IsMouseDown)
		{
			lastIndex = buttonGroup.Buttons.IndexOf(button);
			UpdateText();
		}
	}

	private void SetModel(SelectPaneModel value)
	{
		_model = value;
		ClearOld();
		int num = 0;
		foreach (KeyValuePair<TextModel, Action<int>> valuePair in _model.ValuePairs)
		{
			int capturedIndex = num;
			FSegmentedSelectionPaneSelectableButton fSegmentedSelectionPaneSelectableButton = new FSegmentedSelectionPaneSelectableButton(new FText(TextModelFactory.CopyBasic(valuePair.Key, null, null, null, null, () => (lastIndex != capturedIndex) ? Text.CachedValue : TextSelected.CachedValue)));
			instantiatedTextButtons.Add(fSegmentedSelectionPaneSelectableButton, valuePair.Value);
			fSegmentedSelectionPaneSelectableButton.SetParent(this);
			FSegmentedSelectionPaneSelectableButton capturedInstance = fSegmentedSelectionPaneSelectableButton;
			InteractiveSurface interactiveSurface = fSegmentedSelectionPaneSelectableButton.InteractiveSurface;
			interactiveSurface.OnMouseEnter = (Action)Delegate.Combine(interactiveSurface.OnMouseEnter, (Action)delegate
			{
				OnMouseEnterSubControl(capturedInstance);
			});
			InteractiveSurface interactiveSurface2 = fSegmentedSelectionPaneSelectableButton.InteractiveSurface;
			interactiveSurface2.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(interactiveSurface2.OnMouseAction, (Action<MouseInputCode>)delegate(MouseInputCode x)
			{
				if (x.state == MouseInputState.Down && x.button == MouseInputButton.Left)
				{
					OnMouseDownSubControl(capturedIndex);
				}
			});
			fSegmentedSelectionPaneSelectableButton.CornerRadius.SetResponsiveState(() => base.CornerRadius.CachedValue);
			buttonGroup.Add(fSegmentedSelectionPaneSelectableButton);
			num++;
		}
		layout.FullUpdateLayout();
	}

	private void ClearOld()
	{
		foreach (KeyValuePair<FSegmentedSelectionPaneSelectableButton, Action<int>> instantiatedTextButton in instantiatedTextButtons)
		{
			buttonGroup.Remove(instantiatedTextButton.Key);
			instantiatedTextButton.Key.Dispose();
		}
		instantiatedTextButtons = new Dictionary<FSegmentedSelectionPaneSelectableButton, Action<int>>();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		Vector2 lastValue = rectSpringXY.GetLastValue();
		SKRect local = (base.InteractiveSurface.IsMouseDown ? lastGlobalRect() : buttonGroup.Buttons[buttonGroup.LatestSelection].Shape.GlobalBounds);
		local.Inflate(-buttonGroup.Buttons[buttonGroup.LatestSelection].Padding.CachedValue, -buttonGroup.Buttons[buttonGroup.LatestSelection].Padding.CachedValue);
		SKRect sKRect = base.Transform.GlobalToDrawLocal(local);
		float num = (base.InteractiveSurface.IsMouseDown ? 4 : (base.InteractiveSurface.IsMouseHovering ? 0 : 0));
		Vector2 vector = rectSpringXY.Update(FContext.DeltaTime, new Vector2(sKRect.Left - num, 0f - num));
		Vector2 vector2 = rectSpringWH.Update(FContext.DeltaTime, new Vector2(sKRect.Width + num * 2f, sKRect.Height + num * 2f));
		SKRect rect = SKRect.Create(vector.x, sKRect.Top + vector.y, vector2.x, vector2.y);
		selectionPath?.Dispose();
		selectionPath = SKSquircle.CreateSquircle(rect, base.CornerRadius.CachedValue + (base.InteractiveSurface.IsMouseDown ? base.CornerRadius.CachedValue : 0f));
		if (lastValue.Magnitude != vector.Magnitude)
		{
			Invalidate(Invalidation.SurfaceDirty);
		}
	}

	public override void Render(SKCanvas canvas)
	{
		base.Render(canvas);
		if (selectionPath == null)
		{
			return;
		}
		using SKPaint paint = GetRenderPaint();
		base.RenderMaterial.CachedValue.WithOverride(new Dictionary<string, object> { ["BaseColor"] = (Func<SKColor>)(() => Selection.CachedValue) }).DrawWithMaterial(canvas, selectionPath, this, paint);
	}
}
