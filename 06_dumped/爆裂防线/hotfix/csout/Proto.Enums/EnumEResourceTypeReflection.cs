using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEResourceTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEResourceTypeReflection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChhlbnVtLkVSZXNvdXJjZVR5cGUucHJvdG8SDUVSZXNvdXJjZVR5cGUqmQEK" + "DUVSZXNvdXJjZVR5cGUSDgoKRW1wdHlWYWx1ZRAAEgkKBU1vbmV5EAESCAoE" + "UHJvcBACEggKBEl0ZW0QAxIJCgVFcXVpcBAEEggKBEhlcm8QBRIMCghIZXJv" + "RnJhZxAHEgcKA0dlbRAIEgcKA0JveBAKEggKBEdlbmUQCxIKCgZUYWN0aWMQ" + "DBIOCgpUYWN0aWNGcmFnEA1CLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPK" + "AhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EResourceType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
