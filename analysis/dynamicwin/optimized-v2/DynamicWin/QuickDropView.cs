using System.Collections.Generic;
using FenUISharp.Objects;

namespace DynamicWin;

public class QuickDropView : View
{
	public override List<UIObject> Create()
	{
		return new List<UIObject>
		{
			new QuickDrop()
		};
	}
}
