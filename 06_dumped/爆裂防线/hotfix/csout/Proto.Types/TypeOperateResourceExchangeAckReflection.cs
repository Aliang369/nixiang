using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateResourceExchangeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateResourceExchangeAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVSZXNvdXJjZUV4Y2hhbmdlQWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnBy" + "b3RvGiF0eXBlLlJlc291cmNlRXhjaGFuZ2VEZXRhaWwucHJvdG8ipwEKGk9w" + "ZXJhdGVSZXNvdXJjZUV4Y2hhbmdlQWNrEjsKFFJlc291cmNlRXhjaGFuZ2VM" + "aXN0GAEgAygLMh0udHlwZXMuUmVzb3VyY2VFeGNoYW5nZURldGFpbBIlCglB" + "d2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hh" + "bmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeResourceExchangeDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateResourceExchangeAck), (MessageParser)(object)OperateResourceExchangeAck.Parser, new string[3] { "ResourceExchangeList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
