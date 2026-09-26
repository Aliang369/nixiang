using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EHomeRoomCategory
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("ControlCenter")]
	ControlCenter,
	[OriginalName("Utility")]
	Utility,
	[OriginalName("Defense")]
	Defense,
	[OriginalName("WarRoom")]
	WarRoom,
	[OriginalName("Survival")]
	Survival,
	[OriginalName("Crafting")]
	Crafting
}
