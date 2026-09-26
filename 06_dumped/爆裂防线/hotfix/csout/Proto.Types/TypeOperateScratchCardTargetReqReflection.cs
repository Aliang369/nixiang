using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateScratchCardTargetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateScratchCardTargetReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVTY3JhdGNoQ2FyZFRhcmdldFJlcS5wcm90bxIFdHlw" + "ZXMiOgobT3BlcmF0ZVNjcmF0Y2hDYXJkVGFyZ2V0UmVxEgwKBFVVSUQYASAB" + "KAkSDQoFSW5kZXgYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateScratchCardTargetReq), (MessageParser)(object)OperateScratchCardTargetReq.Parser, new string[2] { "UUID", "Index" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
