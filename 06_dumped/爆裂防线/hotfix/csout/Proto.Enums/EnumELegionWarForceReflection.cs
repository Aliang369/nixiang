using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELegionWarForceReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELegionWarForceReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChplbnVtLkVMZWdpb25XYXJGb3JjZS5wcm90bxIPRUxlZ2lvbldhckZvcmNl" + "KjQKD0VMZWdpb25XYXJGb3JjZRIOCgpFbXB0eVZhbHVlEAASBwoDUmVkEAES" + "CAoEQmx1ZRACQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJnYW1l" + "XFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELegionWarForce) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
