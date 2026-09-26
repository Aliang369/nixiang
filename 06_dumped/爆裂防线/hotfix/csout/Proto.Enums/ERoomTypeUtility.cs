using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ERoomTypeUtility
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Hall")]
	Hall,
	[OriginalName("CommRadar")]
	CommRadar,
	[OriginalName("Specimen")]
	Specimen
}
