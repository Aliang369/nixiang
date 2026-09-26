using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EAttrAddType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Fixed")]
	Fixed,
	[OriginalName("Percent")]
	Percent
}
