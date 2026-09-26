using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEMonsterTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEMonsterTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVNb25zdGVyVHlwZS5wcm90bxIMRU1vbnN0ZXJUeXBlKj8KDEVN" + "b25zdGVyVHlwZRIOCgpFbXB0eVZhbHVlEAASCgoGTm9ybWFsEAESCQoFRWxp" + "dGUQAhIICgRCb3NzEANCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJa" + "YmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EMonsterType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
