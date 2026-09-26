using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EChatType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("EWorld")]
	Eworld,
	[OriginalName("EServer")]
	Eserver,
	[OriginalName("ELegion")]
	Elegion,
	[OriginalName("EPrivate")]
	Eprivate,
	[OriginalName("ESystem")]
	Esystem
}
