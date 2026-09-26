using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESeasonForce
{
	[OriginalName("ForceNeutral")]
	ForceNeutral,
	[OriginalName("ForceA")]
	ForceA,
	[OriginalName("ForceB")]
	ForceB,
	[OriginalName("ForceC")]
	ForceC,
	[OriginalName("ForceD")]
	ForceD
}
