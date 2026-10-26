using System;
using System.Text.Json.Serialization;

namespace DynamicWin;

[JsonConverter(typeof(SettingEntryConverter))]
public interface ISettingEntry
{
	object? Value { get; }

	[JsonIgnore]
	Type ValueType { get; }

	string TypeName { get; }
}
