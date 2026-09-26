using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateRechargeDailyAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRechargeDailyAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLk9wZXJhdGVSZWNoYXJnZURhaWx5QWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3Rv" + "Gh50eXBlLlJlY2hhcmdlRGFpbHlEZXRhaWwucHJvdG8ingEKF09wZXJhdGVS" + "ZWNoYXJnZURhaWx5QWNrEjUKEVJlY2hhcmdlRGFpbHlMaXN0GAEgAygLMhou" + "dHlwZXMuUmVjaGFyZ2VEYWlseURldGFpbBIlCglBd2FyZExpc3QYAiADKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlw" + "ZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRechargeDailyDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRechargeDailyAck), (MessageParser)(object)OperateRechargeDailyAck.Parser, new string[3] { "RechargeDailyList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
