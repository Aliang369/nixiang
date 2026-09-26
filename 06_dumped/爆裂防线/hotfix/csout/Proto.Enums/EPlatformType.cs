using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum EPlatformType
{
	[OriginalName("EmptyValue")]
	EmptyValue = 0,
	[OriginalName("Dev")]
	Dev = 1,
	[OriginalName("Test")]
	Test = 2,
	[OriginalName("Eva")]
	Eva = 3,
	[OriginalName("Demo")]
	Demo = 4,
	[OriginalName("Beta1")]
	Beta1 = 5,
	[OriginalName("Beta2")]
	Beta2 = 6,
	[OriginalName("Beta3")]
	Beta3 = 7,
	[OriginalName("Ga1")]
	Ga1 = 11,
	[OriginalName("Ga2")]
	Ga2 = 12
}
