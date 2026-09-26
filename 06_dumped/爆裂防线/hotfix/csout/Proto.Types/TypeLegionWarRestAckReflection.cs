using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarRestAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarRestAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkxlZ2lvbldhclJlc3RBY2sucHJvdG8SBXR5cGVzGhx0eXBlLkxl" + "Z2lvbldhckJhc2VJbmZvLnByb3RvGh50eXBlLkxlZ2lvbldhckxlZ2lvbklu" + "Zm8ucHJvdG8aG3R5cGUuTGVnaW9uV2FyTWFwSW5mby5wcm90bxocdHlwZS5M" + "ZWdpb25XYXJTZWxmSW5mby5wcm90byLoAQoQTGVnaW9uV2FyUmVzdEFjaxIz" + "ChFMZWdpb25XYXJCYXNlSW5mbxgBIAEoCzIYLnR5cGVzLkxlZ2lvbldhckJh" + "c2VJbmZvEjcKE0xlZ2lvbldhckxlZ2lvbkluZm8YAiABKAsyGi50eXBlcy5M" + "ZWdpb25XYXJMZWdpb25JbmZvEjMKEUxlZ2lvbldhclNlbGZJbmZvGAMgASgL" + "MhgudHlwZXMuTGVnaW9uV2FyU2VsZkluZm8SMQoQTGVnaW9uV2FyTWFwSW5m" + "bxgEIAEoCzIXLnR5cGVzLkxlZ2lvbldhck1hcEluZm9CLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarLegionInfoReflection.Descriptor,
			TypeLegionWarMapInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarRestAck), (MessageParser)(object)LegionWarRestAck.Parser, new string[4] { "LegionWarBaseInfo", "LegionWarLegionInfo", "LegionWarSelfInfo", "LegionWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
