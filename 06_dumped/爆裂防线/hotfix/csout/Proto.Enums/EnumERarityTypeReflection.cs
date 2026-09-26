using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumERarityTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumERarityTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZlbnVtLkVSYXJpdHlUeXBlLnByb3RvEgtFUmFyaXR5VHlwZSpECgtFUmFy" + "aXR5VHlwZRIOCgpFbXB0eVZhbHVlEAASBQoBQhABEgUKAUEQAhIFCgFTEAMS" + "BgoCU1MQBBIICgRSb2xlEApCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPK" + "AhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ERarityType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
