using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSubscribeInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSubscribeInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLlN1YnNjcmliZUluZm8ucHJvdG8SBXR5cGVzGh50eXBlLlN1YnNj" + "cmliZUFjdGl2ZUluZm8ucHJvdG8aH3R5cGUuU3Vic2NyaWJlR29vZHNEZXRh" + "aWwucHJvdG8ibwoNU3Vic2NyaWJlSW5mbxIuCgpBY3RpdmVJbmZvGAEgASgL" + "MhoudHlwZXMuU3Vic2NyaWJlQWN0aXZlSW5mbxIuCglHb29kc0xpc3QYAiAD" + "KAsyGy50eXBlcy5TdWJzY3JpYmVHb29kc0RldGFpbEIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeSubscribeActiveInfoReflection.Descriptor,
			TypeSubscribeGoodsDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SubscribeInfo), (MessageParser)(object)SubscribeInfo.Parser, new string[2] { "ActiveInfo", "GoodsList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
