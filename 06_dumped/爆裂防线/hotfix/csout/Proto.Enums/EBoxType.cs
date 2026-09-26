using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EBoxType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("RandBox")]
	RandBox,
	[OriginalName("SelectBox")]
	SelectBox,
	[OriginalName("ComposeBox")]
	ComposeBox,
	[OriginalName("AwardBox")]
	AwardBox,
	[OriginalName("AutoOpen")]
	AutoOpen
}
