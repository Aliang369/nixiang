using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EBarPoolType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("First")]
	First,
	[OriginalName("FirstTen")]
	FirstTen,
	[OriginalName("CommonTen")]
	CommonTen,
	[OriginalName("RareTen")]
	RareTen,
	[OriginalName("Common")]
	Common
}
