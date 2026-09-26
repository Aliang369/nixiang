using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterThreeZhanLingAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterThreeZhanLingAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci50eXBlLkpvdXJuZXlDaGFwdGVyVGhyZWVaaGFuTGluZ0F3YXJkQWNrLnBy" + "b3RvEgV0eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRh" + "Q2hhbmdlLnByb3RvGhZ0eXBlLkpvdXJuZXlJbmZvLnByb3RvIpwBCiNKb3Vy" + "bmV5Q2hhcHRlclRocmVlWmhhbkxpbmdBd2FyZEFjaxInCgtKb3VybmV5SW5m" + "bxgBIAEoCzISLnR5cGVzLkpvdXJuZXlJbmZvEiUKCUF3YXJkTGlzdBgCIAMo" + "CzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50" + "eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterThreeZhanLingAwardAck), (MessageParser)(object)JourneyChapterThreeZhanLingAwardAck.Parser, new string[3] { "JourneyInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
