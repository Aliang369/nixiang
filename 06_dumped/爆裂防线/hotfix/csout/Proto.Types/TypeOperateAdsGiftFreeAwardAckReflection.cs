using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsGiftFreeAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsGiftFreeAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVBZHNHaWZ0RnJlZUF3YXJkQWNrLnByb3RvEgV0eXBl" + "cxoYdHlwZS5BZHNHaWZ0RGV0YWlsLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWls" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ilQEKGk9wZXJhdGVBZHNH" + "aWZ0RnJlZUF3YXJkQWNrEikKC0Fkc0dpZnRMaXN0GAEgAygLMhQudHlwZXMu" + "QWRzR2lmdERldGFpbBIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2Fy" + "ZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5n" + "ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsGiftDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsGiftFreeAwardAck), (MessageParser)(object)OperateAdsGiftFreeAwardAck.Parser, new string[3] { "AdsGiftList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
