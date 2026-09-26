using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EItemType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("RoleNameCard")]
	RoleNameCard,
	[OriginalName("EquipExp")]
	EquipExp,
	[OriginalName("SkillExp")]
	SkillExp,
	[OriginalName("Material")]
	Material,
	[OriginalName("DefenseExp")]
	DefenseExp,
	[OriginalName("MonsterItem")]
	MonsterItem,
	[OriginalName("GemItem")]
	GemItem,
	[OriginalName("HeroAwakenUpgrade")]
	HeroAwakenUpgrade,
	[OriginalName("HeroAwakenAdvance")]
	HeroAwakenAdvance,
	[OriginalName("BoxKey")]
	BoxKey,
	[OriginalName("MechaSkill")]
	MechaSkill,
	[OriginalName("MechaLevel")]
	MechaLevel,
	[OriginalName("HeroFollow")]
	HeroFollow,
	[OriginalName("FacilityCreate")]
	FacilityCreate
}
