using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEAttrValueTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEAttrValueTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChllbnVtLkVBdHRyVmFsdWVUeXBlLnByb3RvEg5FQXR0clZhbHVlVHlwZSo+" + "Cg5FQXR0clZhbHVlVHlwZRIOCgpFbXB0eVZhbHVlEAASDAoISW50VmFsdWUQ" + "ARIOCgpGbG9hdFZhbHVlEAJCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPK" + "AhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EAttrValueType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
