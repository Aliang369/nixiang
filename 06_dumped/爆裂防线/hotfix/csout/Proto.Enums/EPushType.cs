using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EPushType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("TiliFull")]
	TiliFull,
	[OriginalName("IdleBagFull")]
	IdleBagFull,
	[OriginalName("LimitGameplayOpen")]
	LimitGameplayOpen,
	[OriginalName("TransportArrive")]
	TransportArrive
}
