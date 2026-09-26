using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EBarType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Normal")]
	Normal,
	[OriginalName("Special")]
	Special,
	[OriginalName("Treasure")]
	Treasure,
	[OriginalName("TreasurePlus")]
	TreasurePlus,
	[OriginalName("Tactic")]
	Tactic
}
