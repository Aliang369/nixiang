using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsSignFreeAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsSignFreeAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVBZHNTaWduRnJlZUF3YXJkQWNrLnByb3RvEgV0eXBl" + "cxoYdHlwZS5BZHNTaWduRGV0YWlsLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWls" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ilQEKGk9wZXJhdGVBZHNT" + "aWduRnJlZUF3YXJkQWNrEikKC0Fkc1NpZ25MaXN0GAEgAygLMhQudHlwZXMu" + "QWRzU2lnbkRldGFpbBIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2Fy" + "ZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5n" + "ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsSignDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsSignFreeAwardAck), (MessageParser)(object)OperateAdsSignFreeAwardAck.Parser, new string[3] { "AdsSignList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
