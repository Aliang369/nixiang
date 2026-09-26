using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDifficultyLevel
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Easy")]
	Easy,
	[OriginalName("Normal")]
	Normal,
	[OriginalName("Hard")]
	Hard
}
