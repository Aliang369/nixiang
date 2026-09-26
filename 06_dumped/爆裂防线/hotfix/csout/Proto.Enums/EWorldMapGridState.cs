using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EWorldMapGridState
{
	[OriginalName("Invisible")]
	Invisible,
	[OriginalName("Visible")]
	Visible,
	[OriginalName("Explorable")]
	Explorable,
	[OriginalName("Explored")]
	Explored
}
