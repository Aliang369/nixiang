using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeMainQuestFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMainQuestFightResultAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLk1haW5RdWVzdEZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBlcxoV" + "ZW51bS5FQm9vbFN0YXRlLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3Rv" + "Ght0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8aFXR5cGUuRGF0YUNoYW5n" + "ZS5wcm90bxoYdHlwZS5NYWluUXVlc3RJbmZvLnByb3RvIu4BChdNYWluUXVl" + "c3RGaWdodFJlc3VsdEFjaxIpCglCZXN0U2NvcmUYASABKA4yFi5FQm9vbFN0" + "YXRlLkVCb29sU3RhdGUSLQoMQmF0dGxlU2V0dGxlGAIgASgLMhcudHlwZXMu" + "QmF0dGxlUmVzdWx0SW5mbxIrCg1NYWluUXVlc3RJbmZvGAMgASgLMhQudHlw" + "ZXMuTWFpblF1ZXN0SW5mbxIlCglBd2FyZExpc3QYBCADKAsyEi50eXBlcy5B" + "d2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAUgASgLMhEudHlwZXMuRGF0YUNo" + "YW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMainQuestInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MainQuestFightResultAck), (MessageParser)(object)MainQuestFightResultAck.Parser, new string[5] { "BestScore", "BattleSettle", "MainQuestInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
