using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Geometry;
using Newtonsoft.Json.UnityConverters.Math;

public class SerializationExecutor
{
	public class SaveLoadConverter : JsonConverter
	{
		private AssetCatalog catalog;

		public SaveLoadConverter(AssetCatalog catalog)
		{
			this.catalog = catalog;
		}

		public override bool CanConvert(Type objectType)
		{
			if (typeof(ICTAsset).IsAssignableFrom(objectType))
			{
				return true;
			}
			return false;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			object result = null;
			if (typeof(ICTAsset).IsAssignableFrom(objectType))
			{
				string text = reader.Value?.ToString();
				if (text.IsNullOrEmpty())
				{
					return result;
				}
				ICTAsset iCTAsset = catalog.FullIdToCTAsset<ICTAsset>(text);
				if (iCTAsset != null)
				{
					result = iCTAsset;
				}
				else
				{
					TraceLog.Warn($"FullID-{text}-不存在-{objectType}-{reader.Path}");
				}
			}
			return result;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value != null)
			{
				if (value is ICTAsset iCTAsset)
				{
					writer.WriteValue(iCTAsset.GetFullID());
				}
				else
				{
					serializer.Serialize(writer, value);
				}
			}
		}
	}

	public class SaveLoadResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
			if (jsonProperty.AttributeProvider.GetAttributes(inherit: true).Any((Attribute t) => t is SaveField))
			{
				jsonProperty.Readable = true;
				jsonProperty.Writable = true;
			}
			return jsonProperty;
		}

		protected override List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			FieldInfo[] fields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.GetCustomAttribute(typeof(SaveField)) != null)
				{
					list.Add(fieldInfo);
				}
			}
			return list;
		}

		public override JsonContract ResolveContract(Type type)
		{
			return base.ResolveContract(type);
		}
	}

	public class CustomTraceWrite : ITraceWriter
	{
		public TraceLevel LevelFilter => TraceLevel.Error;

		public void Trace(TraceLevel level, string message, Exception ex)
		{
			if (level == TraceLevel.Error)
			{
				TraceLog.Exception(ex, "序列化错误:" + message, ThinkingAnalyticsExtends.ExceptionType.Other);
			}
		}
	}

	private static readonly ConditionalWeakTable<AssetCatalog, JsonSerializerSettings> SettingsByCatalog = new ConditionalWeakTable<AssetCatalog, JsonSerializerSettings>();

	private static readonly JsonSerializerSettings SettingsWithoutCatalog = GenerateSerilizeSetting(null);

	public AssetCatalog assetCatalog;

	private readonly JsonSerializerSettings settings;

	public SerializationExecutor(AssetCatalog assetCatalog)
	{
		this.assetCatalog = assetCatalog;
		settings = ((assetCatalog == null) ? SettingsWithoutCatalog : SettingsByCatalog.GetValue(assetCatalog, GenerateSerilizeSetting));
	}

	private static JsonSerializerSettings GenerateSerilizeSetting(AssetCatalog assetCatalog)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		return new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			PreserveReferencesHandling = PreserveReferencesHandling.All,
			ContractResolver = new SaveLoadResolver(),
			TraceWriter = new CustomTraceWrite(),
			ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
			TypeNameHandling = TypeNameHandling.Auto,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			MaxDepth = 256,
			Converters = 
			{
				(JsonConverter)new SaveLoadConverter(assetCatalog),
				(JsonConverter)new ProtectedIntJsonConverter(),
				(JsonConverter)new Vector2Converter(),
				(JsonConverter)new Vector2IntConverter(),
				(JsonConverter)new Vector3Converter(),
				(JsonConverter)new Vector3IntConverter(),
				(JsonConverter)new ColorConverter(),
				(JsonConverter)new Color32Converter(),
				(JsonConverter)new RectConverter(),
				(JsonConverter)new RectIntConverter(),
				(JsonConverter)new BoundsConverter()
			}
		};
	}

	public string Serialize(object obj)
	{
		HashSet<object> hashSet = new HashSet<object>();
		UnityHelper.GetFieldWithAttributeNested(obj, typeof(SaveField), hashSet);
		foreach (object item in hashSet)
		{
			if (item is ISaveCallback saveCallback)
			{
				saveCallback.BeforeSave();
			}
		}
		return JsonConvert.SerializeObject(obj, settings);
	}

	public T Deserialize<T>(string text) where T : class
	{
		T val = JsonConvert.DeserializeObject(text, typeof(T), settings) as T;
		HashSet<object> hashSet = new HashSet<object>();
		UnityHelper.GetFieldWithAttributeNested(val, typeof(SaveField), hashSet);
		foreach (object item in hashSet)
		{
			if (item is ISaveCallback)
			{
				(item as ISaveCallback).AfterLoad();
			}
		}
		return val;
	}
}
