using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EEndlessMinePlotType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Clod")]
	Clod,
	[OriginalName("Stone")]
	Stone,
	[OriginalName("Copper")]
	Copper,
	[OriginalName("Iron")]
	Iron,
	[OriginalName("Gold")]
	Gold,
	[OriginalName("Diamond")]
	Diamond,
	[OriginalName("Box")]
	Box,
	[OriginalName("Drill")]
	Drill,
	[OriginalName("Bomb")]
	Bomb,
	[OriginalName("Pickaxe")]
	Pickaxe
}
