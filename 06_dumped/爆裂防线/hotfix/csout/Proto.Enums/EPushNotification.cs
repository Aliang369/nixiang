using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EPushNotification
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("TiliFull")]
	TiliFull = 101,
	[OriginalName("IdleBagFull")]
	IdleBagFull = 201,
	[OriginalName("LimitOpenLegionWar")]
	LimitOpenLegionWar = 301,
	[OriginalName("TransportArrive")]
	TransportArrive = 401
}
