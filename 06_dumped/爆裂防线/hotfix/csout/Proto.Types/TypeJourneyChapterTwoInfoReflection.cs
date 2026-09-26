using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterTwoInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterTwoInfoReflection()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkpvdXJuZXlDaGFwdGVyVHdvSW5mby5wcm90bxIFdHlwZXMaH3R5" + "cGUuSm91cm5leUJhckRyYXdEZXRhaWwucHJvdG8aGnR5cGUuSm91cm5leUJh" + "c2VJbmZvLnByb3RvGiB0eXBlLkpvdXJuZXlFeGNoYW5nZURldGFpbC5wcm90" + "bxoidHlwZS5Kb3VybmV5U3RhdGlzdGljc0RldGFpbC5wcm90bxogdHlwZS5K" + "b3VybmV5WmhhbkxpbmdEZXRhaWwucHJvdG8itAIKFUpvdXJuZXlDaGFwdGVy" + "VHdvSW5mbxIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLkpvdXJuZXlCYXNl" + "SW5mbxIsCgdCYXJEcmF3GAIgASgLMhsudHlwZXMuSm91cm5leUJhckRyYXdE" + "ZXRhaWwSMwoLRGlhbW9uZENvc3QYAyABKAsyHi50eXBlcy5Kb3VybmV5U3Rh" + "dGlzdGljc0RldGFpbBIuCghFeGNoYW5nZRgEIAEoCzIcLnR5cGVzLkpvdXJu" + "ZXlFeGNoYW5nZURldGFpbBIuCgZBY3RpdmUYBSABKAsyHi50eXBlcy5Kb3Vy" + "bmV5U3RhdGlzdGljc0RldGFpbBIuCghaaGFuTGluZxgGIAEoCzIcLnR5cGVz" + "LkpvdXJuZXlaaGFuTGluZ0RldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeJourneyBarDrawDetailReflection.Descriptor,
			TypeJourneyBaseInfoReflection.Descriptor,
			TypeJourneyExchangeDetailReflection.Descriptor,
			TypeJourneyStatisticsDetailReflection.Descriptor,
			TypeJourneyZhanLingDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterTwoInfo), (MessageParser)(object)JourneyChapterTwoInfo.Parser, new string[6] { "BaseInfo", "BarDraw", "DiamondCost", "Exchange", "Active", "ZhanLing" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
