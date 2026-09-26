using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDungeonMapCellFunType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Room")]
	Room,
	[OriginalName("Elevator")]
	Elevator,
	[OriginalName("Spawn")]
	Spawn
}
