using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EVipCardCrossLadder
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("WinScore")]
	WinScore,
	[OriginalName("FailScore")]
	FailScore,
	[OriginalName("FailVictory3")]
	FailVictory3,
	[OriginalName("CombatBuy")]
	CombatBuy,
	[OriginalName("PeakFightRange")]
	PeakFightRange
}
