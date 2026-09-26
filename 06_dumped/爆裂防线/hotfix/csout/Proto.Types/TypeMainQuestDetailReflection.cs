using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeMainQuestDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMainQuestDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLk1haW5RdWVzdERldGFpbC5wcm90bxIFdHlwZXMaHWVudW0uRUJh" + "dHRsZVJlc3VsdFNjb3JlLnByb3RvIqkBCg9NYWluUXVlc3REZXRhaWwSDwoH" + "TGV2ZWxJZBgBIAEoBRISCgpEZWZlbnNlUGN0GAIgASgCEjUKBVNjb3JlGAMg" + "ASgOMiYuRUJhdHRsZVJlc3VsdFNjb3JlLkVCYXR0bGVSZXN1bHRTY29yZRI6" + "CgpTY29yZUF3YXJkGAQgASgOMiYuRUJhdHRsZVJlc3VsdFNjb3JlLkVCYXR0" + "bGVSZXN1bHRTY29yZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBattleResultScoreReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MainQuestDetail), (MessageParser)(object)MainQuestDetail.Parser, new string[4] { "LevelId", "DefensePct", "Score", "ScoreAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
