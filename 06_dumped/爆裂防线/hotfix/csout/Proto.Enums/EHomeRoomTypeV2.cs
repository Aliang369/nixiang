using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EHomeRoomTypeV2
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Utility")]
	Utility,
	[OriginalName("WarRoom")]
	WarRoom,
	[OriginalName("Effect")]
	Effect
}
