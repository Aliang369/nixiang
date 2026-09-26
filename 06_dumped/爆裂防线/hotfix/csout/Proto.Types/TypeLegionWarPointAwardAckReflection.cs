using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarPointAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPointAwardAckReflection()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxlZ2lvbldhclBvaW50QXdhcmRBY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8a" + "HHR5cGUuTGVnaW9uV2FyQmFzZUluZm8ucHJvdG8aHnR5cGUuTGVnaW9uV2Fy" + "TGVnaW9uSW5mby5wcm90bxobdHlwZS5MZWdpb25XYXJNYXBJbmZvLnByb3Rv" + "Ghx0eXBlLkxlZ2lvbldhclNlbGZJbmZvLnByb3RvIrwCChZMZWdpb25XYXJQ" + "b2ludEF3YXJkQWNrEjMKEUxlZ2lvbldhckJhc2VJbmZvGAEgASgLMhgudHlw" + "ZXMuTGVnaW9uV2FyQmFzZUluZm8SNwoTTGVnaW9uV2FyTGVnaW9uSW5mbxgC" + "IAEoCzIaLnR5cGVzLkxlZ2lvbldhckxlZ2lvbkluZm8SMwoRTGVnaW9uV2Fy" + "U2VsZkluZm8YAyABKAsyGC50eXBlcy5MZWdpb25XYXJTZWxmSW5mbxIxChBM" + "ZWdpb25XYXJNYXBJbmZvGAQgASgLMhcudHlwZXMuTGVnaW9uV2FyTWFwSW5m" + "bxIlCglBd2FyZExpc3QYBSADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpE" + "YXRhQ2hhbmdlGAYgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarLegionInfoReflection.Descriptor,
			TypeLegionWarMapInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPointAwardAck), (MessageParser)(object)LegionWarPointAwardAck.Parser, new string[6] { "LegionWarBaseInfo", "LegionWarLegionInfo", "LegionWarSelfInfo", "LegionWarMapInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
