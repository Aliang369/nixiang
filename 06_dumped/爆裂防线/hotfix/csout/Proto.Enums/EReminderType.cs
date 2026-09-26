using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EReminderType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("ArenaRecord")]
	ArenaRecord,
	[OriginalName("HelpAward")]
	HelpAward,
	[OriginalName("LadderRecord")]
	LadderRecord,
	[OriginalName("PeakArenaRecord")]
	PeakArenaRecord,
	[OriginalName("MineRecord")]
	MineRecord,
	[OriginalName("CrossMineRecord")]
	CrossMineRecord,
	[OriginalName("TruckRecord")]
	TruckRecord,
	[OriginalName("LegionWar")]
	LegionWar
}
