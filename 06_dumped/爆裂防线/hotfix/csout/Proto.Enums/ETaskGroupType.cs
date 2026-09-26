using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ETaskGroupType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Parallel")]
	Parallel,
	[OriginalName("Serial")]
	Serial
}
