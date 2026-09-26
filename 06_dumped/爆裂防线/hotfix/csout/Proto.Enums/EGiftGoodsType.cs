using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EGiftGoodsType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Growth")]
	Growth,
	[OriginalName("Pathway")]
	Pathway,
	[OriginalName("VipLevel")]
	VipLevel
}
