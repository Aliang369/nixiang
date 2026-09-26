using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectChapterAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectChapterAwardReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLk9wZXJhdGVXYWxsU3ViamVjdENoYXB0ZXJBd2FyZFJlcS5wcm90" + "bxIFdHlwZXMiRAohT3BlcmF0ZVdhbGxTdWJqZWN0Q2hhcHRlckF3YXJkUmVx" + "EgwKBFVVSUQYASABKAkSEQoJQ2hhcHRlcklkGAIgASgFQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectChapterAwardReq), (MessageParser)(object)OperateWallSubjectChapterAwardReq.Parser, new string[2] { "UUID", "ChapterId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
