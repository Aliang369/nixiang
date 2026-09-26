using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyInfoReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLkpvdXJuZXlJbmZvLnByb3RvEgV0eXBlcxogdHlwZS5Kb3VybmV5" + "Q2hhcHRlck9uZUluZm8ucHJvdG8aInR5cGUuSm91cm5leUNoYXB0ZXJUaHJl" + "ZUluZm8ucHJvdG8aIHR5cGUuSm91cm5leUNoYXB0ZXJUd29JbmZvLnByb3Rv" + "IqcBCgtKb3VybmV5SW5mbxIwCgpDaGFwdGVyT25lGAEgASgLMhwudHlwZXMu" + "Sm91cm5leUNoYXB0ZXJPbmVJbmZvEjAKCkNoYXB0ZXJUd28YAiABKAsyHC50" + "eXBlcy5Kb3VybmV5Q2hhcHRlclR3b0luZm8SNAoMQ2hhcHRlclRocmVlGAMg" + "ASgLMh4udHlwZXMuSm91cm5leUNoYXB0ZXJUaHJlZUluZm9CLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeJourneyChapterOneInfoReflection.Descriptor,
			TypeJourneyChapterThreeInfoReflection.Descriptor,
			TypeJourneyChapterTwoInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyInfo), (MessageParser)(object)JourneyInfo.Parser, new string[3] { "ChapterOne", "ChapterTwo", "ChapterThree" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
