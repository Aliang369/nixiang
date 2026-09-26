using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeElementTrialFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialFightResultAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkVsZW1lbnRUcmlhbEZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXR0bGVSZXN1bHRJ" + "bmZvLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aG3R5cGUuRWxlbWVu" + "dFRyaWFsSW5mby5wcm90byLMAQoaRWxlbWVudFRyaWFsRmlnaHRSZXN1bHRB" + "Y2sSLQoMQmF0dGxlU2V0dGxlGAEgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0" + "SW5mbxIxChBFbGVtZW50VHJpYWxJbmZvGAIgASgLMhcudHlwZXMuRWxlbWVu" + "dFRyaWFsSW5mbxIlCglBd2FyZExpc3QYAyADKAsyEi50eXBlcy5Bd2FyZERl" + "dGFpbBIlCgpEYXRhQ2hhbmdlGAQgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeElementTrialInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialFightResultAck), (MessageParser)(object)ElementTrialFightResultAck.Parser, new string[4] { "BattleSettle", "ElementTrialInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
