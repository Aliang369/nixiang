using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESeasonType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Newbie")]
	Newbie,
	[OriginalName("PK")]
	Pk
}
