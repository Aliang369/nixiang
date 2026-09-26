using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumERebateStateReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumERebateStateReflection()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVSZWJhdGVTdGF0ZS5wcm90bxIMRVJlYmF0ZVN0YXRlKmAKDEVS" + "ZWJhdGVTdGF0ZRIOCgpFbXB0eVZhbHVlEAASDAoITm9SZWJhdGUQARIOCgpX" + "YWl0UmViYXRlEAISEQoNU3VjY2Vzc1JlYmF0ZRADEg8KC090aGVyUmViYXRl" + "EARCLloJLi87cHJvdG9jqgILUHJvdG8uRW51bXPKAhJaYmdhbWVcUHJvdG9c" + "RW51bXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(ERebateState) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
