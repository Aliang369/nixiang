using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELegionOfficial
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Member")]
	Member,
	[OriginalName("Adviser")]
	Adviser,
	[OriginalName("Leader")]
	Leader
}
