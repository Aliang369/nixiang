using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEndlessHordeFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessHordeFightResultAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkVuZGxlc3NIb3JkZUZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBl" + "cxoVZW51bS5FQm9vbFN0YXRlLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnBy" + "b3RvGht0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8aFXR5cGUuRGF0YUNo" + "YW5nZS5wcm90bxobdHlwZS5FbmRsZXNzSG9yZGVJbmZvLnByb3RvIvoBChpF" + "bmRsZXNzSG9yZGVGaWdodFJlc3VsdEFjaxIsCgxCZXN0RGlzdGFuY2UYASAB" + "KA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGUSMQoQRW5kbGVzc0hvcmRlSW5m" + "bxgCIAEoCzIXLnR5cGVzLkVuZGxlc3NIb3JkZUluZm8SLQoMQmF0dGxlU2V0" + "dGxlGAMgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxIlCglBd2FyZExp" + "c3QYBCADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAUg" + "ASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeEndlessHordeInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessHordeFightResultAck), (MessageParser)(object)EndlessHordeFightResultAck.Parser, new string[5] { "BestDistance", "EndlessHordeInfo", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
