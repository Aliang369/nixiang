using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EScrollType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("EScroll")]
	Escroll,
	[OriginalName("ETurn")]
	Eturn,
	[OriginalName("EBarrage")]
	Ebarrage
}
