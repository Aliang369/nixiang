using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionBossSaoDangAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionBossSaoDangAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkxlZ2lvbkJvc3NTYW9EYW5nQWNrLnByb3RvEgV0eXBlcxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhl0" + "eXBlLkxlZ2lvbkJhc2VJbmZvLnByb3RvGhl0eXBlLkxlZ2lvbkJvc3NJbmZv" + "LnByb3RvGhl0eXBlLkxlZ2lvblNlbGZJbmZvLnByb3RvIvEBChRMZWdpb25C" + "b3NzU2FvRGFuZ0FjaxItCg5MZWdpb25CYXNlSW5mbxgBIAEoCzIVLnR5cGVz" + "LkxlZ2lvbkJhc2VJbmZvEi0KDkxlZ2lvblNlbGZJbmZvGAIgASgLMhUudHlw" + "ZXMuTGVnaW9uU2VsZkluZm8SLQoOTGVnaW9uQm9zc0luZm8YAyABKAsyFS50" + "eXBlcy5MZWdpb25Cb3NzSW5mbxIlCglBd2FyZExpc3QYBCADKAsyEi50eXBl" + "cy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAUgASgLMhEudHlwZXMuRGF0" + "YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLegionBaseInfoReflection.Descriptor,
			TypeLegionBossInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionBossSaoDangAck), (MessageParser)(object)LegionBossSaoDangAck.Parser, new string[5] { "LegionBaseInfo", "LegionSelfInfo", "LegionBossInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
