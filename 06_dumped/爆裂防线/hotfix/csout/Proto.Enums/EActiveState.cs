using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EActiveState
{
	[OriginalName("Pending")]
	Pending,
	[OriginalName("Active")]
	Active,
	[OriginalName("Done")]
	Done
}
