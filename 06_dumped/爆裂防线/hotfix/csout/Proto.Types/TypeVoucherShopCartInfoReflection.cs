using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartInfoReflection()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlZvdWNoZXJTaG9wQ2FydEluZm8ucHJvdG8SBXR5cGVzGiZ0eXBl" + "LlZvdWNoZXJTaG9wQ2FydENvdXBvbkRldGFpbC5wcm90bxoldHlwZS5Wb3Vj" + "aGVyU2hvcENhcnRHb29kc0RldGFpbC5wcm90bxogdHlwZS5Wb3VjaGVyU2hv" + "cENhcnRSZXN1bHQucHJvdG8i+QEKE1ZvdWNoZXJTaG9wQ2FydEluZm8SNAoJ" + "R29vZHNMaXN0GAEgAygLMiEudHlwZXMuVm91Y2hlclNob3BDYXJ0R29vZHNE" + "ZXRhaWwSPwoTQXZhaWxhYmxlQ291cG9uTGlzdBgCIAMoCzIiLnR5cGVzLlZv" + "dWNoZXJTaG9wQ2FydENvdXBvbkRldGFpbBI1Cg9SZWNvbW1lbmRSZXN1bHQY" + "AyABKAsyHC50eXBlcy5Wb3VjaGVyU2hvcENhcnRSZXN1bHQSNAoOU2VsZWN0" + "ZWRSZXN1bHQYBCABKAsyHC50eXBlcy5Wb3VjaGVyU2hvcENhcnRSZXN1bHRC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeVoucherShopCartCouponDetailReflection.Descriptor,
			TypeVoucherShopCartGoodsDetailReflection.Descriptor,
			TypeVoucherShopCartResultReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartInfo), (MessageParser)(object)VoucherShopCartInfo.Parser, new string[4] { "GoodsList", "AvailableCouponList", "RecommendResult", "SelectedResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
