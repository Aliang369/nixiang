using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ERebateState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("NoRebate")]
	NoRebate,
	[OriginalName("WaitRebate")]
	WaitRebate,
	[OriginalName("SuccessRebate")]
	SuccessRebate,
	[OriginalName("OtherRebate")]
	OtherRebate
}
