using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EVoucherGoodsType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Common")]
	Common,
	[OriginalName("Limit")]
	Limit
}
