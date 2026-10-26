using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace DynamicWin;

public abstract class NotchView : View
{
	protected Notch RootNotch { get; private set; }

	public virtual Vector2 ExpandedIslandSize { get; set; } = new Vector2(300f, 75f);

	public virtual float ExpandedCornerRadius { get; set; } = 15f;

	public virtual bool ForceExpanded { get; set; }

	public virtual float MarginDivisor { get; set; } = 1f;

	public NotchView(Notch island)
	{
		RootNotch = island;
	}

	public override void OnViewShown()
	{
		base.OnViewShown();
		RootNotch.CornerRadius = ExpandedCornerRadius;
		RootNotch.ExpandedSize = ExpandedIslandSize;
		RootNotch.MarginDivisor = MarginDivisor;
	}
}
