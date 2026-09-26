using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateSubjectChapterDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSubjectChapterDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVTdWJqZWN0Q2hhcHRlckRldGFpbC5wcm90bxIFdHlw" + "ZXMaF2VudW0uRUNvbW1vblN0YXRlLnByb3RvGiR0eXBlLk9wZXJhdGVTdWJq" + "ZWN0TGV2ZWxEZXRhaWwucHJvdG8ilwEKG09wZXJhdGVTdWJqZWN0Q2hhcHRl" + "ckRldGFpbBIRCglDaGFwdGVySWQYASABKAUSMwoJTGV2ZWxMaXN0GAIgAygL" + "MiAudHlwZXMuT3BlcmF0ZVN1YmplY3RMZXZlbERldGFpbBIwCgxDaGFwdGVy" + "QXdhcmQYAyABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1vblN0YXRlQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeOperateSubjectLevelDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSubjectChapterDetail), (MessageParser)(object)OperateSubjectChapterDetail.Parser, new string[3] { "ChapterId", "LevelList", "ChapterAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
