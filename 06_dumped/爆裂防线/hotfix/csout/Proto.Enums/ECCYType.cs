using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ECCYType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("Cny")]
	Cny,
	[OriginalName("Hkd")]
	Hkd,
	[OriginalName("Twd")]
	Twd,
	[OriginalName("Usd")]
	Usd,
	[OriginalName("Eur")]
	Eur,
	[OriginalName("Gbp")]
	Gbp,
	[OriginalName("Vnd")]
	Vnd,
	[OriginalName("Krw")]
	Krw,
	[OriginalName("Sgd")]
	Sgd
}
