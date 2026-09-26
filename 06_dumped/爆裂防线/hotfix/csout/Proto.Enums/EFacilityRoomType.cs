using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EFacilityRoomType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Trash")]
	Trash,
	[OriginalName("Chest")]
	Chest,
	[OriginalName("LockedChest")]
	LockedChest,
	[OriginalName("Monster")]
	Monster,
	[OriginalName("Shop")]
	Shop,
	[OriginalName("Rolling")]
	Rolling,
	[OriginalName("EntranceStair")]
	EntranceStair,
	[OriginalName("ExitStair")]
	ExitStair
}
