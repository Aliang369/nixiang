using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAttrValueType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("IntValue")]
	IntValue,
	[OriginalName("FloatValue")]
	FloatValue
}
