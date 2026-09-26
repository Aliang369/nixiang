using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeInfiniteGiftGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeInfiniteGiftGoodsDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkluZmluaXRlR2lmdEdvb2RzRGV0YWlsLnByb3RvEgV0eXBlcxoX" + "ZW51bS5FQ29tbW9uU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwucHJv" + "dG8imgEKF0luZmluaXRlR2lmdEdvb2RzRGV0YWlsEg0KBUluZGV4GAEgASgF" + "Eg8KB0dvb2RzSWQYAiABKAUSDQoFU2NvcmUYAyABKAUSKQoFU3RhdGUYBCAB" + "KA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1vblN0YXRlEiUKCUF3YXJkTGlzdBgF" + "IAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(InfiniteGiftGoodsDetail), (MessageParser)(object)InfiniteGiftGoodsDetail.Parser, new string[5] { "Index", "GoodsId", "Score", "State", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
