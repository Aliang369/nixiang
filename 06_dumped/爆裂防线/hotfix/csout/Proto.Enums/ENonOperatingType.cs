using Google.Protobuf.Reflection;

namespace Proto.Enums;

public enum ENonOperatingType
{
	[OriginalName("EmptyValue")]
	EmptyValue,
	[OriginalName("PreRegistration")]
	PreRegistration,
	[OriginalName("Repayment")]
	Repayment
}
