using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESeasonPhase
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Prepare")]
	Prepare,
	[OriginalName("Fighting")]
	Fighting,
	[OriginalName("MidSettle")]
	MidSettle,
	[OriginalName("FinalSettle")]
	FinalSettle,
	[OriginalName("Closed")]
	Closed
}
