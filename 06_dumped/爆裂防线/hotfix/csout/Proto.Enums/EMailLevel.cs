using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EMailLevel
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Common")]
	Common,
	[OriginalName("Important")]
	Important
}
