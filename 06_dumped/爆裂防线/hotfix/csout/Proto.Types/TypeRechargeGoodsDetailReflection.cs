using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRechargeGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeGoodsDetailReflection()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlJlY2hhcmdlR29vZHNEZXRhaWwucHJvdG8SBXR5cGVzGhNlbnVt" + "LkVDQ1lUeXBlLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvIsUBChNS" + "ZWNoYXJnZUdvb2RzRGV0YWlsEg8KB0dvb2RzSWQYASABKAUSHwoDQ0NZGAIg" + "ASgOMhIuRUNDWVR5cGUuRUNDWVR5cGUSDQoFUHJpY2UYAyABKAISEAoITGlt" + "aXROdW0YBCABKAUSEAoITGltaXRWaXAYBSABKAUSEwoLQm91Z2h0Q291bnQY" + "BiABKAUSJQoJQXdhcmRMaXN0GAcgAygLMhIudHlwZXMuQXdhcmREZXRhaWwS" + "DQoFQm9udXMYCCABKAlCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECCYTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeGoodsDetail), (MessageParser)(object)RechargeGoodsDetail.Parser, new string[8] { "GoodsId", "CCY", "Price", "LimitNum", "LimitVip", "BoughtCount", "AwardList", "Bonus" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
