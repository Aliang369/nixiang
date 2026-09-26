using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateConsumeDailySumAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateConsumeDailySumAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVDb25zdW1lRGFpbHlTdW1BY2sucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGht0eXBlLkNvbnN1bWVTdW1EZXRh" + "aWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byKfAQoZT3BlcmF0ZUNv" + "bnN1bWVEYWlseVN1bUFjaxI0ChNDb25zdW1lRGFpbHlTdW1MaXN0GAEgAygL" + "MhcudHlwZXMuQ29uc3VtZVN1bURldGFpbBIlCglBd2FyZExpc3QYAiADKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlw" + "ZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeConsumeSumDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateConsumeDailySumAck), (MessageParser)(object)OperateConsumeDailySumAck.Parser, new string[3] { "ConsumeDailySumList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
