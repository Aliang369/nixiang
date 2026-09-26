using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EFightType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Fight")]
	Fight,
	[OriginalName("BeFight")]
	BeFight
}
