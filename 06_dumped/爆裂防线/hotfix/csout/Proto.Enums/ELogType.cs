using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELogType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Legion")]
	Legion,
	[OriginalName("CrossGradeRace")]
	CrossGradeRace
}
