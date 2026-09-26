using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EBattleResultScore
{
	[OriginalName("Fail")]
	Fail,
	[OriginalName("Win1")]
	Win1,
	[OriginalName("Win2")]
	Win2,
	[OriginalName("Win3")]
	Win3
}
