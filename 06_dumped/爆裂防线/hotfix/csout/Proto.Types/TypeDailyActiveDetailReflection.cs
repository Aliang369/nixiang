using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDailyActiveDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDailyActiveDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkRhaWx5QWN0aXZlRGV0YWlsLnByb3RvEgV0eXBlcxoXZW51bS5F" + "Q29tbW9uU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aGnR5" + "cGUuT3BlcmF0ZUJhc2VJbmZvLnByb3RvIp4BChFEYWlseUFjdGl2ZURldGFp" + "bBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVzLk9wZXJhdGVCYXNlSW5mbxIN" + "CgVDb3VudBgCIAEoBRIpCgVTdGF0ZRgDIAEoDjIaLkVDb21tb25TdGF0ZS5F" + "Q29tbW9uU3RhdGUSJQoJQXdhcmRMaXN0GAQgAygLMhIudHlwZXMuQXdhcmRE" + "ZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DailyActiveDetail), (MessageParser)(object)DailyActiveDetail.Parser, new string[4] { "BaseInfo", "Count", "State", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
