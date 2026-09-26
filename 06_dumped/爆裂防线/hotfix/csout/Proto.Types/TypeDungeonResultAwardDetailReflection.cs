using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDungeonResultAwardDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonResultAwardDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkR1bmdlb25SZXN1bHRBd2FyZERldGFpbC5wcm90bxIFdHlwZXMa" + "FWVudW0uRUJvb2xTdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90" + "byJkChhEdW5nZW9uUmVzdWx0QXdhcmREZXRhaWwSJQoFU3RhdGUYASABKA4y" + "Fi5FQm9vbFN0YXRlLkVCb29sU3RhdGUSIQoFQXdhcmQYAiABKAsyEi50eXBl" + "cy5Bd2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonResultAwardDetail), (MessageParser)(object)DungeonResultAwardDetail.Parser, new string[2] { "State", "Award" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
