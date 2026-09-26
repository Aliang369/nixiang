using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEJourneyChapterReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEJourneyChapterReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVKb3VybmV5Q2hhcHRlci5wcm90bxIPRUpvdXJuZXlDaGFwdGVy" + "KlMKD0VKb3VybmV5Q2hhcHRlchIOCgpFbXB0eVZhbHVlEAASDgoKQ2hhcHRl" + "ck9uZRABEg4KCkNoYXB0ZXJUd28QAhIQCgxDaGFwdGVyVGhyZWUQA0IuWgku" + "Lztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EJourneyChapter) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
