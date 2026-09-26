using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartCouponDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartCouponDetailReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLlZvdWNoZXJTaG9wQ2FydENvdXBvbkRldGFpbC5wcm90bxIFdHlw" + "ZXMiPgobVm91Y2hlclNob3BDYXJ0Q291cG9uRGV0YWlsEgwKBFVVSUQYASAB" + "KAkSEQoJQ291cG9uT2ZmGAIgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartCouponDetail), (MessageParser)(object)VoucherShopCartCouponDetail.Parser, new string[2] { "UUID", "CouponOff" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
