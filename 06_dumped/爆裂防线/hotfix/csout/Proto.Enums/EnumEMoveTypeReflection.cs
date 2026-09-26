using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEMoveTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEMoveTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVNb3ZlVHlwZS5wcm90bxIJRU1vdmVUeXBlKjAKCUVNb3ZlVHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASCgoGR3JvdW5kEAESBwoDQWlyEAJCLloJLi87" + "cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EMoveType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
