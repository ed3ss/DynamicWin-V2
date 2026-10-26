using System.Collections.Generic;
using FenUISharp.Objects;

namespace DynamicWin;

public interface ISettingsControlGroup
{
	List<UIObject> Create();
}
