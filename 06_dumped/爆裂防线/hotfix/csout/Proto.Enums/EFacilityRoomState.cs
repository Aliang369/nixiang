using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EFacilityRoomState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Locked")]
	Locked,
	[OriginalName("Ready")]
	Ready,
	[OriginalName("Activated")]
	Activated,
	[OriginalName("Completed")]
	Completed
}
