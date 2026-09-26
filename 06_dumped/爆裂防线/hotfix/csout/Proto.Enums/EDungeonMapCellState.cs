using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EDungeonMapCellState
{
	[OriginalName("Invisible")]
	Invisible,
	[OriginalName("Visible")]
	Visible,
	[OriginalName("Activate")]
	Activate,
	[OriginalName("LeftOpen")]
	LeftOpen,
	[OriginalName("RightOpen")]
	RightOpen,
	[OriginalName("AllOpen")]
	AllOpen
}
