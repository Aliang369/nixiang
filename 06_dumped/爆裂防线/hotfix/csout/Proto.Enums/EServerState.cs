using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EServerState
{
	[OriginalName("Close")]
	Close = 0,
	[OriginalName("Normal")]
	Normal = 1,
	[OriginalName("Hot")]
	Hot = 2,
	[OriginalName("Recommend")]
	Recommend = 3,
	[OriginalName("Limit")]
	Limit = -1
}
