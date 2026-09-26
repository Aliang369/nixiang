using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELadderStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELadderStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVMYWRkZXJTdGF0ZS5wcm90bxIMRUxhZGRlclN0YXRlKmAKDEVM" + "YWRkZXJTdGF0ZRIOCgpFbXB0eVZhbHVlEAASDwoLTGFkZGVyTWF0Y2gQARIN" + "CglVcGdyYWRpbmcQAhIQCgxVcGdyYWRTdWNlc3MQAxIOCgpVcGdyYWRGYWls" + "EARCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9c" + "RW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELadderState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
