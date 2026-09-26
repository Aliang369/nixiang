using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESkillAttackType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Elec")]
	Elec,
	[OriginalName("Fire")]
	Fire,
	[OriginalName("Ice")]
	Ice,
	[OriginalName("Phy")]
	Phy,
	[OriginalName("Energy")]
	Energy,
	[OriginalName("Wind")]
	Wind
}
