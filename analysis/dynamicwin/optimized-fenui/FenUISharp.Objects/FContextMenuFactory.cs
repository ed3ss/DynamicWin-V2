using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Model;

namespace FenUISharp.Objects;

public static class FContextMenuFactory
{
	public static FPopupPanel CreateContextMenu()
	{
		FPopupPanel fPopupPanel = new FPopupPanel(() => new Vector2(120f, 200f), addLayout: true, hasTail: false, scaleAnimationFromZero: false);
		StackContentComponent stackContentComponent = new StackContentComponent(fPopupPanel, StackContentComponent.ContentStackType.Vertical, StackContentComponent.ContentStackBehavior.Scroll);
		stackContentComponent.Pad.SetStaticState(5f);
		stackContentComponent.Gap.SetStaticState(2.5f);
		new FContextMenuButton(new FText(TextModelFactory.CreateBasic("Test"))).SetParent(fPopupPanel);
		new FContextMenuButton(new FText(TextModelFactory.CreateBasic("1"))).SetParent(fPopupPanel);
		new FContextMenuButton(new FText(TextModelFactory.CreateBasic("awpdkapdokwpqokd"))).SetParent(fPopupPanel);
		stackContentComponent.FullUpdateLayout();
		Vector2 currentMousePoint = FContext.GetCurrentWindow().ClientMousePosition;
		fPopupPanel.Show(() => currentMousePoint);
		return fPopupPanel;
	}
}
