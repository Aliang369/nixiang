using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEFightTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEFightTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChVlbnVtLkVGaWdodFR5cGUucHJvdG8SCkVGaWdodFR5cGUqNAoKRUZpZ2h0" + "VHlwZRIOCgpFbXB0eVZhbHVlEAASCQoFRmlnaHQQARILCgdCZUZpZ2h0EAJC" + "LloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51" + "bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EFightType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
