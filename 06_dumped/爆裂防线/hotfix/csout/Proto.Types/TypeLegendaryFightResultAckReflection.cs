using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegendaryFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegendaryFightResultAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxlZ2VuZGFyeUZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXR0bGVSZXN1bHRJbmZv" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aE3R5cGUuSGVscEluZm8u" + "cHJvdG8aGHR5cGUuTGVnZW5kYXJ5SW5mby5wcm90byLmAQoXTGVnZW5kYXJ5" + "RmlnaHRSZXN1bHRBY2sSLQoMQmF0dGxlU2V0dGxlGAEgASgLMhcudHlwZXMu" + "QmF0dGxlUmVzdWx0SW5mbxIrCg1MZWdlbmRhcnlJbmZvGAIgASgLMhQudHlw" + "ZXMuTGVnZW5kYXJ5SW5mbxIlCglBd2FyZExpc3QYAyADKAsyEi50eXBlcy5B" + "d2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAQgASgLMhEudHlwZXMuRGF0YUNo" + "YW5nZRIhCghIZWxwSW5mbxgFIAEoCzIPLnR5cGVzLkhlbHBJbmZvQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeHelpInfoReflection.Descriptor,
			TypeLegendaryInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegendaryFightResultAck), (MessageParser)(object)LegendaryFightResultAck.Parser, new string[5] { "BattleSettle", "LegendaryInfo", "AwardList", "DataChange", "HelpInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
