using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EWorldReconState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Idle")]
	Idle,
	[OriginalName("Explore")]
	Explore,
	[OriginalName("Explored")]
	Explored
}
