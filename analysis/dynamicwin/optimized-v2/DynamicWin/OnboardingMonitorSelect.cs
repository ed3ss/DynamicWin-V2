using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FenUISharp;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class OnboardingMonitorSelect : FenUISharp.Objects.View
{
	public override List<UIObject> Create()
	{
		FText fText = new FText(TextModelFactory.CreateBasic("Select your preferred monitor", 17f, bold: false, italic: false, underlined: false, () => SKColors.Gray, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), null, () => new Vector2(300f, 20f));
		fText.Layout.Alignment.SetStaticState(new Vector2(0f, 0f));
		fText.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0f));
		FPanel fPanel = new FPanel(() => new Vector2(0f, 0f), () => new Vector2(0f, 125f), 7.5f, () => new SKColor(100, 100, 100, 25));
		fPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		fPanel.Layout.MarginHorizontal.SetStaticState(15f);
		fPanel.CornerRadius.SetStaticState(25f);
		fPanel.Padding.SetStaticState(0);
		StackContentComponent stackContentComponent = new StackContentComponent(fPanel, StackContentComponent.ContentStackType.Horizontal, StackContentComponent.ContentStackBehavior.Scroll);
		stackContentComponent.Gap.SetStaticState(20f);
		stackContentComponent.Pad.SetStaticState(25f);
		FButtonGroup screenButtonGroup = new FButtonGroup();
		screenButtonGroup.AllowMultiSelect = false;
		screenButtonGroup.AlwaysMustSelectOne = false;
		int setting = Settings.GetSetting("general.active-display", 0);
		for (int num = 0; num < Screen.AllScreens.Length; num++)
		{
			_ = Screen.AllScreens[num];
			FImage fImage = new FImage(() => Resources.GetImage("monitor"));
			fImage.TintColor.SetResponsiveState(() => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.Primary.Saturate(0.3f).Multiply(1.35f));
			FLargeSelectButton fLargeSelectButton = new FLargeSelectButton(fImage, new FText(TextModelFactory.CreateBasic((num == 0) ? "Primary" : (num + 1).ToString())), null, null, () => new Vector2(0f, -25f));
			fLargeSelectButton.SetParent(fPanel);
			screenButtonGroup.Add(fLargeSelectButton);
			if (num == setting)
			{
				fLargeSelectButton.SetSelected(isSelected: true);
			}
		}
		FButtonGroup fButtonGroup = screenButtonGroup;
		fButtonGroup.OnUserSelectionChanged = (Action<bool[]>)Delegate.Combine(fButtonGroup.OnUserSelectionChanged, (Action<bool[]>)delegate
		{
			Settings.SetSetting("general.active-display", screenButtonGroup.LatestSelection);
		});
		screenButtonGroup.AlwaysMustSelectOne = true;
		stackContentComponent.FullUpdateLayout();
		return new List<UIObject> { fText, fPanel };
	}
}
