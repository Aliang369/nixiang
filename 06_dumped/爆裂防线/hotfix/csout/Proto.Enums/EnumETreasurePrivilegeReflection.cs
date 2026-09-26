using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumETreasurePrivilegeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumETreasurePrivilegeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch1lbnVtLkVUcmVhc3VyZVByaXZpbGVnZS5wcm90bxISRVRyZWFzdXJlUHJp" + "dmlsZWdlKnIKEkVUcmVhc3VyZVByaXZpbGVnZRIOCgpFbXB0eVZhbHVlEAAS" + "DwoLQmFyVHJlYXN1cmUQARISCg5NaW5lV2FyQ29sbGVjdBACEhQKEE1pbmVX" + "YXJDb21iYXRCdXkQAxIRCg1SZWNoYXJnZUF3YXJkEARCLloJLi87cHJvdG9j" + "qgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ETreasurePrivilege) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
