using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitTimeShopItemDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeShopItemDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxpbWl0VGltZVNob3BJdGVtRGV0YWlsLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90byKXAQoXTGltaXRUaW1lU2hvcEl0ZW1E" + "ZXRhaWwSDQoFSW5kZXgYASABKAUSEAoIRGlzY291bnQYAiABKAUSEQoJU2Fs" + "ZVByaWNlGAMgASgFEhAKCExpbWl0TnVtGAQgASgFEg0KBUNvdW50GAUgASgF" + "EicKC0F3YXJkRGV0YWlsGAYgASgLMhIudHlwZXMuQXdhcmREZXRhaWxCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeShopItemDetail), (MessageParser)(object)LimitTimeShopItemDetail.Parser, new string[6] { "Index", "Discount", "SalePrice", "LimitNum", "Count", "AwardDetail" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
