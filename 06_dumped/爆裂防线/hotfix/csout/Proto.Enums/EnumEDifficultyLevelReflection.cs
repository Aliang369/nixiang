using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEDifficultyLevelReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEDifficultyLevelReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChtlbnVtLkVEaWZmaWN1bHR5TGV2ZWwucHJvdG8SEEVEaWZmaWN1bHR5TGV2" + "ZWwqQgoQRURpZmZpY3VsdHlMZXZlbBIOCgpFbXB0eVZhbHVlEAASCAoERWFz" + "eRABEgoKBk5vcm1hbBACEggKBEhhcmQQA0IuWgkuLztwcm90b2OqAgtQcm90" + "by5FbnVtc8oCElpiZ2FtZVxQcm90b1xFbnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EDifficultyLevel) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
