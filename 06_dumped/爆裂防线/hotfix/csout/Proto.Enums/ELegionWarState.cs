using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ELegionWarState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Preview")]
	Preview,
	[OriginalName("SignUp")]
	SignUp,
	[OriginalName("Prepare")]
	Prepare,
	[OriginalName("Fighting")]
	Fighting,
	[OriginalName("Settling")]
	Settling,
	[OriginalName("Finished")]
	Finished,
	[OriginalName("WaitNext")]
	WaitNext
}
