using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EChatActionType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Text")]
	Text,
	[OriginalName("LegionInvite")]
	LegionInvite,
	[OriginalName("BarShare")]
	BarShare,
	[OriginalName("HeroShare")]
	HeroShare,
	[OriginalName("EquipShare")]
	EquipShare,
	[OriginalName("BattleResult")]
	BattleResult,
	[OriginalName("WorldLair")]
	WorldLair
}
