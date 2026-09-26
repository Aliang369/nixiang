using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EOperatePlanType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("NewServer")]
	NewServer,
	[OriginalName("OldServer")]
	OldServer,
	[OriginalName("Holiday")]
	Holiday
}
