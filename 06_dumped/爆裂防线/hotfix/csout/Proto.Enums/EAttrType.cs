using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAttrType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Team")]
	Team,
	[OriginalName("Skill")]
	Skill,
	[OriginalName("Defense")]
	Defense,
	[OriginalName("Monster")]
	Monster
}
