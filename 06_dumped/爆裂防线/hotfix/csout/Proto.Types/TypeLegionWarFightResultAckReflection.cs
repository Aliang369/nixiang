using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarFightResultAckReflection()
	{
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxlZ2lvbldhckZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXR0bGVSZXN1bHRJbmZv" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aHHR5cGUuTGVnaW9uV2Fy" + "QmFzZUluZm8ucHJvdG8aHnR5cGUuTGVnaW9uV2FyTGVnaW9uSW5mby5wcm90" + "bxoddHlwZS5MZWdpb25XYXJQb2ludEluZm8ucHJvdG8aHHR5cGUuTGVnaW9u" + "V2FyU2VsZkluZm8ucHJvdG8i8AIKF0xlZ2lvbldhckZpZ2h0UmVzdWx0QWNr" + "EjMKEUxlZ2lvbldhckJhc2VJbmZvGAEgASgLMhgudHlwZXMuTGVnaW9uV2Fy" + "QmFzZUluZm8SNwoTTGVnaW9uV2FyTGVnaW9uSW5mbxgCIAEoCzIaLnR5cGVz" + "LkxlZ2lvbldhckxlZ2lvbkluZm8SMwoRTGVnaW9uV2FyU2VsZkluZm8YAyAB" + "KAsyGC50eXBlcy5MZWdpb25XYXJTZWxmSW5mbxI1ChJMZWdpb25XYXJQb2lu" + "dEluZm8YBCABKAsyGS50eXBlcy5MZWdpb25XYXJQb2ludEluZm8SLQoMQmF0" + "dGxlU2V0dGxlGAUgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxIlCglB" + "d2FyZExpc3QYBiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hh" + "bmdlGAcgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[7]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarLegionInfoReflection.Descriptor,
			TypeLegionWarPointInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarFightResultAck), (MessageParser)(object)LegionWarFightResultAck.Parser, new string[7] { "LegionWarBaseInfo", "LegionWarLegionInfo", "LegionWarSelfInfo", "LegionWarPointInfo", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
