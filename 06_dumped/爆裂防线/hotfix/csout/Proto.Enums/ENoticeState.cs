using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ENoticeState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Normal")]
	Normal,
	[OriginalName("Latest")]
	Latest,
	[OriginalName("Recommend")]
	Recommend
}
