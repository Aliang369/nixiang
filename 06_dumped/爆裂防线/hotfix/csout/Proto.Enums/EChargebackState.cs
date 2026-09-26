using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EChargebackState
{
	[OriginalName("None")]
	None,
	[OriginalName("Warning")]
	Warning,
	[OriginalName("Danger")]
	Danger
}
