using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EVipCardFuncType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("IdleWall")]
	IdleWall,
	[OriginalName("RefreshMod")]
	RefreshMod,
	[OriginalName("MainSpeed")]
	MainSpeed,
	[OriginalName("MainAward")]
	MainAward,
	[OriginalName("TiliBuy")]
	TiliBuy,
	[OriginalName("LegionBoss")]
	LegionBoss,
	[OriginalName("WorldBoss")]
	WorldBoss
}
