using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ERankType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("WorldBoss")]
	WorldBoss,
	[OriginalName("MainQuest")]
	MainQuest,
	[OriginalName("EndlessHorde")]
	EndlessHorde,
	[OriginalName("Arena")]
	Arena,
	[OriginalName("PeakArena")]
	PeakArena
}
