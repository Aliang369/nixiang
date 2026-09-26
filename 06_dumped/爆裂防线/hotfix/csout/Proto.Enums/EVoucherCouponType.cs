using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EVoucherCouponType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Deduction")]
	Deduction,
	[OriginalName("Discount")]
	Discount
}
