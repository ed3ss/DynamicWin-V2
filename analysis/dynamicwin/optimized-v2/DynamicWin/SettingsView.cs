using System.Collections.Generic;
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

public class SettingsView : View
{
	private FPanel? mainPanel;

	private ModelViewPane? mainPane;

	public string? OpenCategory { get; private set; }

	public override List<UIObject> Create()
	{
		List<UIObject> list = new List<UIObject>();
		float margin = 10f;
		float num = 5f;
		float rightSidePanelWidth = 175f;
		FPanel fPanel = new FPanel(() => new Vector2(margin, 0f), () => new Vector2(rightSidePanelWidth, 0f), 15f, () => new SKColor(100, 100, 100, 150));
		fPanel.Layout.StretchVertical.SetStaticState(value: true);
		fPanel.Layout.MarginVertical.SetStaticState(margin);
		fPanel.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		fPanel.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		FPanel leftSideSubPanel = new FPanel(() => new Vector2(0f, 0f), () => new Vector2(0f, 0f), 15f, () => SKColors.Transparent);
		leftSideSubPanel.SetParent(fPanel);
		leftSideSubPanel.Layout.StretchVertical.SetStaticState(value: true);
		leftSideSubPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		leftSideSubPanel.Layout.MarginVertical.SetStaticState(0f);
		leftSideSubPanel.Layout.MarginHorizontal.SetStaticState(0f);
		float num2 = 8f;
		StackContentComponent stackContentComponent = new StackContentComponent(leftSideSubPanel, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.Scroll);
		stackContentComponent.ContentClipBehaviorProvider = new ScaleContentClipBehavior(stackContentComponent)
		{
			ClipScale = new Vector2(0.8f, 0.2f)
		};
		stackContentComponent.Gap.SetStaticState(num2 / 2f);
		stackContentComponent.AlignInside = new Vector2(0.5f, 0f);
		stackContentComponent.ContentClip = true;
		FText fText = new FText(TextModelFactory.CreateBasic("Settings", 12f, bold: true, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start
		}), () => Vector2.Zero, () => new Vector2(0f, 20f));
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.MarginHorizontal.SetStaticState(num2 + 5f);
		fText.SetParent(leftSideSubPanel);
		FButtonGroup fButtonGroup = new FButtonGroup();
		fButtonGroup.AlwaysMustSelectOne = true;
		int num3 = 0;
		foreach (SettingsCategory c in Settings.Categories)
		{
			if (Settings.CategoryHeaders.TryGetValue(num3, out string value))
			{
				FText fText2 = new FText(TextModelFactory.CreateBasic(value, 12f, bold: true, italic: false, underlined: false, () => FContext.GetCurrentWindow().WindowThemeManager.CurrentTheme.OnSurface, null, new TextAlign
				{
					HorizontalAlign = TextAlign.AlignType.Start
				}), () => Vector2.Zero, () => new Vector2(0f, 20f));
				fText2.Layout.StretchHorizontal.SetStaticState(value: true);
				fText2.Layout.MarginHorizontal.SetStaticState(num2 + 5f);
				fText2.SetParent(leftSideSubPanel);
			}
			if (num3 == 8)
			{
				List<UIObject> list2 = new List<UIObject>();
				Settings.CreateSettingsControlSeparator(list2);
				list2.ForEach(delegate(UIObject x)
				{
					if (x.IsParentRoot)
					{
						x.SetParent(leftSideSubPanel);
					}
				});
			}
			IconButton iconButton = new IconButton(c.Name ?? "", () => c.Icon ?? Resources.GetImage("test-img"), delegate
			{
				ShowCategory(c.CategoryID);
			}, 125f, selectable: true);
			fButtonGroup.Add(iconButton);
			iconButton.Layout.StretchHorizontal.SetStaticState(value: true);
			iconButton.Layout.MarginHorizontal.SetStaticState(num2);
			iconButton.SetParent(leftSideSubPanel);
			num3++;
		}
		mainPanel = new FPanel(() => new Vector2(0f - margin, 0f), () => new Vector2(100f, 0f), 15f, () => new SKColor(100, 100, 100, 125));
		mainPanel.Layout.StretchVertical.SetStaticState(value: true);
		mainPanel.Layout.StretchHorizontal.SetStaticState(value: true);
		mainPanel.Layout.MarginVertical.SetStaticState(margin);
		mainPanel.Layout.MarginHorizontal.SetStaticState(rightSidePanelWidth / 2f + margin + num);
		mainPanel.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		mainPanel.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		mainPane = new ModelViewPane(null, () => Vector2.Zero, () => Vector2.Zero);
		mainPane.Layout.StretchHorizontal.SetStaticState(value: true);
		mainPane.Layout.StretchVertical.SetStaticState(value: true);
		mainPane.Layout.MarginHorizontal.SetStaticState(0f);
		mainPane.Layout.MarginVertical.SetStaticState(0f);
		mainPane.SetParent(mainPanel);
		ModelViewPane? owner = mainPane;
		Vector2? alignInside = new Vector2(0.5f, 0f);
		new StackContentComponent(owner, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.Scroll, null, alignInside).Gap.SetStaticState(10f);
		if (Settings.Categories.Count > 0)
		{
			ShowCategory(Settings.Categories[0].CategoryID);
		}
		list.Add(mainPanel);
		list.Add(fPanel);
		leftSideSubPanel.Layout.RecursivelyUpdateLayout();
		mainPane.Layout.RecursivelyUpdateLayout();
		return list;
	}

	internal void ShowCategory(string? id)
	{
		if (mainPane != null)
		{
			mainPane.ViewModel = new MainPanelView(id ?? "");
		}
		OpenCategory = id;
	}
}
