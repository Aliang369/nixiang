using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELadderState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("LadderMatch")]
	LadderMatch,
	[OriginalName("Upgrading")]
	Upgrading,
	[OriginalName("UpgradSucess")]
	UpgradSucess,
	[OriginalName("UpgradFail")]
	UpgradFail
}
