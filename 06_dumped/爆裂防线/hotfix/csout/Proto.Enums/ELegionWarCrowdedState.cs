using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELegionWarCrowdedState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Idle")]
	Idle,
	[OriginalName("Normal")]
	Normal,
	[OriginalName("Crowded")]
	Crowded
}
