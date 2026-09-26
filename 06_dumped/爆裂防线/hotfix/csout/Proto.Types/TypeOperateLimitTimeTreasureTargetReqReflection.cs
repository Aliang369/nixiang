using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateLimitTimeTreasureTargetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateLimitTimeTreasureTargetReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLk9wZXJhdGVMaW1pdFRpbWVUcmVhc3VyZVRhcmdldFJlcS5wcm90" + "bxIFdHlwZXMiQAohT3BlcmF0ZUxpbWl0VGltZVRyZWFzdXJlVGFyZ2V0UmVx" + "EgwKBFVVSUQYASABKAkSDQoFSW5kZXgYAiABKAVCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateLimitTimeTreasureTargetReq), (MessageParser)(object)OperateLimitTimeTreasureTargetReq.Parser, new string[2] { "UUID", "Index" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
