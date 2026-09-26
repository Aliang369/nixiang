using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePlayerSuggestDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePlayerSuggestDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlBsYXllclN1Z2dlc3REZXRhaWwucHJvdG8SBXR5cGVzGht0eXBl" + "LkJhdHRsZVJlc3VsdEluZm8ucHJvdG8aF3R5cGUuUm9sZVRpcHNJbmZvLnBy" + "b3RvImsKE1BsYXllclN1Z2dlc3REZXRhaWwSJQoIVGlwc0luZm8YASABKAsy" + "Ey50eXBlcy5Sb2xlVGlwc0luZm8SLQoMQmF0dGxlUmVzdWx0GAIgASgLMhcu" + "dHlwZXMuQmF0dGxlUmVzdWx0SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleResultInfoReflection.Descriptor,
			TypeRoleTipsInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PlayerSuggestDetail), (MessageParser)(object)PlayerSuggestDetail.Parser, new string[2] { "TipsInfo", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
