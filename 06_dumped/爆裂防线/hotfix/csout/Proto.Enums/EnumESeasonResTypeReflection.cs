using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumESeasonResTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumESeasonResTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChllbnVtLkVTZWFzb25SZXNUeXBlLnByb3RvEg5FU2Vhc29uUmVzVHlwZSoz" + "Cg5FU2Vhc29uUmVzVHlwZRIOCgpFbXB0eVZhbHVlEAASBwoDR2FzEAESCAoE" + "VGVjaBACQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1lXFBy" + "b3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ESeasonResType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
