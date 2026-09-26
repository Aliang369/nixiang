using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EGender
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Female")]
	Female,
	[OriginalName("Male")]
	Male,
	[OriginalName("Robot")]
	Robot
}
