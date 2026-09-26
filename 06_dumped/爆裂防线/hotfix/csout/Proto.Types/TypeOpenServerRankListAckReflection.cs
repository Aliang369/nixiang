using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOpenServerRankListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOpenServerRankListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk9wZW5TZXJ2ZXJSYW5rTGlzdEFjay5wcm90bxIFdHlwZXMaFnR5" + "cGUuT3BzUmFua0luZm8ucHJvdG8iQAoVT3BlblNlcnZlclJhbmtMaXN0QWNr" + "EicKC09wc1JhbmtJbmZvGAEgASgLMhIudHlwZXMuT3BzUmFua0luZm9CLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeOpsRankInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OpenServerRankListAck), (MessageParser)(object)OpenServerRankListAck.Parser, new string[1] { "OpsRankInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
