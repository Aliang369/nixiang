using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EOperateGoodsType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("NewServer")]
	NewServer,
	[OriginalName("OldServer")]
	OldServer,
	[OriginalName("Holiday")]
	Holiday,
	[OriginalName("Infinite")]
	Infinite
}
