using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ERefreshType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Daily")]
	Daily,
	[OriginalName("Weekly")]
	Weekly,
	[OriginalName("Monthly")]
	Monthly
}
