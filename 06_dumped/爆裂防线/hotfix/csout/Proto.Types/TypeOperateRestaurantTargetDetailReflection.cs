using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateRestaurantTargetDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRestaurantTargetDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLk9wZXJhdGVSZXN0YXVyYW50VGFyZ2V0RGV0YWlsLnByb3RvEgV0" + "eXBlcxoXZW51bS5FQ29tbW9uU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRh" + "aWwucHJvdG8ijQEKHU9wZXJhdGVSZXN0YXVyYW50VGFyZ2V0RGV0YWlsEgoK" + "AklkGAEgASgFEg4KBlRhcmdldBgCIAMoBRIpCgVTdGF0ZRgDIAEoDjIaLkVD" + "b21tb25TdGF0ZS5FQ29tbW9uU3RhdGUSJQoJQXdhcmRMaXN0GAQgAygLMhIu" + "dHlwZXMuQXdhcmREZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECommonStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRestaurantTargetDetail), (MessageParser)(object)OperateRestaurantTargetDetail.Parser, new string[4] { "Id", "Target", "State", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
