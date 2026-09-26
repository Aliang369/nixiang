using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilRollingAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilRollingAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVTbm93VmVpbFJvbGxpbmdBY2sucHJvdG8SBXR5cGVz" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGXR5cGUuU25vd1ZlaWxEZXRhaWwu" + "cHJvdG8ibwoZT3BlcmF0ZVNub3dWZWlsUm9sbGluZ0FjaxIrCgxTbm93VmVp" + "bExpc3QYASADKAsyFS50eXBlcy5Tbm93VmVpbERldGFpbBIlCgpEYXRhQ2hh" + "bmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilRollingAck), (MessageParser)(object)OperateSnowVeilRollingAck.Parser, new string[2] { "SnowVeilList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
