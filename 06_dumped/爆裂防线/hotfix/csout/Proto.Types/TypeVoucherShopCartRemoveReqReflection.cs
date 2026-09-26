using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeVoucherShopCartRemoveReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeVoucherShopCartRemoveReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLlZvdWNoZXJTaG9wQ2FydFJlbW92ZVJlcS5wcm90bxIFdHlwZXMi" + "OgoYVm91Y2hlclNob3BDYXJ0UmVtb3ZlUmVxEg8KB0dvb2RzSWQYASABKAUS" + "DQoFQ291bnQYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(VoucherShopCartRemoveReq), (MessageParser)(object)VoucherShopCartRemoveReq.Parser, new string[2] { "GoodsId", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
