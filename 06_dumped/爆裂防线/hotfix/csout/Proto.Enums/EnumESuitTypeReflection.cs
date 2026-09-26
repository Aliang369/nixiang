using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESuitTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESuitTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVTdWl0VHlwZS5wcm90bxIJRVN1aXRUeXBlKkUKCUVTdWl0VHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASBwoDSGF0EAESCwoHR2xhc3NlcxACEgcKA1Rv" + "cBADEgkKBVVuZGVyEARCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJa" + "YmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESuitType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
