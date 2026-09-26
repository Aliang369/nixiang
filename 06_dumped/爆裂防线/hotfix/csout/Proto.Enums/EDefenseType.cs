using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDefenseType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("WallDefense")]
	WallDefense,
	[OriginalName("CarDefense")]
	CarDefense
}
