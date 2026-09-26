using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarMapInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarMapInfoAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkxlZ2lvbldhck1hcEluZm9BY2sucHJvdG8SBXR5cGVzGh50eXBl" + "LkxlZ2lvbldhckxlZ2lvbkluZm8ucHJvdG8aG3R5cGUuTGVnaW9uV2FyTWFw" + "SW5mby5wcm90bxocdHlwZS5MZWdpb25XYXJTZWxmSW5mby5wcm90byK2AQoT" + "TGVnaW9uV2FyTWFwSW5mb0FjaxI3ChNMZWdpb25XYXJMZWdpb25JbmZvGAEg" + "ASgLMhoudHlwZXMuTGVnaW9uV2FyTGVnaW9uSW5mbxIzChFMZWdpb25XYXJT" + "ZWxmSW5mbxgCIAEoCzIYLnR5cGVzLkxlZ2lvbldhclNlbGZJbmZvEjEKEExl" + "Z2lvbldhck1hcEluZm8YAyABKAsyFy50eXBlcy5MZWdpb25XYXJNYXBJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionWarLegionInfoReflection.Descriptor,
			TypeLegionWarMapInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarMapInfoAck), (MessageParser)(object)LegionWarMapInfoAck.Parser, new string[3] { "LegionWarLegionInfo", "LegionWarSelfInfo", "LegionWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
