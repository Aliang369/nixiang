using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeVoucherCouponDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherCouponDetailReflection()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlZvdWNoZXJDb3Vwb25EZXRhaWwucHJvdG8SBXR5cGVzGh1lbnVt" + "LkVWb3VjaGVyQ291cG9uVHlwZS5wcm90byLdAQoTVm91Y2hlckNvdXBvbkRl" + "dGFpbBIMCgRVVUlEGAEgASgJEhAKCENvdXBvbklkGAIgASgFEjoKCkNvdXBv" + "blR5cGUYAyABKA4yJi5FVm91Y2hlckNvdXBvblR5cGUuRVZvdWNoZXJDb3Vw" + "b25UeXBlEhQKDERlZHVjdGlvbk1pbhgEIAEoBRIWCg5EZWR1Y3Rpb25WYWx1" + "ZRgFIAEoBRITCgtEaXNjb3VudE1heBgGIAEoBRIVCg1EaXNjb3VudFZhbHVl" + "GAcgASgFEhAKCEV4cGlyZVRtGAggASgDQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEVoucherCouponTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherCouponDetail), (MessageParser)(object)VoucherCouponDetail.Parser, new string[8] { "UUID", "CouponId", "CouponType", "DeductionMin", "DeductionValue", "DiscountMax", "DiscountValue", "ExpireTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
