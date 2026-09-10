using System;
using Newtonsoft.Json;

public class ProtectedIntJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ProtectedInt);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue((ProtectedInt)value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return new ProtectedInt(0);
		}
		return new ProtectedInt(Convert.ToInt32(reader.Value));
	}
}
