using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePayVerifyOrderReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePayVerifyOrderReqReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlBheVZlcmlmeU9yZGVyUmVxLnByb3RvEgV0eXBlcxoXZW51bS5F" + "Q2hhbm5lbFR5cGUucHJvdG8ikwEKEVBheVZlcmlmeU9yZGVyUmVxEi4KCkFw" + "cENoYW5uZWwYASABKA4yGi5FQ2hhbm5lbFR5cGUuRUNoYW5uZWxUeXBlEg8K" + "B09yZGVySWQYAiABKAkSDwoHR29vZHNJZBgDIAEoBRINCgVQcmljZRgEIAEo" + "AhIPCgdPcmRlclRtGAUgASgDEgwKBERhdGEYBiABKAlCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChannelTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PayVerifyOrderReq), (MessageParser)(object)PayVerifyOrderReq.Parser, new string[6] { "AppChannel", "OrderId", "GoodsId", "Price", "OrderTm", "Data" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
