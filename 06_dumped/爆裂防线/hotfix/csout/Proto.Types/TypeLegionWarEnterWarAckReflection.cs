using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarEnterWarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarEnterWarAckReflection()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkxlZ2lvbldhckVudGVyV2FyQWNrLnByb3RvEgV0eXBlcxocdHlw" + "ZS5MZWdpb25XYXJCYXNlSW5mby5wcm90bxoedHlwZS5MZWdpb25XYXJMZWdp" + "b25JbmZvLnByb3RvGht0eXBlLkxlZ2lvbldhck1hcEluZm8ucHJvdG8aHHR5" + "cGUuTGVnaW9uV2FyU2VsZkluZm8ucHJvdG8aIHR5cGUuTGVnaW9uV2FyU2Vs" + "ZlRlYW1JbmZvLnByb3RvIqkCChRMZWdpb25XYXJFbnRlcldhckFjaxIzChFM" + "ZWdpb25XYXJCYXNlSW5mbxgBIAEoCzIYLnR5cGVzLkxlZ2lvbldhckJhc2VJ" + "bmZvEjcKE0xlZ2lvbldhckxlZ2lvbkluZm8YAiABKAsyGi50eXBlcy5MZWdp" + "b25XYXJMZWdpb25JbmZvEjMKEUxlZ2lvbldhclNlbGZJbmZvGAMgASgLMhgu" + "dHlwZXMuTGVnaW9uV2FyU2VsZkluZm8SOwoVTGVnaW9uV2FyU2VsZlRlYW1J" + "bmZvGAQgASgLMhwudHlwZXMuTGVnaW9uV2FyU2VsZlRlYW1JbmZvEjEKEExl" + "Z2lvbldhck1hcEluZm8YBSABKAsyFy50eXBlcy5MZWdpb25XYXJNYXBJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarLegionInfoReflection.Descriptor,
			TypeLegionWarMapInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor,
			TypeLegionWarSelfTeamInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarEnterWarAck), (MessageParser)(object)LegionWarEnterWarAck.Parser, new string[5] { "LegionWarBaseInfo", "LegionWarLegionInfo", "LegionWarSelfInfo", "LegionWarSelfTeamInfo", "LegionWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
