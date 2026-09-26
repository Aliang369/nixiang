using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateDailyActiveReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateDailyActiveReqReflection()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk9wZXJhdGVEYWlseUFjdGl2ZVJlcS5wcm90bxIFdHlwZXMiJQoV" + "T3BlcmF0ZURhaWx5QWN0aXZlUmVxEgwKBFVVSUQYASABKAlCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateDailyActiveReq), (MessageParser)(object)OperateDailyActiveReq.Parser, new string[1] { "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
