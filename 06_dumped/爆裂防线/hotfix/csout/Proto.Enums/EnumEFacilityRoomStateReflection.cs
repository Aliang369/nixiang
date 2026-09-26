using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEFacilityRoomStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEFacilityRoomStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch1lbnVtLkVGYWNpbGl0eVJvb21TdGF0ZS5wcm90bxISRUZhY2lsaXR5Um9v" + "bVN0YXRlKlkKEkVGYWNpbGl0eVJvb21TdGF0ZRIOCgpFbXB0eVZhbHVlEAAS" + "CgoGTG9ja2VkEAESCQoFUmVhZHkQAhINCglBY3RpdmF0ZWQQAxINCglDb21w" + "bGV0ZWQQBEIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQ" + "cm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EFacilityRoomState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
