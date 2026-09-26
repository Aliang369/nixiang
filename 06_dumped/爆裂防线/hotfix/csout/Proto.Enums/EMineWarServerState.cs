using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EMineWarServerState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Barren")]
	Barren,
	[OriginalName("Normal")]
	Normal,
	[OriginalName("Prosperity")]
	Prosperity
}
