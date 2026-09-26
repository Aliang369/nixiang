using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESeasonAreaState
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Neutral")]
	Neutral,
	[OriginalName("Contested")]
	Contested,
	[OriginalName("Occupied")]
	Occupied,
	[OriginalName("Fallen")]
	Fallen
}
