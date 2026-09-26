using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopInfoReflection()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlZvdWNoZXJTaG9wSW5mby5wcm90bxIFdHlwZXMaHnR5cGUuVm91" + "Y2hlckNvdXBvbkRldGFpbC5wcm90bxohdHlwZS5Wb3VjaGVyUmVjZW50QnV5" + "RGV0YWlsLnByb3RvGh50eXBlLlZvdWNoZXJTaG9wQ2FydEluZm8ucHJvdG8a" + "IXR5cGUuVm91Y2hlclNob3BHb29kc0RldGFpbC5wcm90byKZAgoPVm91Y2hl" + "clNob3BJbmZvEi4KCkNvdXBvbkxpc3QYASADKAsyGi50eXBlcy5Wb3VjaGVy" + "Q291cG9uRGV0YWlsEjAKCVJlY2VudEJ1eRgCIAMoCzIdLnR5cGVzLlZvdWNo" + "ZXJSZWNlbnRCdXlEZXRhaWwSFAoMRGlzY291bnRMaXN0GAMgAygFEjAKCUxp" + "bWl0VGltZRgEIAMoCzIdLnR5cGVzLlZvdWNoZXJTaG9wR29vZHNEZXRhaWwS" + "LgoHSG90U2FsZRgFIAMoCzIdLnR5cGVzLlZvdWNoZXJTaG9wR29vZHNEZXRh" + "aWwSLAoIU2hvcENhcnQYBiABKAsyGi50eXBlcy5Wb3VjaGVyU2hvcENhcnRJ" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeVoucherCouponDetailReflection.Descriptor,
			TypeVoucherRecentBuyDetailReflection.Descriptor,
			TypeVoucherShopCartInfoReflection.Descriptor,
			TypeVoucherShopGoodsDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopInfo), (MessageParser)(object)VoucherShopInfo.Parser, new string[6] { "CouponList", "RecentBuy", "DiscountList", "LimitTime", "HotSale", "ShopCart" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
