using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEReminderTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEReminderTypeReflection()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChhlbnVtLkVSZW1pbmRlclR5cGUucHJvdG8SDUVSZW1pbmRlclR5cGUqqwEK" + "DUVSZW1pbmRlclR5cGUSDgoKRW1wdHlWYWx1ZRAAEg8KC0FyZW5hUmVjb3Jk" + "EAESDQoJSGVscEF3YXJkEAISEAoMTGFkZGVyUmVjb3JkEAMSEwoPUGVha0Fy" + "ZW5hUmVjb3JkEAQSDgoKTWluZVJlY29yZBAFEhMKD0Nyb3NzTWluZVJlY29y" + "ZBAGEg8KC1RydWNrUmVjb3JkEAcSDQoJTGVnaW9uV2FyEAhCLloJLi87cHJv" + "dG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EReminderType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
