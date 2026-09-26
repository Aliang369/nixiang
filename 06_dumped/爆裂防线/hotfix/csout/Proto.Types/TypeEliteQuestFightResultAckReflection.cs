using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEliteQuestFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestFightResultAckReflection()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkVsaXRlUXVlc3RGaWdodFJlc3VsdEFjay5wcm90bxIFdHlwZXMa" + "FWVudW0uRUJvb2xTdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90" + "bxobdHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvGhV0eXBlLkRhdGFDaGFu" + "Z2UucHJvdG8aGXR5cGUuRWxpdGVRdWVzdEluZm8ucHJvdG8aE3R5cGUuSGVs" + "cEluZm8ucHJvdG8ilAIKGEVsaXRlUXVlc3RGaWdodFJlc3VsdEFjaxIpCglC" + "ZXN0U2NvcmUYASABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGUSLQoMQmF0" + "dGxlU2V0dGxlGAIgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxItCg5F" + "bGl0ZVF1ZXN0SW5mbxgDIAEoCzIVLnR5cGVzLkVsaXRlUXVlc3RJbmZvEiUK" + "CUF3YXJkTGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFD" + "aGFuZ2UYBSABKAsyES50eXBlcy5EYXRhQ2hhbmdlEiEKCEhlbHBJbmZvGAYg" + "ASgLMg8udHlwZXMuSGVscEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeEliteQuestInfoReflection.Descriptor,
			TypeHelpInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestFightResultAck), (MessageParser)(object)EliteQuestFightResultAck.Parser, new string[6] { "BestScore", "BattleSettle", "EliteQuestInfo", "AwardList", "DataChange", "HelpInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
