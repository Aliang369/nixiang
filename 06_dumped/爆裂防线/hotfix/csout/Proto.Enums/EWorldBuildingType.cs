using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EWorldBuildingType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Home")]
	Home,
	[OriginalName("Common")]
	Common,
	[OriginalName("Resource")]
	Resource,
	[OriginalName("Event")]
	Event
}
