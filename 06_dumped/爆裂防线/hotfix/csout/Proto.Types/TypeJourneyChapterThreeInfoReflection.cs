using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterThreeInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterThreeInfoReflection()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkpvdXJuZXlDaGFwdGVyVGhyZWVJbmZvLnByb3RvEgV0eXBlcxof" + "dHlwZS5Kb3VybmV5QmFyRHJhd0RldGFpbC5wcm90bxoadHlwZS5Kb3VybmV5" + "QmFzZUluZm8ucHJvdG8aIHR5cGUuSm91cm5leUV4Y2hhbmdlRGV0YWlsLnBy" + "b3RvGhx0eXBlLkpvdXJuZXlTaWduRGV0YWlsLnByb3RvGiJ0eXBlLkpvdXJu" + "ZXlTdGF0aXN0aWNzRGV0YWlsLnByb3RvGiB0eXBlLkpvdXJuZXlaaGFuTGlu" + "Z0RldGFpbC5wcm90byKsAgoXSm91cm5leUNoYXB0ZXJUaHJlZUluZm8SKAoI" + "QmFzZUluZm8YASABKAsyFi50eXBlcy5Kb3VybmV5QmFzZUluZm8SLAoHQmFy" + "RHJhdxgCIAEoCzIbLnR5cGVzLkpvdXJuZXlCYXJEcmF3RGV0YWlsEiYKBFNp" + "Z24YAyABKAsyGC50eXBlcy5Kb3VybmV5U2lnbkRldGFpbBIuCghFeGNoYW5n" + "ZRgEIAEoCzIcLnR5cGVzLkpvdXJuZXlFeGNoYW5nZURldGFpbBIxCglEcmF3" + "Q291bnQYBSABKAsyHi50eXBlcy5Kb3VybmV5U3RhdGlzdGljc0RldGFpbBIu" + "CghaaGFuTGluZxgGIAEoCzIcLnR5cGVzLkpvdXJuZXlaaGFuTGluZ0RldGFp" + "bEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeJourneyBarDrawDetailReflection.Descriptor,
			TypeJourneyBaseInfoReflection.Descriptor,
			TypeJourneyExchangeDetailReflection.Descriptor,
			TypeJourneySignDetailReflection.Descriptor,
			TypeJourneyStatisticsDetailReflection.Descriptor,
			TypeJourneyZhanLingDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterThreeInfo), (MessageParser)(object)JourneyChapterThreeInfo.Parser, new string[6] { "BaseInfo", "BarDraw", "Sign", "Exchange", "DrawCount", "ZhanLing" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
