using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeMoneyExchangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMoneyExchangeDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLk1vbmV5RXhjaGFuZ2VEZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVt" + "LkVCb29sU3RhdGUucHJvdG8aFWVudW0uRU1vbmV5VHlwZS5wcm90byJ1ChNN" + "b25leUV4Y2hhbmdlRGV0YWlsEikKCU1vbmV5VHlwZRgBIAEoDjIWLkVNb25l" + "eVR5cGUuRU1vbmV5VHlwZRINCgVDb3VudBgCIAEoBRIkCgRIYWxmGAMgASgO" + "MhYuRUJvb2xTdGF0ZS5FQm9vbFN0YXRlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEMoneyTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MoneyExchangeDetail), (MessageParser)(object)MoneyExchangeDetail.Parser, new string[3] { "MoneyType", "Count", "Half" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
