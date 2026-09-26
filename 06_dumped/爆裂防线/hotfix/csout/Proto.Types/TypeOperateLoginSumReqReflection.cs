using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateLoginSumReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateLoginSumReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLk9wZXJhdGVMb2dpblN1bVJlcS5wcm90bxIFdHlwZXMiMQoST3Bl" + "cmF0ZUxvZ2luU3VtUmVxEgwKBFVVSUQYASABKAkSDQoFSW5kZXgYAiABKAVC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateLoginSumReq), (MessageParser)(object)OperateLoginSumReq.Parser, new string[2] { "UUID", "Index" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
