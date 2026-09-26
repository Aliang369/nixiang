using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumELanguageTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumELanguageTypeReflection()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChhlbnVtLkVMYW5ndWFnZVR5cGUucHJvdG8SDUVMYW5ndWFnZVR5cGUqpQEK" + "DUVMYW5ndWFnZVR5cGUSDgoKRW1wdHlWYWx1ZRAAEggKBFpoY24QARIICgRa" + "aHR3EAISBgoCRW4QAxIGCgJWaRAEEgYKAklkEAYSBgoCSmEQBxIGCgJLbxAI" + "EgYKAkZyEAoSBgoCRGUQCxIGCgJUaBAMEgYKAlJ1EA0SBwoDU3BhEA4SBgoC" + "UHQQDxIGCgJJdBAQEgYKAlRyEBESBwoDQXJhEBJCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ELanguageType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
