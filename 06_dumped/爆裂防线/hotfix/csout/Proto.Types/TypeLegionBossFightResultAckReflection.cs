using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionBossFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionBossFightResultAckReflection()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkxlZ2lvbkJvc3NGaWdodFJlc3VsdEFjay5wcm90bxIFdHlwZXMa" + "FnR5cGUuQXdhcmREZXRhaWwucHJvdG8aG3R5cGUuQmF0dGxlUmVzdWx0SW5m" + "by5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhl0eXBlLkxlZ2lvbkJh" + "c2VJbmZvLnByb3RvGhl0eXBlLkxlZ2lvbkJvc3NJbmZvLnByb3RvGhl0eXBl" + "LkxlZ2lvblNlbGZJbmZvLnByb3RvIqQCChhMZWdpb25Cb3NzRmlnaHRSZXN1" + "bHRBY2sSLQoMQmF0dGxlU2V0dGxlGAEgASgLMhcudHlwZXMuQmF0dGxlUmVz" + "dWx0SW5mbxItCg5MZWdpb25CYXNlSW5mbxgCIAEoCzIVLnR5cGVzLkxlZ2lv" + "bkJhc2VJbmZvEi0KDkxlZ2lvblNlbGZJbmZvGAMgASgLMhUudHlwZXMuTGVn" + "aW9uU2VsZkluZm8SLQoOTGVnaW9uQm9zc0luZm8YBCABKAsyFS50eXBlcy5M" + "ZWdpb25Cb3NzSW5mbxIlCglBd2FyZExpc3QYBSADKAsyEi50eXBlcy5Bd2Fy" + "ZERldGFpbBIlCgpEYXRhQ2hhbmdlGAYgASgLMhEudHlwZXMuRGF0YUNoYW5n" + "ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLegionBaseInfoReflection.Descriptor,
			TypeLegionBossInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionBossFightResultAck), (MessageParser)(object)LegionBossFightResultAck.Parser, new string[6] { "BattleSettle", "LegionBaseInfo", "LegionSelfInfo", "LegionBossInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
