using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESeasonBattleType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("CityPvP")]
	CityPvP,
	[OriginalName("CityPvE")]
	CityPvE,
	[OriginalName("Airdrop")]
	Airdrop,
	[OriginalName("Impeach")]
	Impeach
}
