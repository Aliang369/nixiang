using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePassportBuyLevelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePassportBuyLevelAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlBhc3Nwb3J0QnV5TGV2ZWxBY2sucHJvdG8SBXR5cGVzGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8aHXR5cGUuUmVjaGFyZ2VBY3RpdmVJbmZvLnBy" + "b3RvInMKE1Bhc3Nwb3J0QnV5TGV2ZWxBY2sSNQoSUmVjaGFyZ2VBY3RpdmVJ" + "bmZvGAEgASgLMhkudHlwZXMuUmVjaGFyZ2VBY3RpdmVJbmZvEiUKCkRhdGFD" + "aGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PassportBuyLevelAck), (MessageParser)(object)PassportBuyLevelAck.Parser, new string[2] { "RechargeActiveInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
