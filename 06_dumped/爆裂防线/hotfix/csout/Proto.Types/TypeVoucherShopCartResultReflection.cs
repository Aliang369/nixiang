using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartResultReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartResultReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlZvdWNoZXJTaG9wQ2FydFJlc3VsdC5wcm90bxIFdHlwZXMiXgoV" + "Vm91Y2hlclNob3BDYXJ0UmVzdWx0EgwKBFVVSUQYASABKAkSEQoJQ291cG9u" + "T2ZmGAIgASgFEhAKCFRvdGFsT2ZmGAMgASgFEhIKClRvdGFsUHJpY2UYBCAB" + "KAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartResult), (MessageParser)(object)VoucherShopCartResult.Parser, new string[4] { "UUID", "CouponOff", "TotalOff", "TotalPrice" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
