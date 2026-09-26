using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EGeneShape
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Circle")]
	Circle,
	[OriginalName("Square")]
	Square,
	[OriginalName("Triangle")]
	Triangle,
	[OriginalName("Rhombus")]
	Rhombus,
	[OriginalName("Hexagon")]
	Hexagon
}
