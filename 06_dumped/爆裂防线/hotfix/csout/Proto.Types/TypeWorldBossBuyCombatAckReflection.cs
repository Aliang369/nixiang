using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossBuyCombatAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossBuyCombatAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLldvcmxkQm9zc0J1eUNvbWJhdEFjay5wcm90bxIFdHlwZXMaFXR5" + "cGUuRGF0YUNoYW5nZS5wcm90bxocdHlwZS5Xb3JsZEJvc3NTZWxmSW5mby5w" + "cm90byJzChVXb3JsZEJvc3NCdXlDb21iYXRBY2sSMwoRV29ybGRCb3NzU2Vs" + "ZkluZm8YASABKAsyGC50eXBlcy5Xb3JsZEJvc3NTZWxmSW5mbxIlCgpEYXRh" + "Q2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeWorldBossSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossBuyCombatAck), (MessageParser)(object)WorldBossBuyCombatAck.Parser, new string[2] { "WorldBossSelfInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
