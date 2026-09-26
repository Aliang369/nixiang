using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOpenServerRankTopAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOpenServerRankTopAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLk9wZW5TZXJ2ZXJSYW5rVG9wQWNrLnByb3RvEgV0eXBlcxobdHlw" + "ZS5PcHNSYW5rVG9wRGV0YWlsLnByb3RvIkQKFE9wZW5TZXJ2ZXJSYW5rVG9w" + "QWNrEiwKC1JhbmtUb3BMaXN0GAEgAygLMhcudHlwZXMuT3BzUmFua1RvcERl" + "dGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeOpsRankTopDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OpenServerRankTopAck), (MessageParser)(object)OpenServerRankTopAck.Parser, new string[1] { "RankTopList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
