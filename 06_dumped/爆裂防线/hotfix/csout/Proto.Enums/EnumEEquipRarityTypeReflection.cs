using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEEquipRarityTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEEquipRarityTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVFcXVpcFJhcml0eVR5cGUucHJvdG8SEEVFcXVpcFJhcml0eVR5" + "cGUqRwoQRUVxdWlwUmFyaXR5VHlwZRIOCgpFbXB0eVZhbHVlEAASCgoGQ29t" + "bW9uEAESDQoJRXhjZWxsZW50EAISCAoEUmFyZRADQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EEquipRarityType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
