using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumERefreshTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumERefreshTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVSZWZyZXNoVHlwZS5wcm90bxIMRVJlZnJlc2hUeXBlKkIKDEVS" + "ZWZyZXNoVHlwZRIOCgpFbXB0eVZhbHVlEAASCQoFRGFpbHkQARIKCgZXZWVr" + "bHkQAhILCgdNb250aGx5EANCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPK" + "AhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ERefreshType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
