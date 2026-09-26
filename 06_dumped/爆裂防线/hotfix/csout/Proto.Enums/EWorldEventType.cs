using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EWorldEventType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("AwardBox")]
	AwardBox,
	[OriginalName("Fight")]
	Fight,
	[OriginalName("Dungeon")]
	Dungeon,
	[OriginalName("WorldLair")]
	WorldLair
}
