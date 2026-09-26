using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELegionFuncType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Dinner")]
	Dinner,
	[OriginalName("Reward")]
	Reward,
	[OriginalName("Boss")]
	Boss
}
