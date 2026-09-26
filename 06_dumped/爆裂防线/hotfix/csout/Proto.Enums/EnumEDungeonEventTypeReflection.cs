using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEDungeonEventTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEDungeonEventTypeReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChxlbnVtLkVEdW5nZW9uRXZlbnRUeXBlLnByb3RvEhFFRHVuZ2VvbkV2ZW50" + "VHlwZSpoChFFRHVuZ2VvbkV2ZW50VHlwZRIOCgpFbXB0eVZhbHVlEAASDQoJ" + "RWFzeUZpZ2h0EAESDQoJUmlvdEZpZ2h0EAISDQoJQm9zc0ZpZ2h0EAMSCwoH" + "TW9kUmFuZBAEEgkKBVBvcHVwEAVCLloJLi87cHJvdG9jqgILUHJvdG8uRW51" + "bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EDungeonEventType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
