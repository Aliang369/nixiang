using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEEquipTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEEquipTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVFcXVpcFR5cGUucHJvdG8SCkVFcXVpcFR5cGUqQwoKRUVxdWlw" + "VHlwZRIOCgpFbXB0eVZhbHVlEAASCgoGTmV1cm9uEAESCAoETW5zchACEg8K" + "C0V4b1NrZWxldG9uEANCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJa" + "YmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EEquipType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
