using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ESnowVeilLandType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Item")]
	Item,
	[OriginalName("Turntable")]
	Turntable,
	[OriginalName("Rolling")]
	Rolling,
	[OriginalName("Score")]
	Score,
	[OriginalName("HighScore")]
	HighScore
}
