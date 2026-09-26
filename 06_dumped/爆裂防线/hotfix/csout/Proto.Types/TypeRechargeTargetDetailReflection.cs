using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRechargeTargetDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeTargetDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlJlY2hhcmdlVGFyZ2V0RGV0YWlsLnByb3RvEgV0eXBlcxoXZW51" + "bS5FQ29tbW9uU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwucHJvdG8i" + "hwEKFFJlY2hhcmdlVGFyZ2V0RGV0YWlsEg0KBUluZGV4GAEgASgFEg4KBlRh" + "cmdldBgCIAEoBRIpCgVTdGF0ZRgDIAEoDjIaLkVDb21tb25TdGF0ZS5FQ29t" + "bW9uU3RhdGUSJQoJQXdhcmRMaXN0GAQgAygLMhIudHlwZXMuQXdhcmREZXRh" + "aWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeTargetDetail), (MessageParser)(object)RechargeTargetDetail.Parser, new string[4] { "Index", "Target", "State", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
