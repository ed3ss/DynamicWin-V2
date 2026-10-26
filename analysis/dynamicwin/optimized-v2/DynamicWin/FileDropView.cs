using System.Collections.Generic;
using FenUISharp.Mathematics;
using FenUISharp.Objects;

namespace DynamicWin;

public class FileDropView : NotchView
{
	public override bool ForceExpanded => true;

	public override float ExpandedCornerRadius => 30f;

	public override Vector2 ExpandedIslandSize => new Vector2(380f, 105f);

	public FileDropView(Notch island)
		: base(island)
	{
	}

	public override List<UIObject> Create()
	{
		return new List<UIObject>
		{
			new TrayDropArea(() => new Vector2(-92f, 3f)),
			new ShareDropArea(() => new Vector2(92f, 3f))
		};
	}
}
