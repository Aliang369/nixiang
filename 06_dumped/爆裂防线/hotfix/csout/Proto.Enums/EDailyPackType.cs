using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDailyPackType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Free")]
	Free,
	[OriginalName("One")]
	One,
	[OriginalName("Batch")]
	Batch
}
