using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EMoveType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Ground")]
	Ground,
	[OriginalName("Air")]
	Air
}
