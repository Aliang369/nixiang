using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDungeonEventTrigger
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Force")]
	Force,
	[OriginalName("Active")]
	Active,
	[OriginalName("Passive")]
	Passive
}
