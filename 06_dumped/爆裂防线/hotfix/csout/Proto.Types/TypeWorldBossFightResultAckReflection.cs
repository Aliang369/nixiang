using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossFightResultAckReflection()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLldvcmxkQm9zc0ZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXR0bGVSZXN1bHRJbmZv" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aE3R5cGUuUmFua0RhdGEu" + "cHJvdG8aGHR5cGUuV29ybGRCb3NzSW5mby5wcm90bxocdHlwZS5Xb3JsZEJv" + "c3NTZWxmSW5mby5wcm90byKkAgoXV29ybGRCb3NzRmlnaHRSZXN1bHRBY2sS" + "LQoMQmF0dGxlU2V0dGxlGAEgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5m" + "bxIzChFXb3JsZEJvc3NTZWxmSW5mbxgCIAEoCzIYLnR5cGVzLldvcmxkQm9z" + "c1NlbGZJbmZvEioKEVdvcmxkQm9zc1NlbGZSYW5rGAMgASgLMg8udHlwZXMu" + "UmFua0RhdGESKwoNV29ybGRCb3NzSW5mbxgEIAEoCzIULnR5cGVzLldvcmxk" + "Qm9zc0luZm8SJQoJQXdhcmRMaXN0GAUgAygLMhIudHlwZXMuQXdhcmREZXRh" + "aWwSJQoKRGF0YUNoYW5nZRgGIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRankDataReflection.Descriptor,
			TypeWorldBossInfoReflection.Descriptor,
			TypeWorldBossSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossFightResultAck), (MessageParser)(object)WorldBossFightResultAck.Parser, new string[6] { "BattleSettle", "WorldBossSelfInfo", "WorldBossSelfRank", "WorldBossInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
