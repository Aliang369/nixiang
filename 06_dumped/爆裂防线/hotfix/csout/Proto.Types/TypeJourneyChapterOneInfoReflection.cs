using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterOneInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterOneInfoReflection()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkpvdXJuZXlDaGFwdGVyT25lSW5mby5wcm90bxIFdHlwZXMaH3R5" + "cGUuSm91cm5leUJhckRyYXdEZXRhaWwucHJvdG8aGnR5cGUuSm91cm5leUJh" + "c2VJbmZvLnByb3RvGiB0eXBlLkpvdXJuZXlFeGNoYW5nZURldGFpbC5wcm90" + "bxoidHlwZS5Kb3VybmV5U3RhdGlzdGljc0RldGFpbC5wcm90bxogdHlwZS5K" + "b3VybmV5WmhhbkxpbmdEZXRhaWwucHJvdG8isgIKFUpvdXJuZXlDaGFwdGVy" + "T25lSW5mbxIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLkpvdXJuZXlCYXNl" + "SW5mbxIsCgdCYXJEcmF3GAIgASgLMhsudHlwZXMuSm91cm5leUJhckRyYXdE" + "ZXRhaWwSMwoLUmVjaGFyZ2VTdW0YAyABKAsyHi50eXBlcy5Kb3VybmV5U3Rh" + "dGlzdGljc0RldGFpbBIuCghFeGNoYW5nZRgEIAEoCzIcLnR5cGVzLkpvdXJu" + "ZXlFeGNoYW5nZURldGFpbBIsCgRTaWduGAUgASgLMh4udHlwZXMuSm91cm5l" + "eVN0YXRpc3RpY3NEZXRhaWwSLgoIWmhhbkxpbmcYBiABKAsyHC50eXBlcy5K" + "b3VybmV5WmhhbkxpbmdEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeJourneyBarDrawDetailReflection.Descriptor,
			TypeJourneyBaseInfoReflection.Descriptor,
			TypeJourneyExchangeDetailReflection.Descriptor,
			TypeJourneyStatisticsDetailReflection.Descriptor,
			TypeJourneyZhanLingDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterOneInfo), (MessageParser)(object)JourneyChapterOneInfo.Parser, new string[6] { "BaseInfo", "BarDraw", "RechargeSum", "Exchange", "Sign", "ZhanLing" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
