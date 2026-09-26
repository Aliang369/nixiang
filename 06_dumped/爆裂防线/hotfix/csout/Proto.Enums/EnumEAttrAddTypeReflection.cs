using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEAttrAddTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEAttrAddTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVBdHRyQWRkVHlwZS5wcm90bxIMRUF0dHJBZGRUeXBlKjYKDEVB" + "dHRyQWRkVHlwZRIOCgpFbXB0eVZhbHVlEAASCQoFRml4ZWQQARILCgdQZXJj" + "ZW50EAJCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJv" + "dG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EAttrAddType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
