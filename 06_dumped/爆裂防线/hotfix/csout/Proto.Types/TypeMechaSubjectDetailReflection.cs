using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaSubjectDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaSubjectDetailReflection()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLk1lY2hhU3ViamVjdERldGFpbC5wcm90bxIFdHlwZXMaGnR5cGUu" + "T3BlcmF0ZUJhc2VJbmZvLnByb3RvGiB0eXBlLk9wZXJhdGVTdWJqZWN0QmFy" + "SW5mby5wcm90bxokdHlwZS5PcGVyYXRlU3ViamVjdENoYXB0ZXJJbmZvLnBy" + "b3RvGiV0eXBlLk9wZXJhdGVTdWJqZWN0Um91bGV0dGVJbmZvLnByb3RvGh10" + "eXBlLlpoYW5MaW5nQ29tbW9uSW5mby5wcm90byKhAgoSTWVjaGFTdWJqZWN0" + "RGV0YWlsEigKCEJhc2VJbmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJhc2VJ" + "bmZvEhEKCVN1YmplY3RJZBgCIAEoBRI1CgtDaGFwdGVySW5mbxgDIAEoCzIg" + "LnR5cGVzLk9wZXJhdGVTdWJqZWN0Q2hhcHRlckluZm8SLQoHQmFySW5mbxgE" + "IAEoCzIcLnR5cGVzLk9wZXJhdGVTdWJqZWN0QmFySW5mbxI3CgxSb3VsZXR0" + "ZUluZm8YBSABKAsyIS50eXBlcy5PcGVyYXRlU3ViamVjdFJvdWxldHRlSW5m" + "bxIvCgxaaGFuTGluZ0luZm8YBiABKAsyGS50eXBlcy5aaGFuTGluZ0NvbW1v" + "bkluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateSubjectBarInfoReflection.Descriptor,
			TypeOperateSubjectChapterInfoReflection.Descriptor,
			TypeOperateSubjectRouletteInfoReflection.Descriptor,
			TypeZhanLingCommonInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaSubjectDetail), (MessageParser)(object)MechaSubjectDetail.Parser, new string[6] { "BaseInfo", "SubjectId", "ChapterInfo", "BarInfo", "RouletteInfo", "ZhanLingInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
