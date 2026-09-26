using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeVoucherShopGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopGoodsDetailReflection()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlZvdWNoZXJTaG9wR29vZHNEZXRhaWwucHJvdG8SBXR5cGVzGhNl" + "bnVtLkVRdWFsaXR5LnByb3RvGhdlbnVtLkVSZWZyZXNoVHlwZS5wcm90bxoc" + "ZW51bS5FVm91Y2hlckdvb2RzVHlwZS5wcm90bxoWdHlwZS5Bd2FyZERldGFp" + "bC5wcm90byLoAgoWVm91Y2hlclNob3BHb29kc0RldGFpbBIPCgdHb29kc0lk" + "GAEgASgFEjcKCUdvb2RzVHlwZRgCIAEoDjIkLkVWb3VjaGVyR29vZHNUeXBl" + "LkVWb3VjaGVyR29vZHNUeXBlEigKDEdvb2RzUXVhbGl0eRgDIAEoDjISLkVR" + "dWFsaXR5LkVRdWFsaXR5EhEKCVNvcnRJbmRleBgEIAEoBRIvCgtSZWZyZXNo" + "VHlwZRgFIAEoDjIaLkVSZWZyZXNoVHlwZS5FUmVmcmVzaFR5cGUSEAoIRXhw" + "aXJlVG0YBiABKAMSEAoITGltaXROdW0YByABKAUSEwoLQm91Z2h0Q291bnQY" + "CCABKAUSEQoJU2hvd1ByaWNlGAkgASgFEhEKCVNhbGVQcmljZRgKIAEoBRIQ" + "CghEaXNjb3VudBgLIAEoBRIlCglBd2FyZExpc3QYDCADKAsyEi50eXBlcy5B" + "d2FyZERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEQualityReflection.Descriptor,
			EnumERefreshTypeReflection.Descriptor,
			EnumEVoucherGoodsTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopGoodsDetail), (MessageParser)(object)VoucherShopGoodsDetail.Parser, new string[12]
			{
				"GoodsId", "GoodsType", "GoodsQuality", "SortIndex", "RefreshType", "ExpireTm", "LimitNum", "BoughtCount", "ShowPrice", "SalePrice",
				"Discount", "AwardList"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
