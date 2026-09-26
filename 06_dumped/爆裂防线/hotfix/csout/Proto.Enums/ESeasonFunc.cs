using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESeasonFunc
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("LeaderElect")]
	LeaderElect = 100,
	[OriginalName("CityWar")]
	CityWar = 101,
	[OriginalName("DeclareHigh")]
	DeclareHigh = 102,
	[OriginalName("Airdrop")]
	Airdrop = 103,
	[OriginalName("SeasonShop")]
	SeasonShop = 104,
	[OriginalName("Tech")]
	Tech = 105
}
