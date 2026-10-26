using System.Collections.Generic;
using FenUISharp;
using FenUISharp.Objects;

namespace DynamicWin;

public class DropObstructorView : View
{
	public override List<UIObject> Create()
	{
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			new DropObstructor();
		}, 1f);
		return new List<UIObject>();
	}
}
