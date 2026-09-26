using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EHomelandWorkState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Idle")]
	Idle,
	[OriginalName("Pause")]
	Pause,
	[OriginalName("Shutdown")]
	Shutdown,
	[OriginalName("Working")]
	Working
}
