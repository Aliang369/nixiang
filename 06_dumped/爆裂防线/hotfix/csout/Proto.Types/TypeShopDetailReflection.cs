using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeShopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeShopDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLlNob3BEZXRhaWwucHJvdG8SBXR5cGVzGhRlbnVtLkVTaG9wVHlw" + "ZS5wcm90bxoZdHlwZS5TaG9wU2xvdERldGFpbC5wcm90byJvCgpTaG9wRGV0" + "YWlsEiYKCFNob3BUeXBlGAEgASgOMhQuRVNob3BUeXBlLkVTaG9wVHlwZRIn" + "CghTbG90TGlzdBgCIAMoCzIVLnR5cGVzLlNob3BTbG90RGV0YWlsEhAKCFZp" + "cExldmVsGAMgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEShopTypeReflection.Descriptor,
			TypeShopSlotDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ShopDetail), (MessageParser)(object)ShopDetail.Parser, new string[3] { "ShopType", "SlotList", "VipLevel" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
