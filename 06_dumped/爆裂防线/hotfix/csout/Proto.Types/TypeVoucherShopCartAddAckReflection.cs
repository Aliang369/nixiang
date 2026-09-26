using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartAddAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartAddAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlZvdWNoZXJTaG9wQ2FydEFkZEFjay5wcm90bxIFdHlwZXMaGnR5" + "cGUuVm91Y2hlclNob3BJbmZvLnByb3RvIkgKFVZvdWNoZXJTaG9wQ2FydEFk" + "ZEFjaxIvCg9Wb3VjaGVyU2hvcEluZm8YASABKAsyFi50eXBlcy5Wb3VjaGVy" + "U2hvcEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeVoucherShopInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartAddAck), (MessageParser)(object)VoucherShopCartAddAck.Parser, new string[1] { "VoucherShopInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
