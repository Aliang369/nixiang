using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAttrMonsterType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Hp")]
	Hp,
	[OriginalName("Damage")]
	Damage,
	[OriginalName("AttackSpeed")]
	AttackSpeed,
	[OriginalName("AttackRange")]
	AttackRange,
	[OriginalName("MoveSpeed")]
	MoveSpeed,
	[OriginalName("MoveType")]
	MoveType,
	[OriginalName("BodyScale")]
	BodyScale,
	[OriginalName("Mass")]
	Mass,
	[OriginalName("DamageTaken")]
	DamageTaken
}
