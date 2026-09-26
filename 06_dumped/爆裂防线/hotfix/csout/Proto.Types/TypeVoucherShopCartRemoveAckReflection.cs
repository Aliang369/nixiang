using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartRemoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartRemoveAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLlZvdWNoZXJTaG9wQ2FydFJlbW92ZUFjay5wcm90bxIFdHlwZXMa" + "GnR5cGUuVm91Y2hlclNob3BJbmZvLnByb3RvIksKGFZvdWNoZXJTaG9wQ2Fy" + "dFJlbW92ZUFjaxIvCg9Wb3VjaGVyU2hvcEluZm8YASABKAsyFi50eXBlcy5W" + "b3VjaGVyU2hvcEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeVoucherShopInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartRemoveAck), (MessageParser)(object)VoucherShopCartRemoveAck.Parser, new string[1] { "VoucherShopInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
