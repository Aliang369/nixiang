using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleResultInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleResultInfoReflection()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8SBXR5cGVzGhplbnVtLkVC" + "YXR0bGVGdW5jVHlwZS5wcm90bxodZW51bS5FQmF0dGxlUmVzdWx0U2NvcmUu" + "cHJvdG8aJnR5cGUuQmF0dGxlUmVzdWx0RGFtYWdlV2F0Y2hJbmZvLnByb3Rv" + "GiN0eXBlLkJhdHRsZVJlc3VsdFNpZGVUZWFtSW5mby5wcm90byLYAgoQQmF0" + "dGxlUmVzdWx0SW5mbxIMCgRVVUlEGAEgASgJEjQKCkJhdHRsZVR5cGUYAiAB" + "KA4yIC5FQmF0dGxlRnVuY1R5cGUuRUJhdHRsZUZ1bmNUeXBlEjsKC1Jlc3Vs" + "dFNjb3JlGAMgASgOMiYuRUJhdHRsZVJlc3VsdFNjb3JlLkVCYXR0bGVSZXN1" + "bHRTY29yZRIQCghUaW1lVXNlZBgEIAEoBRI5ChBBdHRhY2tlclRlYW1JbmZv" + "GAUgASgLMh8udHlwZXMuQmF0dGxlUmVzdWx0U2lkZVRlYW1JbmZvEjkKEERl" + "ZmVuZGVyVGVhbUluZm8YBiABKAsyHy50eXBlcy5CYXR0bGVSZXN1bHRTaWRl" + "VGVhbUluZm8SOwoPRGFtYWdlV2F0Y2hJbmZvGAcgASgLMiIudHlwZXMuQmF0" + "dGxlUmVzdWx0RGFtYWdlV2F0Y2hJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEBattleFuncTypeReflection.Descriptor,
			EnumEBattleResultScoreReflection.Descriptor,
			TypeBattleResultDamageWatchInfoReflection.Descriptor,
			TypeBattleResultSideTeamInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleResultInfo), (MessageParser)(object)BattleResultInfo.Parser, new string[7] { "UUID", "BattleType", "ResultScore", "TimeUsed", "AttackerTeamInfo", "DefenderTeamInfo", "DamageWatchInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
