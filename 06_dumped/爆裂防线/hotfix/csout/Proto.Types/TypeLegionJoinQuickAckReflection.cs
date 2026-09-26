using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionJoinQuickAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionJoinQuickAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbkpvaW5RdWlja0Fjay5wcm90bxIFdHlwZXMaFXR5cGUu" + "TGVnaW9uSW5mby5wcm90bxoZdHlwZS5MZWdpb25TZWxmSW5mby5wcm90byJq" + "ChJMZWdpb25Kb2luUXVpY2tBY2sSLQoOTGVnaW9uU2VsZkluZm8YASABKAsy" + "FS50eXBlcy5MZWdpb25TZWxmSW5mbxIlCgpMZWdpb25JbmZvGAIgASgLMhEu" + "dHlwZXMuTGVnaW9uSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionJoinQuickAck), (MessageParser)(object)LegionJoinQuickAck.Parser, new string[2] { "LegionSelfInfo", "LegionInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
