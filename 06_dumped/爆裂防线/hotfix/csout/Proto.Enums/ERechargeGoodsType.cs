using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ERechargeGoodsType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Diamond")]
	Diamond = 1,
	[OriginalName("VipCard")]
	VipCard = 2,
	[OriginalName("Voucher")]
	Voucher = 3,
	[OriginalName("WebMall")]
	WebMall = 4,
	[OriginalName("Approval")]
	Approval = 6,
	[OriginalName("Subscribe")]
	Subscribe = 7
}
