using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELegionFuncTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELegionFuncTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVMZWdpb25GdW5jVHlwZS5wcm90bxIPRUxlZ2lvbkZ1bmNUeXBl" + "KkMKD0VMZWdpb25GdW5jVHlwZRIOCgpFbXB0eVZhbHVlEAASCgoGRGlubmVy" + "EAESCgoGUmV3YXJkEAISCAoEQm9zcxADQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELegionFuncType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
