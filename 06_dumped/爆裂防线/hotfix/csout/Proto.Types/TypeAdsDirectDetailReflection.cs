using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdsDirectDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsDirectDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkFkc0RpcmVjdERldGFpbC5wcm90bxIFdHlwZXMaH3R5cGUuQWRz" + "RGlyZWN0R29vZHNEZXRhaWwucHJvdG8aGnR5cGUuT3BlcmF0ZUJhc2VJbmZv" + "LnByb3RvImsKD0Fkc0RpcmVjdERldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIW" + "LnR5cGVzLk9wZXJhdGVCYXNlSW5mbxIuCglHb29kc0xpc3QYAiADKAsyGy50" + "eXBlcy5BZHNEaXJlY3RHb29kc0RldGFpbEIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAdsDirectGoodsDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsDirectDetail), (MessageParser)(object)AdsDirectDetail.Parser, new string[2] { "BaseInfo", "GoodsList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
