using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSubjectBarInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSubjectBarInfoReflection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk9wZXJhdGVTdWJqZWN0QmFySW5mby5wcm90bxIFdHlwZXMiTwoV" + "T3BlcmF0ZVN1YmplY3RCYXJJbmZvEg8KB1JhcmVIaXQYASABKAUSEgoKVG90" + "YWxDb3VudBgCIAEoBRIRCglSZWxpY0xpc3QYAyADKAVCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSubjectBarInfo), (MessageParser)(object)OperateSubjectBarInfo.Parser, new string[3] { "RareHit", "TotalCount", "RelicList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
