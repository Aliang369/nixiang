using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeZhanLingMainInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeZhanLingMainInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlpoYW5MaW5nTWFpbkluZm8ucHJvdG8SBXR5cGVzGiN0eXBlLlpo" + "YW5MaW5nTWFpbkFjdGl2ZURldGFpbC5wcm90byJoChBaaGFuTGluZ01haW5J" + "bmZvEg0KBVJlYWNoGAEgASgFEhAKCExvd0F3YXJkGAIgASgFEjMKCkFjdGl2" + "ZUxpc3QYAyADKAsyHy50eXBlcy5aaGFuTGluZ01haW5BY3RpdmVEZXRhaWxC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeZhanLingMainActiveDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ZhanLingMainInfo), (MessageParser)(object)ZhanLingMainInfo.Parser, new string[3] { "Reach", "LowAward", "ActiveList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
