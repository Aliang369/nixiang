using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEDefenseTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEDefenseTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVEZWZlbnNlVHlwZS5wcm90bxIMRURlZmVuc2VUeXBlKj8KDEVE" + "ZWZlbnNlVHlwZRIOCgpFbXB0eVZhbHVlEAASDwoLV2FsbERlZmVuc2UQARIO" + "CgpDYXJEZWZlbnNlEAJCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJa" + "YmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EDefenseType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
