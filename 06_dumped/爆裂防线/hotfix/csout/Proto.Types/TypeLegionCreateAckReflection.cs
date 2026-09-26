using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionCreateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionCreateAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkxlZ2lvbkNyZWF0ZUFjay5wcm90bxIFdHlwZXMaFXR5cGUuRGF0" + "YUNoYW5nZS5wcm90bxoVdHlwZS5MZWdpb25JbmZvLnByb3RvGhl0eXBlLkxl" + "Z2lvblNlbGZJbmZvLnByb3RvIo4BCg9MZWdpb25DcmVhdGVBY2sSLQoOTGVn" + "aW9uU2VsZkluZm8YASABKAsyFS50eXBlcy5MZWdpb25TZWxmSW5mbxIlCgpM" + "ZWdpb25JbmZvGAIgASgLMhEudHlwZXMuTGVnaW9uSW5mbxIlCgpEYXRhQ2hh" + "bmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeLegionInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionCreateAck), (MessageParser)(object)LegionCreateAck.Parser, new string[3] { "LegionSelfInfo", "LegionInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
