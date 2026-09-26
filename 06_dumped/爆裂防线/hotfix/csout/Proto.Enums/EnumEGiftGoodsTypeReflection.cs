using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEGiftGoodsTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEGiftGoodsTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChllbnVtLkVHaWZ0R29vZHNUeXBlLnByb3RvEg5FR2lmdEdvb2RzVHlwZSpH" + "Cg5FR2lmdEdvb2RzVHlwZRIOCgpFbXB0eVZhbHVlEAASCgoGR3Jvd3RoEAES" + "CwoHUGF0aHdheRACEgwKCFZpcExldmVsEANCLloJLi87cHJvdG9jqgILUHJv" + "dG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EGiftGoodsType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
