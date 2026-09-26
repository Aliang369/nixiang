using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarBaseInfoReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkxlZ2lvbldhckJhc2VJbmZvLnByb3RvEgV0eXBlcxocdHlwZS5M" + "ZWdpb25XYXJPcGVuSW5mby5wcm90bxogdHlwZS5MZWdpb25XYXJaaGFuTGlu" + "Z0luZm8ucHJvdG8aFXR5cGUuVGFza0RldGFpbC5wcm90byLDAQoRTGVnaW9u" + "V2FyQmFzZUluZm8SDgoGU2Vhc29uGAEgASgFEjMKEUxlZ2lvbldhck9wZW5J" + "bmZvGAIgASgLMhgudHlwZXMuTGVnaW9uV2FyT3BlbkluZm8SOwoVTGVnaW9u" + "V2FyWmhhbkxpbmdJbmZvGAMgASgLMhwudHlwZXMuTGVnaW9uV2FyWmhhbkxp" + "bmdJbmZvEiwKEUxlZ2lvbldhclRhc2tMaXN0GAQgAygLMhEudHlwZXMuVGFz" + "a0RldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionWarOpenInfoReflection.Descriptor,
			TypeLegionWarZhanLingInfoReflection.Descriptor,
			TypeTaskDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarBaseInfo), (MessageParser)(object)LegionWarBaseInfo.Parser, new string[4] { "Season", "LegionWarOpenInfo", "LegionWarZhanLingInfo", "LegionWarTaskList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
