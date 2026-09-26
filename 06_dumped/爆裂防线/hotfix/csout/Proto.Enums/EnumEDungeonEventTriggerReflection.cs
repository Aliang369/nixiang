using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEDungeonEventTriggerReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEDungeonEventTriggerReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch9lbnVtLkVEdW5nZW9uRXZlbnRUcmlnZ2VyLnByb3RvEhRFRHVuZ2VvbkV2" + "ZW50VHJpZ2dlcipKChRFRHVuZ2VvbkV2ZW50VHJpZ2dlchIOCgpFbXB0eVZh" + "bHVlEAASCQoFRm9yY2UQARIKCgZBY3RpdmUQAhILCgdQYXNzaXZlEANCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EDungeonEventTrigger) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
