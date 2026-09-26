using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumECCYTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumECCYTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChNlbnVtLkVDQ1lUeXBlLnByb3RvEghFQ0NZVHlwZSprCghFQ0NZVHlwZRIO" + "CgpFbXB0eVZhbHVlEAASBwoDQ255EAESBwoDSGtkEAISBwoDVHdkEAMSBwoD" + "VXNkEAQSBwoDRXVyEAUSBwoDR2JwEAYSBwoDVm5kEAcSBwoDS3J3EAgSBwoD" + "U2dkEAlCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJv" + "dG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ECCYType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
