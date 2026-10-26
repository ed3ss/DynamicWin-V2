using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DynamicWin;

[JsonSerializable(typeof(UpdateManager.GitHubRelease))]
[JsonSerializable(typeof(UpdateManager.GitHubAsset[]))]
[GeneratedCode("System.Text.Json.SourceGeneration", "9.0.12.6610")]
public class GitHubSerializationContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<UpdateManager.GitHubAsset>? _GitHubAsset;

	private JsonTypeInfo<UpdateManager.GitHubAsset[]>? _GitHubAssetArray;

	private JsonTypeInfo<UpdateManager.GitHubRelease>? _GitHubRelease;

	private JsonTypeInfo<string>? _String;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions();

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_name = JsonEncodedText.Encode("name");

	private static readonly JsonEncodedText PropName_browser_download_url = JsonEncodedText.Encode("browser_download_url");

	private static readonly JsonEncodedText PropName_tag_name = JsonEncodedText.Encode("tag_name");

	private static readonly JsonEncodedText PropName_assets = JsonEncodedText.Encode("assets");

	public JsonTypeInfo<UpdateManager.GitHubAsset> GitHubAsset => _GitHubAsset ?? (_GitHubAsset = (JsonTypeInfo<UpdateManager.GitHubAsset>)base.Options.GetTypeInfo(typeof(UpdateManager.GitHubAsset)));

	public JsonTypeInfo<UpdateManager.GitHubAsset[]> GitHubAssetArray => _GitHubAssetArray ?? (_GitHubAssetArray = (JsonTypeInfo<UpdateManager.GitHubAsset[]>)base.Options.GetTypeInfo(typeof(UpdateManager.GitHubAsset[])));

	public JsonTypeInfo<UpdateManager.GitHubRelease> GitHubRelease => _GitHubRelease ?? (_GitHubRelease = (JsonTypeInfo<UpdateManager.GitHubRelease>)base.Options.GetTypeInfo(typeof(UpdateManager.GitHubRelease)));

	public JsonTypeInfo<string> String => _String ?? (_String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));

	public static GitHubSerializationContext Default { get; } = new GitHubSerializationContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<UpdateManager.GitHubAsset> Create_GitHubAsset(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<UpdateManager.GitHubAsset> jsonTypeInfo))
		{
			JsonObjectInfoValues<UpdateManager.GitHubAsset> objectInfo = new JsonObjectInfoValues<UpdateManager.GitHubAsset>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new UpdateManager.GitHubAsset((string)args[0], (string)args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => GitHubAssetPropInit(options),
				ConstructorParameterMetadataInitializer = GitHubAssetCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(UpdateManager.GitHubAsset).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(string),
					typeof(string)
				}, null),
				SerializeHandler = GitHubAssetSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] GitHubAssetPropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UpdateManager.GitHubAsset),
			Converter = null,
			Getter = (object obj) => ((UpdateManager.GitHubAsset)obj).Name,
			Setter = delegate(object obj, string? value)
			{
				((UpdateManager.GitHubAsset)obj).Name = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Name",
			JsonPropertyName = "name",
			AttributeProviderFactory = () => typeof(UpdateManager.GitHubAsset).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<string> propertyInfo2 = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UpdateManager.GitHubAsset),
			Converter = null,
			Getter = (object obj) => ((UpdateManager.GitHubAsset)obj).BrowserDownloadUrl,
			Setter = delegate(object obj, string? value)
			{
				((UpdateManager.GitHubAsset)obj).BrowserDownloadUrl = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "BrowserDownloadUrl",
			JsonPropertyName = "browser_download_url",
			AttributeProviderFactory = () => typeof(UpdateManager.GitHubAsset).GetProperty("BrowserDownloadUrl", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private void GitHubAssetSerializeHandler(Utf8JsonWriter writer, UpdateManager.GitHubAsset? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_name, value.Name);
		writer.WriteString(PropName_browser_download_url, value.BrowserDownloadUrl);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] GitHubAssetCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "name",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "browserDownloadUrl",
				ParameterType = typeof(string),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	private JsonTypeInfo<UpdateManager.GitHubAsset[]> Create_GitHubAssetArray(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<UpdateManager.GitHubAsset[]> jsonTypeInfo))
		{
			JsonCollectionInfoValues<UpdateManager.GitHubAsset[]> collectionInfo = new JsonCollectionInfoValues<UpdateManager.GitHubAsset[]>
			{
				ObjectCreator = null,
				SerializeHandler = GitHubAssetArraySerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateArrayInfo(options, collectionInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private void GitHubAssetArraySerializeHandler(Utf8JsonWriter writer, UpdateManager.GitHubAsset[]? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartArray();
		for (int i = 0; i < value.Length; i++)
		{
			GitHubAssetSerializeHandler(writer, value[i]);
		}
		writer.WriteEndArray();
	}

	private JsonTypeInfo<UpdateManager.GitHubRelease> Create_GitHubRelease(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<UpdateManager.GitHubRelease> jsonTypeInfo))
		{
			JsonObjectInfoValues<UpdateManager.GitHubRelease> objectInfo = new JsonObjectInfoValues<UpdateManager.GitHubRelease>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new UpdateManager.GitHubRelease((string)args[0], (UpdateManager.GitHubAsset[])args[1]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => GitHubReleasePropInit(options),
				ConstructorParameterMetadataInitializer = GitHubReleaseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(UpdateManager.GitHubRelease).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(string),
					typeof(UpdateManager.GitHubAsset[])
				}, null),
				SerializeHandler = GitHubReleaseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] GitHubReleasePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[2];
		JsonPropertyInfoValues<string> propertyInfo = new JsonPropertyInfoValues<string>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UpdateManager.GitHubRelease),
			Converter = null,
			Getter = (object obj) => ((UpdateManager.GitHubRelease)obj).TagName,
			Setter = delegate(object obj, string? value)
			{
				((UpdateManager.GitHubRelease)obj).TagName = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "TagName",
			JsonPropertyName = "tag_name",
			AttributeProviderFactory = () => typeof(UpdateManager.GitHubRelease).GetProperty("TagName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(string), Array.Empty<Type>(), null)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		array[0].IsGetNullable = false;
		array[0].IsSetNullable = false;
		JsonPropertyInfoValues<UpdateManager.GitHubAsset[]> propertyInfo2 = new JsonPropertyInfoValues<UpdateManager.GitHubAsset[]>
		{
			IsProperty = true,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(UpdateManager.GitHubRelease),
			Converter = null,
			Getter = (object obj) => ((UpdateManager.GitHubRelease)obj).Assets,
			Setter = delegate(object obj, UpdateManager.GitHubAsset[]? value)
			{
				((UpdateManager.GitHubRelease)obj).Assets = value;
			},
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "Assets",
			JsonPropertyName = "assets",
			AttributeProviderFactory = () => typeof(UpdateManager.GitHubRelease).GetProperty("Assets", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(UpdateManager.GitHubAsset[]), Array.Empty<Type>(), null)
		};
		array[1] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo2);
		array[1].IsGetNullable = false;
		array[1].IsSetNullable = false;
		return array;
	}

	private void GitHubReleaseSerializeHandler(Utf8JsonWriter writer, UpdateManager.GitHubRelease? value)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}
		writer.WriteStartObject();
		writer.WriteString(PropName_tag_name, value.TagName);
		writer.WritePropertyName(PropName_assets);
		GitHubAssetArraySerializeHandler(writer, value.Assets);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] GitHubReleaseCtorParamInit()
	{
		return new JsonParameterInfoValues[2]
		{
			new JsonParameterInfoValues
			{
				Name = "tagName",
				ParameterType = typeof(string),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			},
			new JsonParameterInfoValues
			{
				Name = "assets",
				ParameterType = typeof(UpdateManager.GitHubAsset[]),
				Position = 1,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
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

	public GitHubSerializationContext()
		: base(null)
	{
	}

	public GitHubSerializationContext(JsonSerializerOptions options)
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
		if (type == typeof(UpdateManager.GitHubAsset))
		{
			return Create_GitHubAsset(options);
		}
		if (type == typeof(UpdateManager.GitHubAsset[]))
		{
			return Create_GitHubAssetArray(options);
		}
		if (type == typeof(UpdateManager.GitHubRelease))
		{
			return Create_GitHubRelease(options);
		}
		if (type == typeof(string))
		{
			return Create_String(options);
		}
		return null;
	}
}
