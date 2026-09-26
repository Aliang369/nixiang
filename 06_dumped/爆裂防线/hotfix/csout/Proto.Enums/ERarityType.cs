using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ERarityType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("B")]
	B = 1,
	[OriginalName("A")]
	A = 2,
	[OriginalName("S")]
	S = 3,
	[OriginalName("SS")]
	Ss = 4,
	[OriginalName("Role")]
	Role = 10
}
