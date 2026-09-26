using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EGoodsType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Refer")]
	Refer,
	[OriginalName("Recharge")]
	Recharge,
	[OriginalName("Func")]
	Func,
	[OriginalName("Operate")]
	Operate,
	[OriginalName("Gift")]
	Gift,
	[OriginalName("NonOperating")]
	NonOperating
}
