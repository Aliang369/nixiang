using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartCheckOutAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartCheckOutAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLlZvdWNoZXJTaG9wQ2FydENoZWNrT3V0QWNrLnByb3RvEgV0eXBl" + "cxoadHlwZS5Wb3VjaGVyU2hvcEluZm8ucHJvdG8iTQoaVm91Y2hlclNob3BD" + "YXJ0Q2hlY2tPdXRBY2sSLwoPVm91Y2hlclNob3BJbmZvGAEgASgLMhYudHlw" + "ZXMuVm91Y2hlclNob3BJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeVoucherShopInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartCheckOutAck), (MessageParser)(object)VoucherShopCartCheckOutAck.Parser, new string[1] { "VoucherShopInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
