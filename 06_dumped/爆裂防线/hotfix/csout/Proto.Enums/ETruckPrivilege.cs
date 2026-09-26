using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ETruckPrivilege
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("UnlockSlot")]
	UnlockSlot,
	[OriginalName("TransportRefresh")]
	TransportRefresh,
	[OriginalName("TransportCount")]
	TransportCount
}
