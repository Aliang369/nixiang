using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDungeonEventType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("EasyFight")]
	EasyFight,
	[OriginalName("RiotFight")]
	RiotFight,
	[OriginalName("BossFight")]
	BossFight,
	[OriginalName("ModRand")]
	ModRand,
	[OriginalName("Popup")]
	Popup
}
