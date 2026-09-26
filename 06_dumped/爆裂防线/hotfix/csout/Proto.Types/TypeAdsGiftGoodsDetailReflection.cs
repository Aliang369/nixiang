using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAdsGiftGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsGiftGoodsDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkFkc0dpZnRHb29kc0RldGFpbC5wcm90bxIFdHlwZXMaFWVudW0u" + "RUJvb2xTdGF0ZS5wcm90bxoXZW51bS5FQ29tbW9uU3RhdGUucHJvdG8aFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8inQEKEkFkc0dpZnRHb29kc0RldGFpbBIN" + "CgVJbmRleBgBIAEoBRImCgZOZWVkQWQYAiABKA4yFi5FQm9vbFN0YXRlLkVC" + "b29sU3RhdGUSKQoFU3RhdGUYAyABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1v" + "blN0YXRlEiUKCUF3YXJkTGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWls" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsGiftGoodsDetail), (MessageParser)(object)AdsGiftGoodsDetail.Parser, new string[4] { "Index", "NeedAd", "State", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
