using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EWorkerRarityType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("B")]
	B,
	[OriginalName("A")]
	A,
	[OriginalName("S")]
	S,
	[OriginalName("SS")]
	Ss
}
