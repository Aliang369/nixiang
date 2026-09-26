using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossPeakFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossPeakFightResultAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkNyb3NzUGVha0ZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXR0bGVSZXN1bHRJbmZv" + "LnByb3RvGhx0eXBlLkNyb3NzUGVha1NlbGZJbmZvLnByb3RvGhV0eXBlLkRh" + "dGFDaGFuZ2UucHJvdG8aGXR5cGUuU2V0dGxlUmFua0luZm8ucHJvdG8i+gEK" + "F0Nyb3NzUGVha0ZpZ2h0UmVzdWx0QWNrEjMKEUNyb3NzUGVha1NlbGZJbmZv" + "GAEgASgLMhgudHlwZXMuQ3Jvc3NQZWFrU2VsZkluZm8SLQoMQmF0dGxlU2V0" + "dGxlGAIgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxItCg5TZXR0bGVS" + "YW5rSW5mbxgDIAEoCzIVLnR5cGVzLlNldHRsZVJhbmtJbmZvEiUKCUF3YXJk" + "TGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UY" + "BSABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeCrossPeakSelfInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeSettleRankInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossPeakFightResultAck), (MessageParser)(object)CrossPeakFightResultAck.Parser, new string[5] { "CrossPeakSelfInfo", "BattleSettle", "SettleRankInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
