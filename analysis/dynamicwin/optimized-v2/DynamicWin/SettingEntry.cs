using System;
using System.Text.Json.Serialization;

namespace DynamicWin;

public class SettingEntry<T> : ISettingEntry
{
	[JsonInclude]
	public T TypedValue { get; set; }

	[JsonInclude]
	public string TypeName => typeof(T).AssemblyQualifiedName;

	[JsonIgnore]
	public object? Value => TypedValue;

	[JsonIgnore]
	public Type ValueType => typeof(T);

	public SettingEntry()
	{
	}

	public SettingEntry(T value)
	{
		TypedValue = value;
	}
}
