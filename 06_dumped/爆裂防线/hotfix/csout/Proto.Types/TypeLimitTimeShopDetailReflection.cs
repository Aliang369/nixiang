using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitTimeShopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeShopDetailReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkxpbWl0VGltZVNob3BEZXRhaWwucHJvdG8SBXR5cGVzGiJ0eXBl" + "LkxpbWl0VGltZVNob3BJdGVtRGV0YWlsLnByb3RvGhp0eXBlLk9wZXJhdGVC" + "YXNlSW5mby5wcm90byJxChNMaW1pdFRpbWVTaG9wRGV0YWlsEigKCEJhc2VJ" + "bmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZvEjAKCEl0ZW1MaXN0" + "GAIgAygLMh4udHlwZXMuTGltaXRUaW1lU2hvcEl0ZW1EZXRhaWxCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLimitTimeShopItemDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeShopDetail), (MessageParser)(object)LimitTimeShopDetail.Parser, new string[2] { "BaseInfo", "ItemList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
