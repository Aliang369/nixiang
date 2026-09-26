using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeTreasurePrivilegeInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTreasurePrivilegeInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlRyZWFzdXJlUHJpdmlsZWdlSW5mby5wcm90bxIFdHlwZXMaFWVu" + "dW0uRUJvb2xTdGF0ZS5wcm90byJvChVUcmVhc3VyZVByaXZpbGVnZUluZm8S" + "DwoHR29vZHNJZBgBIAEoBRImCgZBY3RpdmUYAiABKA4yFi5FQm9vbFN0YXRl" + "LkVCb29sU3RhdGUSDQoFUmVhY2gYAyABKAUSDgoGVGFyZ2V0GAQgASgFQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TreasurePrivilegeInfo), (MessageParser)(object)TreasurePrivilegeInfo.Parser, new string[4] { "GoodsId", "Active", "Reach", "Target" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
