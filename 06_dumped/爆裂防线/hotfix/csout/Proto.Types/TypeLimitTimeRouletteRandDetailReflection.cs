using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLimitTimeRouletteRandDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeRouletteRandDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkxpbWl0VGltZVJvdWxldHRlUmFuZERldGFpbC5wcm90bxIFdHlw" + "ZXMaFWVudW0uRUJvb2xTdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5w" + "cm90byJ7ChtMaW1pdFRpbWVSb3VsZXR0ZVJhbmREZXRhaWwSDQoFSW5kZXgY" + "ASABKAUSJAoEUmFyZRgCIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0ZRIn" + "CgtBd2FyZERldGFpbBgDIAEoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeRouletteRandDetail), (MessageParser)(object)LimitTimeRouletteRandDetail.Parser, new string[3] { "Index", "Rare", "AwardDetail" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
