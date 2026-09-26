using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarMarkPointAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarMarkPointAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbldhck1hcmtQb2ludEFjay5wcm90bxIFdHlwZXMaHnR5" + "cGUuTGVnaW9uV2FyTGVnaW9uSW5mby5wcm90bxobdHlwZS5MZWdpb25XYXJN" + "YXBJbmZvLnByb3RvGhx0eXBlLkxlZ2lvbldhclNlbGZJbmZvLnByb3RvIrgB" + "ChVMZWdpb25XYXJNYXJrUG9pbnRBY2sSNwoTTGVnaW9uV2FyTGVnaW9uSW5m" + "bxgBIAEoCzIaLnR5cGVzLkxlZ2lvbldhckxlZ2lvbkluZm8SMwoRTGVnaW9u" + "V2FyU2VsZkluZm8YAiABKAsyGC50eXBlcy5MZWdpb25XYXJTZWxmSW5mbxIx" + "ChBMZWdpb25XYXJNYXBJbmZvGAMgASgLMhcudHlwZXMuTGVnaW9uV2FyTWFw" + "SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionWarLegionInfoReflection.Descriptor,
			TypeLegionWarMapInfoReflection.Descriptor,
			TypeLegionWarSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarMarkPointAck), (MessageParser)(object)LegionWarMarkPointAck.Parser, new string[3] { "LegionWarLegionInfo", "LegionWarSelfInfo", "LegionWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
