using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionAuxRewardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionAuxRewardAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbkF1eFJld2FyZEFjay5wcm90bxIFdHlwZXMaFXR5cGUu" + "RGF0YUNoYW5nZS5wcm90bxoVdHlwZS5MZWdpb25JbmZvLnByb3RvGhl0eXBl" + "LkxlZ2lvblNlbGZJbmZvLnByb3RvIpEBChJMZWdpb25BdXhSZXdhcmRBY2sS" + "LQoOTGVnaW9uU2VsZkluZm8YASABKAsyFS50eXBlcy5MZWdpb25TZWxmSW5m" + "bxIlCgpMZWdpb25JbmZvGAIgASgLMhEudHlwZXMuTGVnaW9uSW5mbxIlCgpE" + "YXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeLegionInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionAuxRewardAck), (MessageParser)(object)LegionAuxRewardAck.Parser, new string[3] { "LegionSelfInfo", "LegionInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
