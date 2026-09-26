using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEPushTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEPushTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVQdXNoVHlwZS5wcm90bxIJRVB1c2hUeXBlKmYKCUVQdXNoVHlw" + "ZRIOCgpFbXB0eVZhbHVlEAASDAoIVGlsaUZ1bGwQARIPCgtJZGxlQmFnRnVs" + "bBACEhUKEUxpbWl0R2FtZXBsYXlPcGVuEAMSEwoPVHJhbnNwb3J0QXJyaXZl" + "EARCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9c" + "RW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EPushType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
