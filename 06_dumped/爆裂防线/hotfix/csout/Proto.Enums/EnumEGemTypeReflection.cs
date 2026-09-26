using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEGemTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEGemTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChNlbnVtLkVHZW1UeXBlLnByb3RvEghFR2VtVHlwZSplCghFR2VtVHlwZRIO" + "CgpFbXB0eVZhbHVlEAASDAoIVHJpYW5nbGUQARIKCgZTcXVhcmUQAhIMCghQ" + "ZW50YWdvbhADEgsKB0hleGFnb24QBBIKCgZDaXJjbGUQBRIICgRPdmFsEAZC" + "LloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51" + "bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EGemType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
