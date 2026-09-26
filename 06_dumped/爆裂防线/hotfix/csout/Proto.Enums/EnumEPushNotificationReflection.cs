using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEPushNotificationReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEPushNotificationReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVQdXNoTm90aWZpY2F0aW9uLnByb3RvEhFFUHVzaE5vdGlmaWNh" + "dGlvbipyChFFUHVzaE5vdGlmaWNhdGlvbhIOCgpFbXB0eVZhbHVlEAASDAoI" + "VGlsaUZ1bGwQZRIQCgtJZGxlQmFnRnVsbBDJARIXChJMaW1pdE9wZW5MZWdp" + "b25XYXIQrQISFAoPVHJhbnNwb3J0QXJyaXZlEJEDQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLkVudW1zygISWmJnYW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EPushNotification) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
