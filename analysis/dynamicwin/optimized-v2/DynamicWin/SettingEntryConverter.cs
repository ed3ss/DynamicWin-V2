using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicWin;

public class SettingEntryConverter : JsonConverter<ISettingEntry>
{
	public override ISettingEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
		JsonElement rootElement = jsonDocument.RootElement;
		string text = rootElement.GetProperty("TypeName").GetString();
		new JsonSerializerOptions
		{
			IncludeFields = true,
			PropertyNameCaseInsensitive = true
		};
		if ((text ?? "").Contains("Boolean"))
		{
			return new SettingEntry<bool>(rootElement.GetProperty("TypedValue").GetBoolean());
		}
		if ((text ?? "").Contains("String"))
		{
			return new SettingEntry<string>(rootElement.GetProperty("TypedValue").GetString() ?? "");
		}
		if ((text ?? "").Contains("Single"))
		{
			return new SettingEntry<float>(rootElement.GetProperty("TypedValue").GetSingle());
		}
		if ((text ?? "").Contains("Int32"))
		{
			return new SettingEntry<int>(rootElement.GetProperty("TypedValue").GetInt32());
		}
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, ISettingEntry value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, value.GetType(), SerializationContext.Default);
	}
}
