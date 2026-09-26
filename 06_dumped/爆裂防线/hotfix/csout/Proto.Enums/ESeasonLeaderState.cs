using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESeasonLeaderState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("None")]
	None,
	[OriginalName("Incumbent")]
	Incumbent,
	[OriginalName("Impeaching")]
	Impeaching
}
