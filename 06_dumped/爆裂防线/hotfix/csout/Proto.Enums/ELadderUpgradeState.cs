using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELadderUpgradeState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Win")]
	Win,
	[OriginalName("Fail")]
	Fail
}
