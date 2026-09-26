using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarModSelectAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarModSelectAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbldhck1vZFNlbGVjdEFjay5wcm90bxIFdHlwZXMaHHR5" + "cGUuTGVnaW9uV2FyQmFzZUluZm8ucHJvdG8aIHR5cGUuTGVnaW9uV2FyU2Vs" + "ZlRlYW1JbmZvLnByb3RvIokBChVMZWdpb25XYXJNb2RTZWxlY3RBY2sSMwoR" + "TGVnaW9uV2FyQmFzZUluZm8YASABKAsyGC50eXBlcy5MZWdpb25XYXJCYXNl" + "SW5mbxI7ChVMZWdpb25XYXJTZWxmVGVhbUluZm8YAiABKAsyHC50eXBlcy5M" + "ZWdpb25XYXJTZWxmVGVhbUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarSelfTeamInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarModSelectAck), (MessageParser)(object)LegionWarModSelectAck.Parser, new string[2] { "LegionWarBaseInfo", "LegionWarSelfTeamInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
