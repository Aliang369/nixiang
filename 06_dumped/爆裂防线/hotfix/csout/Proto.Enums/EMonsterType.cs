using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EMonsterType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Normal")]
	Normal,
	[OriginalName("Elite")]
	Elite,
	[OriginalName("Boss")]
	Boss
}
