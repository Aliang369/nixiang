using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESuitType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Hat")]
	Hat,
	[OriginalName("Glasses")]
	Glasses,
	[OriginalName("Top")]
	Top,
	[OriginalName("Under")]
	Under
}
