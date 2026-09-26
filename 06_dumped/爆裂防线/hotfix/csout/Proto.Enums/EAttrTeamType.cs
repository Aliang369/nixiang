using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAttrTeamType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("CritRate")]
	CritRate = 1,
	[OriginalName("CritDamage")]
	CritDamage = 2,
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
	[OriginalName("Attack")]
	Attack = 21,
	[OriginalName("ElecAttack")]
	ElecAttack = 22,
	[OriginalName("FireAttack")]
	FireAttack = 23,
	[OriginalName("IceAttack")]
	IceAttack = 24,
	[OriginalName("PhyAttack")]
	PhyAttack = 25,
	[OriginalName("EnergyAttack")]
	EnergyAttack = 26,
	[OriginalName("WindAttack")]
	WindAttack = 27,
	[OriginalName("DamagePct")]
	DamagePct = 31,
	[OriginalName("ElecDamagePct")]
	ElecDamagePct = 32,
	[OriginalName("FireDamagePct")]
	FireDamagePct = 33,
	[OriginalName("IceDamagePct")]
	IceDamagePct = 34,
	[OriginalName("PhyDamagePct")]
	PhyDamagePct = 35,
	[OriginalName("EnergyDamagePct")]
	EnergyDamagePct = 36,
	[OriginalName("WindDamagePct")]
	WindDamagePct = 37
}
