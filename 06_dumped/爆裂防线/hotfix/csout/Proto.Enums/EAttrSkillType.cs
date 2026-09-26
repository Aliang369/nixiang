using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAttrSkillType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Damage")]
	Damage = 1,
	[OriginalName("Penetration")]
	Penetration = 2,
	[OriginalName("AlertRange")]
	AlertRange = 3,
	[OriginalName("DamageRange")]
	DamageRange = 4,
	[OriginalName("Duration")]
	Duration = 5,
	[OriginalName("DamageInterval")]
	DamageInterval = 6,
	[OriginalName("PeripheralDamage")]
	PeripheralDamage = 7,
	[OriginalName("CentralDamage")]
	CentralDamage = 8,
	[OriginalName("ExplosionDamage")]
	ExplosionDamage = 9,
	[OriginalName("SplashDamage")]
	SplashDamage = 10,
	[OriginalName("ExplosionRange")]
	ExplosionRange = 11,
	[OriginalName("RicochetCount")]
	RicochetCount = 12,
	[OriginalName("Magazine")]
	Magazine = 13,
	[OriginalName("ReloadTime")]
	ReloadTime = 14,
	[OriginalName("BulletSpeed")]
	BulletSpeed = 15,
	[OriginalName("MoveSpeed")]
	MoveSpeed = 16,
	[OriginalName("Repeater")]
	Repeater = 17,
	[OriginalName("Extra")]
	Extra = 18,
	[OriginalName("Volley")]
	Volley = 19,
	[OriginalName("FireRate")]
	FireRate = 20,
	[OriginalName("Refraction")]
	Refraction = 21,
	[OriginalName("Cd")]
	Cd = 23,
	[OriginalName("DamageRate")]
	DamageRate = 24,
	[OriginalName("Knockback")]
	Knockback = 25,
	[OriginalName("SlowDown")]
	SlowDown = 26,
	[OriginalName("SlowTime")]
	SlowTime = 27,
	[OriginalName("Tow")]
	Tow = 28,
	[OriginalName("FreezeTime")]
	FreezeTime = 29,
	[OriginalName("ParalysisTime")]
	ParalysisTime = 30,
	[OriginalName("DazeTime")]
	DazeTime = 31,
	[OriginalName("DianqiuDamage")]
	DianqiuDamage = 32,
	[OriginalName("BurnTime")]
	BurnTime = 33,
	[OriginalName("BurnDamage")]
	BurnDamage = 34,
	[OriginalName("IceDamage")]
	IceDamage = 35,
	[OriginalName("IceTime")]
	IceTime = 36
}
