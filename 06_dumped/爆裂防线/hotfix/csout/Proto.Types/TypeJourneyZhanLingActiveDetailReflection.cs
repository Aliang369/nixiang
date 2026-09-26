using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeJourneyZhanLingActiveDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyZhanLingActiveDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkpvdXJuZXlaaGFuTGluZ0FjdGl2ZURldGFpbC5wcm90bxIFdHlw" + "ZXMaFWVudW0uRUJvb2xTdGF0ZS5wcm90byJpChtKb3VybmV5WmhhbkxpbmdB" + "Y3RpdmVEZXRhaWwSDwoHR29vZHNJZBgBIAEoBRImCgZBY3RpdmUYAiABKA4y" + "Fi5FQm9vbFN0YXRlLkVCb29sU3RhdGUSEQoJSGlnaEF3YXJkGAMgASgFQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyZhanLingActiveDetail), (MessageParser)(object)JourneyZhanLingActiveDetail.Parser, new string[3] { "GoodsId", "Active", "HighAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
