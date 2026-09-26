using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDungeonType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("HomeFloor")]
	HomeFloor,
	[OriginalName("World")]
	World
}
