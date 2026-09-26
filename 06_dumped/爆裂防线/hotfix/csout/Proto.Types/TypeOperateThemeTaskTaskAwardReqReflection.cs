using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateThemeTaskTaskAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateThemeTaskTaskAwardReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVUaGVtZVRhc2tUYXNrQXdhcmRSZXEucHJvdG8SBXR5" + "cGVzIjwKHE9wZXJhdGVUaGVtZVRhc2tUYXNrQXdhcmRSZXESDAoEVVVJRBgB" + "IAEoCRIOCgZUYXNrSWQYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateThemeTaskTaskAwardReq), (MessageParser)(object)OperateThemeTaskTaskAwardReq.Parser, new string[2] { "UUID", "TaskId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
