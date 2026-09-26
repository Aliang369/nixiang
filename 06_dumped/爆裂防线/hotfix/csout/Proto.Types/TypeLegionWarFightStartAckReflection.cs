using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarFightStartAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxlZ2lvbldhckZpZ2h0U3RhcnRBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkJhdHRsZUluZm8ucHJvdG8aHHR5cGUuTGVnaW9uV2FyU2VsZkluZm8u" + "cHJvdG8inAEKFkxlZ2lvbldhckZpZ2h0U3RhcnRBY2sSEAoIUGxheWVySWQY" + "ASABKAUSFAoMQWN0aW9uUG9pbnRzGAIgASgFEiUKCkJhdHRsZUluZm8YAyAB" + "KAsyES50eXBlcy5CYXR0bGVJbmZvEjMKEUxlZ2lvbldhclNlbGZJbmZvGAQg" + "ASgLMhgudHlwZXMuTGVnaW9uV2FyU2VsZkluZm9CLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarFightStartAck), (MessageParser)(object)LegionWarFightStartAck.Parser, new string[4] { "PlayerId", "ActionPoints", "BattleInfo", "LegionWarSelfInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
