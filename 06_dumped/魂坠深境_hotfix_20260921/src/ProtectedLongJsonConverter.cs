using System;
using Newtonsoft.Json;

public class ProtectedLongJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ProtectedLong);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue((ProtectedLong)value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return new ProtectedLong(0L);
		}
		return new ProtectedLong(Convert.ToInt64(reader.Value));
	}
}
