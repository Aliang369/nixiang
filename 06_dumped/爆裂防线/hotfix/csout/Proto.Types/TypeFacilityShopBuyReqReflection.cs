using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityShopBuyReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityShopBuyReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkZhY2lsaXR5U2hvcEJ1eVJlcS5wcm90bxIFdHlwZXMiXAoSRmFj" + "aWxpdHlTaG9wQnV5UmVxEhIKCkZhY2lsaXR5SWQYASABKAUSDgoGUm9vbUlk" + "GAIgASgFEhAKCFNob3BTbG90GAMgASgFEhAKCEJ1eUNvdW50GAQgASgFQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityShopBuyReq), (MessageParser)(object)FacilityShopBuyReq.Parser, new string[4] { "FacilityId", "RoomId", "ShopSlot", "BuyCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
