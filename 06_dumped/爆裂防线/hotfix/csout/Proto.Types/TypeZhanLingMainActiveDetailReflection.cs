using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeZhanLingMainActiveDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeZhanLingMainActiveDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLlpoYW5MaW5nTWFpbkFjdGl2ZURldGFpbC5wcm90bxIFdHlwZXMa" + "FWVudW0uRUJvb2xTdGF0ZS5wcm90byJmChhaaGFuTGluZ01haW5BY3RpdmVE" + "ZXRhaWwSDwoHR29vZHNJZBgBIAEoBRImCgZBY3RpdmUYAiABKA4yFi5FQm9v" + "bFN0YXRlLkVCb29sU3RhdGUSEQoJSGlnaEF3YXJkGAMgASgFQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ZhanLingMainActiveDetail), (MessageParser)(object)ZhanLingMainActiveDetail.Parser, new string[3] { "GoodsId", "Active", "HighAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
