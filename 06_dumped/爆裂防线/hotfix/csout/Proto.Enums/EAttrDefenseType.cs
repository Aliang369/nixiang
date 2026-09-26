using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAttrDefenseType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("DefenseHp")]
	DefenseHp = 11,
	[OriginalName("ShieldHp")]
	ShieldHp = 12,
	[OriginalName("DamageDecr")]
	DamageDecr = 13,
	[OriginalName("DamageImmunity")]
	DamageImmunity = 14,
	[OriginalName("EnergyRegen")]
	EnergyRegen = 15,
	[OriginalName("FailureRate")]
	FailureRate = 21,
	[OriginalName("MaxMileage")]
	MaxMileage = 22,
	[OriginalName("VisionRange")]
	VisionRange = 23
}
