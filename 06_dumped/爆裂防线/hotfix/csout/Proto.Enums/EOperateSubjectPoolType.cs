using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EOperateSubjectPoolType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Common")]
	Common,
	[OriginalName("CommonTen")]
	CommonTen,
	[OriginalName("RareTen")]
	RareTen
}
