using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELegionWarTaskType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("SignUp")]
	SignUp,
	[OriginalName("EnterMap")]
	EnterMap,
	[OriginalName("GotoBigPointCount")]
	GotoBigPointCount,
	[OriginalName("RestCount")]
	RestCount,
	[OriginalName("FightCount")]
	FightCount,
	[OriginalName("KillCount")]
	KillCount,
	[OriginalName("SearchAwardCount")]
	SearchAwardCount,
	[OriginalName("MoveCount")]
	MoveCount
}
