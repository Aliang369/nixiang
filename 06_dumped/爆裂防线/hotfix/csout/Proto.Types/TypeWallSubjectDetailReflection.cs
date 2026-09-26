using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallSubjectDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallSubjectDetailReflection()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLldhbGxTdWJqZWN0RGV0YWlsLnByb3RvEgV0eXBlcxoadHlwZS5P" + "cGVyYXRlQmFzZUluZm8ucHJvdG8aIHR5cGUuT3BlcmF0ZVN1YmplY3RCYXJJ" + "bmZvLnByb3RvGiR0eXBlLk9wZXJhdGVTdWJqZWN0Q2hhcHRlckluZm8ucHJv" + "dG8aJXR5cGUuT3BlcmF0ZVN1YmplY3RSb3VsZXR0ZUluZm8ucHJvdG8aHXR5" + "cGUuWmhhbkxpbmdDb21tb25JbmZvLnByb3RvIqACChFXYWxsU3ViamVjdERl" + "dGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLk9wZXJhdGVCYXNlSW5m" + "bxIRCglTdWJqZWN0SWQYAiABKAUSNQoLQ2hhcHRlckluZm8YAyABKAsyIC50" + "eXBlcy5PcGVyYXRlU3ViamVjdENoYXB0ZXJJbmZvEi0KB0JhckluZm8YBCAB" + "KAsyHC50eXBlcy5PcGVyYXRlU3ViamVjdEJhckluZm8SNwoMUm91bGV0dGVJ" + "bmZvGAUgASgLMiEudHlwZXMuT3BlcmF0ZVN1YmplY3RSb3VsZXR0ZUluZm8S" + "LwoMWmhhbkxpbmdJbmZvGAYgASgLMhkudHlwZXMuWmhhbkxpbmdDb21tb25J" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateSubjectBarInfoReflection.Descriptor,
			TypeOperateSubjectChapterInfoReflection.Descriptor,
			TypeOperateSubjectRouletteInfoReflection.Descriptor,
			TypeZhanLingCommonInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallSubjectDetail), (MessageParser)(object)WallSubjectDetail.Parser, new string[6] { "BaseInfo", "SubjectId", "ChapterInfo", "BarInfo", "RouletteInfo", "ZhanLingInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
