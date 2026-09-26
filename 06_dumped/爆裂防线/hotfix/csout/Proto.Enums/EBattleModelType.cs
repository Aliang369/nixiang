using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EBattleModelType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("KillAll")]
	KillAll,
	[OriginalName("HoldOn")]
	HoldOn,
	[OriginalName("NoVictory")]
	NoVictory,
	[OriginalName("Single")]
	Single,
	[OriginalName("KillBoss")]
	KillBoss,
	[OriginalName("PVP")]
	Pvp
}
