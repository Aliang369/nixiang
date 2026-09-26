using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdsGiftDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsGiftDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkFkc0dpZnREZXRhaWwucHJvdG8SBXR5cGVzGh10eXBlLkFkc0dp" + "ZnRHb29kc0RldGFpbC5wcm90bxoadHlwZS5PcGVyYXRlQmFzZUluZm8ucHJv" + "dG8ieQoNQWRzR2lmdERldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5cGVz" + "Lk9wZXJhdGVCYXNlSW5mbxIsCglHb29kc0xpc3QYAiADKAsyGS50eXBlcy5B" + "ZHNHaWZ0R29vZHNEZXRhaWwSEAoIQnV5Q291bnQYAyABKAVCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAdsGiftGoodsDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsGiftDetail), (MessageParser)(object)AdsGiftDetail.Parser, new string[3] { "BaseInfo", "GoodsList", "BuyCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
