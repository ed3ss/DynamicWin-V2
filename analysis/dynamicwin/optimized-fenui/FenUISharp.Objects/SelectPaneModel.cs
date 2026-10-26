using System;
using System.Collections.Generic;
using FenUISharp.Objects.Text.Model;

namespace FenUISharp.Objects;

public struct SelectPaneModel
{
	public Dictionary<TextModel, Action<int>> ValuePairs { get; init; }

	public SelectPaneModel(Dictionary<TextModel, Action<int>> ValuePairs)
	{
		this.ValuePairs = ValuePairs;
	}
}
