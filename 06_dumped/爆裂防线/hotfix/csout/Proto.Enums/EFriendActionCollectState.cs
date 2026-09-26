using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EFriendActionCollectState
{
	[OriginalName("NotExist")]
	NotExist,
	[OriginalName("Pending")]
	Pending,
	[OriginalName("Collected")]
	Collected,
	[OriginalName("Cannot")]
	Cannot
}
