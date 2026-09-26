using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSubjectChapterInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSubjectChapterInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVTdWJqZWN0Q2hhcHRlckluZm8ucHJvdG8SBXR5cGVz" + "GiZ0eXBlLk9wZXJhdGVTdWJqZWN0Q2hhcHRlckRldGFpbC5wcm90byJnChlP" + "cGVyYXRlU3ViamVjdENoYXB0ZXJJbmZvEhEKCVBhc3NMZXZlbBgBIAEoBRI3" + "CgtDaGFwdGVyTGlzdBgCIAMoCzIiLnR5cGVzLk9wZXJhdGVTdWJqZWN0Q2hh" + "cHRlckRldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeOperateSubjectChapterDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSubjectChapterInfo), (MessageParser)(object)OperateSubjectChapterInfo.Parser, new string[2] { "PassLevel", "ChapterList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
