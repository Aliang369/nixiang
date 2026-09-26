using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeShopGrowthGiftAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeShopGrowthGiftAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLlJlY2hhcmdlU2hvcEdyb3d0aEdpZnRBY2sucHJvdG8SBXR5cGVz" + "Gh90eXBlLkdyb3d0aEdpZnRTaG9wRGV0YWlsLnByb3RvImEKGVJlY2hhcmdl" + "U2hvcEdyb3d0aEdpZnRBY2sSDwoHR29vZHNJZBgBIAEoBRIzCg5Hcm93dGhH" + "aWZ0U2hvcBgCIAMoCzIbLnR5cGVzLkdyb3d0aEdpZnRTaG9wRGV0YWlsQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeGrowthGiftShopDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeShopGrowthGiftAck), (MessageParser)(object)RechargeShopGrowthGiftAck.Parser, new string[2] { "GoodsId", "GrowthGiftShop" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
