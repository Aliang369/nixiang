using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ERoleState
{
	[OriginalName("Init")]
	Init,
	[OriginalName("Activate")]
	Activate,
	[OriginalName("Online")]
	Online
}
