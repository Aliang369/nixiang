using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBarrageCmdAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBarrageCmdAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLk9wZXJhdGVCYXJyYWdlQ21kQWNrLnByb3RvEgV0eXBlcxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXJyYWdlQ21kRGV0YWlsLnBy" + "b3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ilQEKFE9wZXJhdGVCYXJyYWdl" + "Q21kQWNrEi8KDkJhcnJhZ2VDbWRMaXN0GAEgAygLMhcudHlwZXMuQmFycmFn" + "ZUNtZERldGFpbBIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERl" + "dGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBarrageCmdDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBarrageCmdAck), (MessageParser)(object)OperateBarrageCmdAck.Parser, new string[3] { "BarrageCmdList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
