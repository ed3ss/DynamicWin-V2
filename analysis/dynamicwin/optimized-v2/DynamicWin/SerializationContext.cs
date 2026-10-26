using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DynamicWin;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, ISettingEntry>))]
[JsonSerializable(typeof(SettingEntry<bool>))]
[JsonSerializable(typeof(SettingEntry<int>))]
[JsonSerializable(typeof(SettingEntry<float>))]
[JsonSerializable(typeof(SettingEntry<string>))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.6610")]
public class SerializationContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<bool>? _Boolean;

	private JsonTypeInfo<float>? _Single;

	private JsonTypeInfo<ISettingEntry>? _ISettingEntry;

	private JsonTypeInfo<SettingEntry<bool>>? _SettingEntryBoolean;

	private JsonTypeInfo<SettingEntry<float>>? _SettingEntrySingle;

	private JsonTypeInfo<SettingEntry<int>>? _SettingEntryInt32;

	private JsonTypeInfo<SettingEntry<string>>? _SettingEntryString;

	private JsonTypeInfo<Dictionary<string, ISettingEntry>>? _DictionaryStringISettingEntry;

	private JsonTypeInfo<int>? _Int32;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public JsonTypeInfo<bool> Boolean => _Boolean ?? (_Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));

	public JsonTypeInfo<float> Single => _Single ?? (_Single = (JsonTypeInfo<float>)base.Options.GetTypeInfo(typeof(float)));

	public JsonTypeInfo<ISettingEntry> ISettingEntry => _ISettingEntry ?? (_ISettingEntry = (JsonTypeInfo<ISettingEntry>)base.Options.GetTypeInfo(typeof(ISettingEntry)));

	public JsonTypeInfo<SettingEntry<bool>> SettingEntryBoolean => _SettingEntryBoolean ?? (_SettingEntryBoolean = (JsonTypeInfo<SettingEntry<bool>>)base.Options.GetTypeInfo(typeof(SettingEntry<bool>)));

	public JsonTypeInfo<SettingEntry<float>> SettingEntrySingle => _SettingEntrySingle ?? (_SettingEntrySingle = (JsonTypeInfo<SettingEntry<float>>)base.Options.GetTypeInfo(typeof(SettingEntry<float>)));

	public JsonTypeInfo<SettingEntry<int>> SettingEntryInt32 => _SettingEntryInt32 ?? (_SettingEntryInt32 = (JsonTypeInfo<SettingEntry<int>>)base.Options.GetTypeInfo(typeof(SettingEntry<int>)));

	public JsonTypeInfo<SettingEntry<string>> SettingEntryString => _SettingEntryString ?? (_SettingEntryString = (JsonTypeInfo<SettingEntry<string>>)base.Options.GetTypeInfo(typeof(SettingEntry<string>)));

	public JsonTypeInfo<Dictionary<string, ISettingEntry>> DictionaryStringISettingEntry => _DictionaryStringISettingEntry ?? (_DictionaryStringISettingEntry = (JsonTypeInfo<Dictionary<string, ISettingEntry>>)base.Options.GetTypeInfo(typeof(Dictionary<string, ISettingEntry>)));

	public JsonTypeInfo<int> Int32 => _Int32 ?? (_Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static SerializationContext Default { get; } = new SerializationContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<bool> Create_Boolean(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<bool> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<bool>(options, JsonMetadataServices.BooleanConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<float> Create_Single(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<float> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<float>(options, JsonMetadataServices.SingleConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<ISettingEntry> Create_ISettingEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ISettingEntry> jsonTypeInfo))
		{
			JsonConverter converter = ExpandConverter(typeof(ISettingEntry), new SettingEntryConverter(), options);
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ISettingEntry>(options, converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<SettingEntry<bool>> Create_SettingEntryBoolean(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SettingEntry<bool>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SettingEntry<bool>> objectInfo = new JsonObjectInfoValues<SettingEntry<bool>>
			{
				ObjectCreator = () => new SettingEntry<bool>(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SettingEntryBooleanPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SettingEntry<bool>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SettingEntryBooleanPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<bool> propertyInfo = new JsonPropertyInfoValues<bool>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<bool>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<bool>)obj).TypedValue,
			Setter = delegate(object obj, bool value)
			{
				((SettingEntry<bool>)obj).TypedValue = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypedValue",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<bool>).GetProperty("TypedValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(bool), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<bool>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<bool>)obj).TypeName,
			Setter = delegate
			{
				throw new InvalidOperationException("The member 'SettingEntry.TypeName' has been annotated with the JsonIncludeAttribute but is not visible to the source generator.");
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypeName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<bool>).GetProperty("TypeName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		JsonPropertyInfoValues<object> propertyInfo3 = new JsonPropertyInfoValues<object>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<bool>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Value",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<bool>).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(object), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<Type> propertyInfo4 = new JsonPropertyInfoValues<Type>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<bool>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ValueType",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<bool>).GetProperty("ValueType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Type), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		return array;
	}

	private JsonTypeInfo<SettingEntry<float>> Create_SettingEntrySingle(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SettingEntry<float>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SettingEntry<float>> objectInfo = new JsonObjectInfoValues<SettingEntry<float>>
			{
				ObjectCreator = () => new SettingEntry<float>(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SettingEntrySinglePropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SettingEntry<float>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SettingEntrySinglePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<float> propertyInfo = new JsonPropertyInfoValues<float>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<float>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<float>)obj).TypedValue,
			Setter = delegate(object obj, float value)
			{
				((SettingEntry<float>)obj).TypedValue = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypedValue",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<float>).GetProperty("TypedValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(float), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<float>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<float>)obj).TypeName,
			Setter = delegate
			{
				throw new InvalidOperationException("The member 'SettingEntry.TypeName' has been annotated with the JsonIncludeAttribute but is not visible to the source generator.");
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypeName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<float>).GetProperty("TypeName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		JsonPropertyInfoValues<object> propertyInfo3 = new JsonPropertyInfoValues<object>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<float>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Value",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<float>).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(object), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<Type> propertyInfo4 = new JsonPropertyInfoValues<Type>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<float>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ValueType",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<float>).GetProperty("ValueType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Type), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		return array;
	}

	private JsonTypeInfo<SettingEntry<int>> Create_SettingEntryInt32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SettingEntry<int>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SettingEntry<int>> objectInfo = new JsonObjectInfoValues<SettingEntry<int>>
			{
				ObjectCreator = () => new SettingEntry<int>(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SettingEntryInt32PropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SettingEntry<int>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SettingEntryInt32PropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<int> propertyInfo = new JsonPropertyInfoValues<int>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<int>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<int>)obj).TypedValue,
			Setter = delegate(object obj, int value)
			{
				((SettingEntry<int>)obj).TypedValue = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypedValue",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<int>).GetProperty("TypedValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(int), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<int>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<int>)obj).TypeName,
			Setter = delegate
			{
				throw new InvalidOperationException("The member 'SettingEntry.TypeName' has been annotated with the JsonIncludeAttribute but is not visible to the source generator.");
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypeName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<int>).GetProperty("TypeName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		JsonPropertyInfoValues<object> propertyInfo3 = new JsonPropertyInfoValues<object>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<int>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Value",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<int>).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(object), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<Type> propertyInfo4 = new JsonPropertyInfoValues<Type>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<int>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ValueType",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<int>).GetProperty("ValueType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Type), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		return array;
	}

	private JsonTypeInfo<SettingEntry<string>> Create_SettingEntryString(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<SettingEntry<string>> jsonTypeInfo))
		{
			JsonObjectInfoValues<SettingEntry<string>> objectInfo = new JsonObjectInfoValues<SettingEntry<string>>
			{
				ObjectCreator = () => new SettingEntry<string>(),
				ObjectWithParameterizedConstructorCreator = null,
				PropertyMetadataInitializer = (JsonSerializerContext _) => SettingEntryStringPropInit(options),
				ConstructorParameterMetadataInitializer = null,
				ConstructorAttributeProviderFactory = () => typeof(SettingEntry<string>).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null),
				SerializeHandler = null
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] SettingEntryStringPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[4];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<string>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<string>)obj).TypedValue,
			Setter = delegate(object obj, string? value)
			{
				((SettingEntry<string>)obj).TypedValue = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypedValue",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<string>).GetProperty("TypedValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<string>),
			Converter = null,
			Getter = (object obj) => ((SettingEntry<string>)obj).TypeName,
			Setter = delegate
			{
				throw new InvalidOperationException("The member 'SettingEntry.TypeName' has been annotated with the JsonIncludeAttribute but is not visible to the source generator.");
			},
			IgnoreCondition = null,
			HasJsonInclude = true,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TypeName",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<string>).GetProperty("TypeName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		JsonPropertyInfoValues<object> propertyInfo3 = new JsonPropertyInfoValues<object>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<string>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Value",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<string>).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(object), Array.Empty<Type>(), null)
		};
		array[2] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo3);
		JsonPropertyInfoValues<Type> propertyInfo4 = new JsonPropertyInfoValues<Type>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(SettingEntry<string>),
			Converter = null,
			Getter = null,
			Setter = null,
			IgnoreCondition = JsonIgnoreCondition.Always,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ValueType",
			JsonPropertyName = null,
			AttributeProviderFactory = () => typeof(SettingEntry<string>).GetProperty("ValueType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(Type), Array.Empty<Type>(), null)
		};
		array[3] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo4);
		array[3].IsGetNullable = false;
		return array;
	}

	private JsonTypeInfo<Dictionary<string, ISettingEntry>> Create_DictionaryStringISettingEntry(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<Dictionary<string, ISettingEntry>> jsonTypeInfo))
		{
			JsonCollectionInfoValues<Dictionary<string, ISettingEntry>> collectionInfo = new JsonCollectionInfoValues<Dictionary<string, ISettingEntry>>
			{
				ObjectCreator = () => new Dictionary<string, ISettingEntry>(),
				SerializeHandler = DictionaryStringISettingEntrySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<string, ISettingEntry>, string, ISettingEntry>(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void DictionaryStringISettingEntrySerializeHandler(Utf8JsonWriter writer, Dictionary<string, ISettingEntry>? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		foreach (KeyValuePair<string, ISettingEntry> item in value)
		{
			writer.WritePropertyName(item.Key);
			JsonSerializer.Serialize(writer, item.Value, ISettingEntry);
		}
		writer.WriteEndObject();
	}

	private JsonTypeInfo<int> Create_Int32(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<int> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int>(options, JsonMetadataServices.Int32Converter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<string> Create_String(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<string> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter);
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	public SerializationContext()
		: base(null)
	{
	}

	public SerializationContext(JsonSerializerOptions options)
		: base(options)
	{
	}

	private static bool TryGetTypeInfoForRuntimeCustomConverter<TJsonMetadataType>(JsonSerializerOptions options, out JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
	{
		JsonConverter runtimeConverterForType = GetRuntimeConverterForType(typeof(TJsonMetadataType), options);
		if (runtimeConverterForType != null)
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, runtimeConverterForType);
			return true;
		}
		jsonTypeInfo = null;
		return false;
	}

	private static JsonConverter? GetRuntimeConverterForType(Type type, JsonSerializerOptions options)
	{
		for (int i = 0; i < options.Converters.Count; i++)
		{
			JsonConverter jsonConverter = options.Converters[i];
			if (jsonConverter != null && jsonConverter.CanConvert(type))
			{
				return ExpandConverter(type, jsonConverter, options, validateCanConvert: false);
			}
		}
		return null;
	}

	private static JsonConverter ExpandConverter(Type type, JsonConverter converter, JsonSerializerOptions options, bool validateCanConvert = true)
	{
		if (validateCanConvert && !converter.CanConvert(type))
		{
			throw new InvalidOperationException($"The converter '{converter.GetType()}' is not compatible with the type '{type}'.");
		}
		if (converter is JsonConverterFactory jsonConverterFactory)
		{
			converter = jsonConverterFactory.CreateConverter(type, options);
			if (converter == null || converter is JsonConverterFactory)
			{
				throw new InvalidOperationException($"The converter '{jsonConverterFactory.GetType()}' cannot return null or a JsonConverterFactory instance.");
			}
		}
		return converter;
	}

	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		base.Options.TryGetTypeInfo(type, out JsonTypeInfo typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(bool))
		{
			return Create_Boolean(options);
		}
		if (type == typeof(float))
		{
			return Create_Single(options);
		}
		if (type == typeof(ISettingEntry))
		{
			return Create_ISettingEntry(options);
		}
		if (type == typeof(SettingEntry<bool>))
		{
			return Create_SettingEntryBoolean(options);
		}
		if (type == typeof(SettingEntry<float>))
		{
			return Create_SettingEntrySingle(options);
		}
		if (type == typeof(SettingEntry<int>))
		{
			return Create_SettingEntryInt32(options);
		}
		if (type == typeof(SettingEntry<string>))
		{
			return Create_SettingEntryString(options);
		}
		if (type == typeof(Dictionary<string, ISettingEntry>))
		{
			return Create_DictionaryStringISettingEntry(options);
		}
		if (type == typeof(int))
		{
			return Create_Int32(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}
