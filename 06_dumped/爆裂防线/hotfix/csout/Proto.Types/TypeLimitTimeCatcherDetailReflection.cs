using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLimitTimeCatcherDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeCatcherDetailReflection()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxpbWl0VGltZUNhdGNoZXJEZXRhaWwucHJvdG8SBXR5cGVzGhdl" + "bnVtLkVDb21tb25TdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90" + "bxondHlwZS5MaW1pdFRpbWVDYXRjaGVyVGFyZ2V0RGV0YWlsLnByb3RvGhp0" + "eXBlLk9wZXJhdGVCYXNlSW5mby5wcm90byKMAgoWTGltaXRUaW1lQ2F0Y2hl" + "ckRldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLk9wZXJhdGVCYXNl" + "SW5mbxIRCglDb2luQ291bnQYAiABKAUSEAoISGVyb0xpc3QYAyADKAUSNwoK" + "VGFyZ2V0TGlzdBgEIAMoCzIjLnR5cGVzLkxpbWl0VGltZUNhdGNoZXJUYXJn" + "ZXREZXRhaWwSLgoKQXdhcmRTdGF0ZRgFIAEoDjIaLkVDb21tb25TdGF0ZS5F" + "Q29tbW9uU3RhdGUSEwoLRnJhZ1BlckNvaW4YBiABKAUSJQoJQXdhcmRMaXN0" + "GAcgAygLMhIudHlwZXMuQXdhcmREZXRhaWxCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeLimitTimeCatcherTargetDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeCatcherDetail), (MessageParser)(object)LimitTimeCatcherDetail.Parser, new string[7] { "BaseInfo", "CoinCount", "HeroList", "TargetList", "AwardState", "FragPerCoin", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
