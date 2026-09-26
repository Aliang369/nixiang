using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEGenderReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEGenderReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChJlbnVtLkVHZW5kZXIucHJvdG8SB0VHZW5kZXIqOgoHRUdlbmRlchIOCgpF" + "bXB0eVZhbHVlEAASCgoGRmVtYWxlEAESCAoETWFsZRACEgkKBVJvYm90EANC" + "LloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51" + "bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EGender) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
