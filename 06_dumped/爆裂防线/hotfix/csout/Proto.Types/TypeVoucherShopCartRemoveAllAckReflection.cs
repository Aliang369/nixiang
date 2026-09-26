using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartRemoveAllAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartRemoveAllAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLlZvdWNoZXJTaG9wQ2FydFJlbW92ZUFsbEFjay5wcm90bxIFdHlw" + "ZXMaGnR5cGUuVm91Y2hlclNob3BJbmZvLnByb3RvIk4KG1ZvdWNoZXJTaG9w" + "Q2FydFJlbW92ZUFsbEFjaxIvCg9Wb3VjaGVyU2hvcEluZm8YASABKAsyFi50" + "eXBlcy5Wb3VjaGVyU2hvcEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeVoucherShopInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartRemoveAllAck), (MessageParser)(object)VoucherShopCartRemoveAllAck.Parser, new string[1] { "VoucherShopInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
