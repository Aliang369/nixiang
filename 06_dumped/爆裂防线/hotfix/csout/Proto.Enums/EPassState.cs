using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EPassState
{
	[OriginalName("NotStarted")]
	NotStarted,
	[OriginalName("InProgress")]
	InProgress,
	[OriginalName("Failed")]
	Failed,
	[OriginalName("Passed")]
	Passed,
	[OriginalName("Settled")]
	Settled
}
