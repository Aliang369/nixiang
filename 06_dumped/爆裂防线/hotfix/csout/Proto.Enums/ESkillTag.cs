using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESkillTag
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Ballistic")]
	Ballistic,
	[OriginalName("Laser")]
	Laser,
	[OriginalName("Ground")]
	Ground,
	[OriginalName("Explode")]
	Explode
}
