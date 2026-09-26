using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumENoticeStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumENoticeStateReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVOb3RpY2VTdGF0ZS5wcm90bxIMRU5vdGljZVN0YXRlKkUKDEVO" + "b3RpY2VTdGF0ZRIOCgpFbXB0eVZhbHVlEAASCgoGTm9ybWFsEAESCgoGTGF0" + "ZXN0EAISDQoJUmVjb21tZW5kEANCLloJLi87cHJvdG9jqgILUHJvdG8uRW51" + "bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ENoticeState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
