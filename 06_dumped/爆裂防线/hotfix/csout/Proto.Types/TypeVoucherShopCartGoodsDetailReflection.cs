using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartGoodsDetailReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLlZvdWNoZXJTaG9wQ2FydEdvb2RzRGV0YWlsLnByb3RvEgV0eXBl" + "cyI8ChpWb3VjaGVyU2hvcENhcnRHb29kc0RldGFpbBIPCgdHb29kc0lkGAEg" + "ASgFEg0KBUNvdW50GAIgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartGoodsDetail), (MessageParser)(object)VoucherShopCartGoodsDetail.Parser, new string[2] { "GoodsId", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
