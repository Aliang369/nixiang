using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHolidayTaskExchangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHolidayTaskExchangeDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkhvbGlkYXlUYXNrRXhjaGFuZ2VEZXRhaWwucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvIo8BChlIb2xpZGF5VGFza0V4Y2hh" + "bmdlRGV0YWlsEg0KBUluZGV4GAEgASgFEhEKCVNjb3JlQ29zdBgCIAEoBRIS" + "CgpMaW1pdENvdW50GAMgASgFEhUKDUV4Y2hhbmdlQ291bnQYBCABKAUSJQoJ" + "QXdhcmRMaXN0GAUgAygLMhIudHlwZXMuQXdhcmREZXRhaWxCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HolidayTaskExchangeDetail), (MessageParser)(object)HolidayTaskExchangeDetail.Parser, new string[5] { "Index", "ScoreCost", "LimitCount", "ExchangeCount", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
