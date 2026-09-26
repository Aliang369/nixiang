using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EEquipRarityType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Common")]
	Common,
	[OriginalName("Excellent")]
	Excellent,
	[OriginalName("Rare")]
	Rare
}
