using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEFuncGoodsTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEFuncGoodsTypeReflection()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChllbnVtLkVGdW5jR29vZHNUeXBlLnByb3RvEg5FRnVuY0dvb2RzVHlwZSos" + "Cg5FRnVuY0dvb2RzVHlwZRIOCgpFbXB0eVZhbHVlEAASCgoGQWN0aXZlEAFC" + "LloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9cRW51" + "bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EFuncGoodsType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
