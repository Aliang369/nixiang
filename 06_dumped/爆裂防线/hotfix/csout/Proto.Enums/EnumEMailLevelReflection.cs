using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEMailLevelReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEMailLevelReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVNYWlsTGV2ZWwucHJvdG8SCkVNYWlsTGV2ZWwqNwoKRU1haWxM" + "ZXZlbBIOCgpFbXB0eVZhbHVlEAASCgoGQ29tbW9uEAESDQoJSW1wb3J0YW50" + "EAJCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9c" + "RW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EMailLevel) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
