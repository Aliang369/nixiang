using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroAwakenInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenInfoReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkhlcm9Bd2FrZW5JbmZvLnByb3RvEgV0eXBlcxobdHlwZS5IZXJv" + "QXdha2VuRGV0YWlsLnByb3RvIj0KDkhlcm9Bd2FrZW5JbmZvEisKCkF3YWtl" + "bkxpc3QYASADKAsyFy50eXBlcy5IZXJvQXdha2VuRGV0YWlsQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeHeroAwakenDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenInfo), (MessageParser)(object)HeroAwakenInfo.Parser, new string[1] { "AwakenList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
