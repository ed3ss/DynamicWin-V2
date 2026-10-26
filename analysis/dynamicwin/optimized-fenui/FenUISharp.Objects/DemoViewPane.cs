using System;
using System.Collections.Generic;
using FenUISharp.Behavior;
using FenUISharp.Materials;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects;

public class DemoViewPane : View
{
	public override List<UIObject> Create()
	{
		FPanel fPanel = new FPanel();
		fPanel.RenderMaterial.SetStaticState(new EmptyDefaultMaterial
		{
			BaseColor = () => SKColors.Transparent
		});
		fPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		fPanel.Layout.StretchVertical.SetStaticState(value: true);
		fPanel.Layout.MarginVertical.SetStaticState(25f);
		new StackContentComponent(fPanel, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.Scroll);
		new FSimpleButton(new FText(TextModelFactory.CreateBasic("")), delegate
		{
			FContextMenuFactory.CreateContextMenu();
		});
		new FSimpleButton(new FText(TextModelFactory.CreateBasic("")), delegate
		{
			new FPopupPanel(() => new Vector2(150f, 400f), addLayout: true, hasTail: false, scaleAnimationFromZero: false).Show(() => new Vector2(0f, 0f));
		}, () => new Vector2(200f, 0f));
		new FText(TextModelFactory.CreateBasic("Images", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel2 = new FPanel();
		fPanel2.SetParent(fPanel);
		StackContentComponent stackContentComponent = new StackContentComponent(fPanel2, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.SizeToFitAll);
		FImage fImage = new FImage(() => Resources.GetImage("test-img"), drawBackground: false, dynamicColor: false, null, () => new Vector2(75f, 75f));
		fImage.ScaleMode.SetStaticState(FImage.ImageScaleMode.Stretch);
		fImage.SetParent(fPanel2);
		FImage fImage2 = new FImage(() => Resources.GetImage("test-img"), drawBackground: false, dynamicColor: false, null, () => new Vector2(133f, 75f));
		fImage.ScaleMode.SetStaticState(FImage.ImageScaleMode.Contain);
		fImage2.SetParent(fPanel2);
		FImage fImage3 = new FImage(() => Resources.GetImage("test-img"), drawBackground: false, dynamicColor: false, null, () => new Vector2(75f, 75f));
		fImage.ScaleMode.SetStaticState(FImage.ImageScaleMode.Fit);
		fImage3.SetParent(fPanel2);
		stackContentComponent.FullUpdateLayout();
		new FText(TextModelFactory.CreateBasic("Text", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel3 = new FPanel();
		fPanel3.Transform.Size.SetStaticState(new Vector2(500f, 0f));
		fPanel3.SetParent(fPanel);
		StackContentComponent stackContentComponent2 = new StackContentComponent(fPanel3, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent2.Gap.SetStaticState(25f);
		stackContentComponent2.Pad.SetStaticState(30f);
		FText fText = new FText(TextModelFactory.CreateBasic("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est"), null, () => new Vector2(0f, 100f));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.MarginHorizontal.SetStaticState(65f);
		fText.Transform.Size.SetStaticState(new Vector2(0f, fText.LayoutModel.GetBoundingRect(fText.Model, SKRect.Create(fPanel3.Transform.Size.CachedValue.x, 250f)).Height));
		fText.SetParent(fPanel3);
		FText fText2 = new FText(TextModelFactory.CreateTest("Multiple text styles in one!"), null, () => new Vector2(0f, 100f));
		fText2.Layout.StretchHorizontal.SetStaticState(value: true);
		fText2.Layout.MarginHorizontal.SetStaticState(65f);
		fText2.Transform.Size.SetStaticState(new Vector2(0f, fText2.LayoutModel.GetBoundingRect(fText2.Model, SKRect.Create(fPanel3.Transform.Size.CachedValue.x, 250f)).Height));
		fText2.SetParent(fPanel3);
		FText fText3 = new FText(TextModelFactory.CreateBasic("Dynamic text processors"), null, () => new Vector2(0f, 100f));
		fText3.Layout.StretchHorizontal.SetStaticState(value: true);
		fText3.LayoutModel = new WiggleCharsLayoutProcessor(fText3, new WrapLayout(fText3));
		fText3.Layout.MarginHorizontal.SetStaticState(65f);
		fText3.Transform.Size.SetStaticState(new Vector2(0f, fText3.LayoutModel.GetBoundingRect(fText3.Model, SKRect.Create(fPanel3.Transform.Size.CachedValue.x, 250f)).Height));
		fText3.SetParent(fPanel3);
		FText text4 = new FText(TextModelFactory.CreateBasic("Text change animation (Text 1)"), null, () => new Vector2(0f, 100f));
		text4.Layout.StretchHorizontal.SetStaticState(value: true);
		text4.LayoutModel = new BlurLayoutProcessor(text4, new WrapLayout(text4));
		text4.Layout.MarginHorizontal.SetStaticState(65f);
		text4.Transform.Size.SetStaticState(new Vector2(0f, text4.LayoutModel.GetBoundingRect(text4.Model, SKRect.Create(fPanel3.Transform.Size.CachedValue.x, 250f)).Height));
		text4.SetParent(fPanel3);
		float val = 0f;
		int lastText = 0;
		FWindowCallbacks callbacks = FContext.GetCurrentWindow().Callbacks;
		callbacks.OnPreUpdate = (Action)Delegate.Combine(callbacks.OnPreUpdate, (Action)delegate
		{
			val = ((float)Math.Sin(FContext.Time) + 1f) / 2f + 1.5f;
			int num = (int)val;
			if (lastText != num)
			{
				text4.Model = TextModelFactory.CreateBasic($"{Math.Round(Random.Shared.NextSingle() * 10f) / 10.0} <- Random number\n Text change animation (Text {num})");
			}
			lastText = num;
		});
		stackContentComponent2.FullUpdateLayout();
		new FText(TextModelFactory.CreateBasic("Sliders / Scrollers", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel4 = new FPanel();
		fPanel4.Transform.Size.SetStaticState(new Vector2(500f, 0f));
		fPanel4.SetParent(fPanel);
		StackContentComponent stackContentComponent3 = new StackContentComponent(fPanel4, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent3.Pad.SetStaticState(25f);
		stackContentComponent3.Gap.SetStaticState(30f);
		new FSlider(null, 150f).SetParent(fPanel4);
		FSlider fSlider = new FSlider(null, 150f);
		fSlider.SnappingInterval = 0.25f;
		fSlider.KnobSize = new Vector2(fSlider.KnobSize.x / 2.5f, fSlider.KnobSize.y);
		fSlider.SetParent(fPanel4);
		FNumericScroller fNumericScroller = new FNumericScroller(new FText(TextModelFactory.CreateBasic("")), () => "C");
		fNumericScroller.MaxValue.SetStaticState(100f);
		fNumericScroller.Value = 10f;
		fNumericScroller.SetParent(fPanel4);
		stackContentComponent3.FullUpdateLayout();
		new FText(TextModelFactory.CreateBasic("Toggles", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel5 = new FPanel();
		fPanel5.Transform.Size.SetStaticState(new Vector2(500f, 0f));
		fPanel5.SetParent(fPanel);
		StackContentComponent stackContentComponent4 = new StackContentComponent(fPanel5, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent4.Pad.SetStaticState(25f);
		stackContentComponent4.Gap.SetStaticState(30f);
		new FRoundToggle().SetParent(fPanel5);
		new FToggle().SetParent(fPanel5);
		stackContentComponent4.FullUpdateLayout();
		new FText(TextModelFactory.CreateBasic("Button Groups", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel6 = new FPanel();
		fPanel6.SetParent(fPanel);
		StackContentComponent stackContentComponent5 = new StackContentComponent(fPanel6, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.SizeToFitAll);
		FRoundToggle fRoundToggle = new FRoundToggle();
		fRoundToggle.SetParent(fPanel6);
		FRoundToggle fRoundToggle2 = new FRoundToggle();
		fRoundToggle2.SetParent(fPanel6);
		FRoundToggle fRoundToggle3 = new FRoundToggle();
		fRoundToggle3.SetParent(fPanel6);
		FButtonGroup fButtonGroup = new FButtonGroup();
		fButtonGroup.Add(fRoundToggle);
		fButtonGroup.Add(fRoundToggle2);
		fButtonGroup.Add(fRoundToggle3);
		fButtonGroup.AllowMultiSelect = false;
		fButtonGroup.AlwaysMustSelectOne = true;
		stackContentComponent5.FullUpdateLayout();
		FPanel fPanel7 = new FPanel();
		fPanel7.SetParent(fPanel);
		StackContentComponent stackContentComponent6 = new StackContentComponent(fPanel7, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.SizeToFitAll);
		FRoundToggle fRoundToggle4 = new FRoundToggle();
		fRoundToggle4.SetParent(fPanel7);
		FRoundToggle fRoundToggle5 = new FRoundToggle();
		fRoundToggle5.SetParent(fPanel7);
		FRoundToggle fRoundToggle6 = new FRoundToggle();
		fRoundToggle6.SetParent(fPanel7);
		FButtonGroup fButtonGroup2 = new FButtonGroup();
		fButtonGroup2.Add(fRoundToggle4);
		fButtonGroup2.Add(fRoundToggle5);
		fButtonGroup2.Add(fRoundToggle6);
		fButtonGroup2.AllowMultiSelect = true;
		fButtonGroup2.AlwaysMustSelectOne = true;
		stackContentComponent6.FullUpdateLayout();
		FPanel fPanel8 = new FPanel();
		fPanel8.SetParent(fPanel);
		StackContentComponent stackContentComponent7 = new StackContentComponent(fPanel8, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.SizeToFitAll);
		FRoundToggle fRoundToggle7 = new FRoundToggle();
		fRoundToggle7.SetParent(fPanel8);
		FRoundToggle fRoundToggle8 = new FRoundToggle();
		fRoundToggle8.SetParent(fPanel8);
		FRoundToggle fRoundToggle9 = new FRoundToggle();
		fRoundToggle9.SetParent(fPanel8);
		FButtonGroup fButtonGroup3 = new FButtonGroup();
		fButtonGroup3.Add(fRoundToggle7);
		fButtonGroup3.Add(fRoundToggle8);
		fButtonGroup3.Add(fRoundToggle9);
		fButtonGroup3.AllowMultiSelect = false;
		fButtonGroup3.AlwaysMustSelectOne = false;
		stackContentComponent7.FullUpdateLayout();
		new FText(TextModelFactory.CreateBasic("Segmented Controls", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel9 = new FPanel();
		fPanel9.Transform.Size.SetStaticState(new Vector2(500f, 0f));
		fPanel9.SetParent(fPanel);
		StackContentComponent stackContentComponent8 = new StackContentComponent(fPanel9, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent8.Pad.SetStaticState(10f);
		stackContentComponent8.Gap.SetStaticState(5f);
		Dictionary<TextModel, Action<int>> dictionary = new Dictionary<TextModel, Action<int>>();
		TextModel key = TextModelFactory.CreateBasic("B", 14f, bold: true);
		dictionary[key] = delegate
		{
		};
		TextModel key2 = TextModelFactory.CreateBasic("U", 14f, bold: false, italic: false, underlined: true);
		dictionary[key2] = delegate
		{
		};
		TextModel key3 = TextModelFactory.CreateBasic("I", 14f, bold: false, italic: true);
		dictionary[key3] = delegate
		{
		};
		new FSegmentedControl(new SelectPaneModel(dictionary), 2).SetParent(fPanel9);
		Dictionary<TextModel, Action<int>> dictionary2 = new Dictionary<TextModel, Action<int>>();
		key3 = TextModelFactory.CreateBasic("Text 1");
		dictionary2[key3] = delegate
		{
		};
		key2 = TextModelFactory.CreateBasic("Text 2");
		dictionary2[key2] = delegate
		{
		};
		key = TextModelFactory.CreateBasic("Text 2");
		dictionary2[key] = delegate
		{
		};
		new FSegmentedControl(new SelectPaneModel(dictionary2), 2).SetParent(fPanel9);
		stackContentComponent8.FullUpdateLayout();
		new FText(TextModelFactory.CreateBasic("Other Controls", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel10 = new FPanel();
		fPanel10.Transform.Size.SetStaticState(new Vector2(500f, 0f));
		fPanel10.SetParent(fPanel);
		StackContentComponent stackContentComponent9 = new StackContentComponent(fPanel10, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent9.Pad.SetStaticState(25f);
		stackContentComponent9.Gap.SetStaticState(30f);
		new FColorPatch(SKColors.Red).SetParent(fPanel10);
		new FTextInputField(new FText(TextModelFactory.CreateBasic(""))).SetParent(fPanel10);
		FTextInputField fTextInputField = new FTextInputField(new FText(TextModelFactory.CreateBasic("")));
		fTextInputField.PlaceholderText = "Type in password...";
		fTextInputField.TextInputMode = FTextInputField.TextInputFieldMode.Password;
		fTextInputField.SetParent(fPanel10);
		stackContentComponent9.FullUpdateLayout();
		new FText(TextModelFactory.CreateBasic("Theme", 20f, bold: true), null, () => new Vector2(200f, 75f)).SetParent(fPanel);
		FPanel fPanel11 = new FPanel();
		fPanel11.Transform.Size.SetStaticState(new Vector2(500f, 0f));
		fPanel11.SetParent(fPanel);
		StackContentComponent stackContentComponent10 = new StackContentComponent(fPanel11, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.SizeToFit);
		stackContentComponent10.Pad.SetStaticState(10f);
		stackContentComponent10.Gap.SetStaticState(5f);
		Dictionary<TextModel, Action<int>> dictionary3 = new Dictionary<TextModel, Action<int>>();
		key = TextModelFactory.CreateBasic("Light");
		dictionary3[key] = delegate
		{
			FContext.GetCurrentWindow().WindowThemeManager.SetTheme(Resources.GetTheme("default-light"));
			FContext.GetCurrentWindow().Properties.UseSystemDarkMode = false;
		};
		key2 = TextModelFactory.CreateBasic("Dark");
		dictionary3[key2] = delegate
		{
			FContext.GetCurrentWindow().WindowThemeManager.SetTheme(Resources.GetTheme("default-dark"));
			FContext.GetCurrentWindow().Properties.UseSystemDarkMode = true;
		};
		new FSegmentedControl(new SelectPaneModel(dictionary3), 1).SetParent(fPanel11);
		stackContentComponent10.FullUpdateLayout();
		return new List<UIObject> { fPanel };
	}
}
