using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityGoodsDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkZhY2lsaXR5R29vZHNEZXRhaWwucHJvdG8SBXR5cGVzImwKE0Zh" + "Y2lsaXR5R29vZHNEZXRhaWwSEAoIU2hvcFNsb3QYASABKAUSDwoHR29vZHNJ" + "ZBgCIAEoBRIOCgZSb29tSWQYAyABKAUSEAoITGltaXROdW0YBCABKAUSEAoI" + "QnV5Q291bnQYBSABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityGoodsDetail), (MessageParser)(object)FacilityGoodsDetail.Parser, new string[5] { "ShopSlot", "GoodsId", "RoomId", "LimitNum", "BuyCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
