using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EUnlockType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("RoleLevel")]
	RoleLevel,
	[OriginalName("Peerage")]
	Peerage
}
