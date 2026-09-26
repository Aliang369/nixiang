using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityShopInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityShopInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkZhY2lsaXR5U2hvcEluZm8ucHJvdG8SBXR5cGVzGh50eXBlLkZh" + "Y2lsaXR5R29vZHNEZXRhaWwucHJvdG8iZQoQRmFjaWxpdHlTaG9wSW5mbxIS" + "CgpGYWNpbGl0eUlkGAEgASgFEg4KBlJvb21JZBgCIAEoBRItCglHb29kc0xp" + "c3QYAyADKAsyGi50eXBlcy5GYWNpbGl0eUdvb2RzRGV0YWlsQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFacilityGoodsDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityShopInfo), (MessageParser)(object)FacilityShopInfo.Parser, new string[3] { "FacilityId", "RoomId", "GoodsList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
