using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EGemType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Triangle")]
	Triangle,
	[OriginalName("Square")]
	Square,
	[OriginalName("Pentagon")]
	Pentagon,
	[OriginalName("Hexagon")]
	Hexagon,
	[OriginalName("Circle")]
	Circle,
	[OriginalName("Oval")]
	Oval
}
