using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Geometry;
using Newtonsoft.Json.UnityConverters.Math;
using UnityEngine;

public static class TraceLog
{
	public class TracelogConverter : JsonConverter
	{
		private AssetCatalog catalog => Bootstrap.instance.assetCatalog;

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
			return null;
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

	private static JsonSerializerSettings _settings;

	public static bool EnabledLog => CommonConfig.Instance.enableDebugLog;

	public static bool EnabledWarning => CommonConfig.Instance.enableWarningLog;

	public static bool EnabledError => CommonConfig.Instance.enableErrorLog;

	private static JsonSerializerSettings settings
	{
		get
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Expected O, but got Unknown
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Expected O, but got Unknown
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Expected O, but got Unknown
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			if (_settings == null)
			{
				_settings = new JsonSerializerSettings
				{
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					PreserveReferencesHandling = PreserveReferencesHandling.All,
					TraceWriter = null,
					ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
					MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
					TypeNameHandling = TypeNameHandling.Auto,
					MissingMemberHandling = MissingMemberHandling.Ignore,
					MaxDepth = 10,
					ContractResolver = new DefaultContractResolver()
				};
				_settings.Converters.Add((JsonConverter)new Vector2Converter());
				_settings.Converters.Add((JsonConverter)new Vector2IntConverter());
				_settings.Converters.Add((JsonConverter)new Vector3Converter());
				_settings.Converters.Add((JsonConverter)new Vector3IntConverter());
				_settings.Converters.Add((JsonConverter)new ColorConverter());
				_settings.Converters.Add((JsonConverter)new Color32Converter());
				_settings.Converters.Add((JsonConverter)new RectConverter());
				_settings.Converters.Add((JsonConverter)new RectIntConverter());
				_settings.Converters.Add((JsonConverter)new BoundsConverter());
				_settings.Converters.Add(new TracelogConverter());
			}
			return _settings;
		}
	}

	public static void Initialize()
	{
	}

	public static void Debug(string message)
	{
		if (EnabledLog)
		{
			UnityEngine.Debug.Log(message);
		}
	}

	public static void TryDebug(Func<string> sf)
	{
		if (!EnabledLog)
		{
			return;
		}
		try
		{
			Debug(sf?.Invoke());
		}
		catch (Exception e)
		{
			Exception(e);
		}
	}

	public static void Warn(string message)
	{
		if (EnabledWarning)
		{
			UnityEngine.Debug.LogWarning(message);
		}
	}

	public static void Error(string message)
	{
		if (EnabledError)
		{
			UnityEngine.Debug.LogError(message);
		}
	}

	public static void Exception(Exception e, string appendMessage = null)
	{
		Exception(e, appendMessage, ThinkingAnalyticsExtends.ExceptionType.Other);
	}

	public static void Exception(Exception e, string appendMessage, ThinkingAnalyticsExtends.ExceptionType type)
	{
		ThinkingAnalyticsExtends.TraceException(appendMessage + e.Message, e.StackTrace, type);
		if (EnabledError)
		{
			UnityEngine.Debug.LogException(e);
		}
	}

	public static string ToJsonString(object obj)
	{
		try
		{
			return JsonConvert.SerializeObject(obj, settings);
		}
		catch
		{
			return "Error";
		}
	}
}
