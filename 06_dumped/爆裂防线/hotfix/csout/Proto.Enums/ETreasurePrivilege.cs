using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ETreasurePrivilege
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("BarTreasure")]
	BarTreasure,
	[OriginalName("MineWarCollect")]
	MineWarCollect,
	[OriginalName("MineWarCombatBuy")]
	MineWarCombatBuy,
	[OriginalName("RechargeAward")]
	RechargeAward
}
