using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDailyPackDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDailyPackDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkRhaWx5UGFja0RldGFpbC5wcm90bxIFdHlwZXMaF2VudW0uRUFj" + "dGl2ZVN0YXRlLnByb3RvIlcKD0RhaWx5UGFja0RldGFpbBIKCgJJZBgBIAEo" + "BRINCgVDb3VudBgCIAEoBRIpCgVTdGF0ZRgDIAEoDjIaLkVBY3RpdmVTdGF0" + "ZS5FQWN0aXZlU3RhdGVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEActiveStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DailyPackDetail), (MessageParser)(object)DailyPackDetail.Parser, new string[3] { "Id", "Count", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
