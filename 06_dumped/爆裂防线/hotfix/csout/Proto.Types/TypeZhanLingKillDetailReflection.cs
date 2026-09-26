using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeZhanLingKillDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeZhanLingKillDetailReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlpoYW5MaW5nS2lsbERldGFpbC5wcm90bxIFdHlwZXMaGnR5cGUu" + "T3BlcmF0ZUJhc2VJbmZvLnByb3RvGiV0eXBlLk9wZXJhdGVaaGFuTGluZ0V4" + "dHJhRGV0YWlsLnByb3RvGh50eXBlLk9wZXJhdGVaaGFuTGluZ0luZm8ucHJv" + "dG8iqwEKElpoYW5MaW5nS2lsbERldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIW" + "LnR5cGVzLk9wZXJhdGVCYXNlSW5mbxIwCgxaaGFuTGluZ0luZm8YAiABKAsy" + "Gi50eXBlcy5PcGVyYXRlWmhhbkxpbmdJbmZvEjkKDkdyYW5kRXh0cmFMaXN0" + "GAMgAygLMiEudHlwZXMuT3BlcmF0ZVpoYW5MaW5nRXh0cmFEZXRhaWxCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateZhanLingExtraDetailReflection.Descriptor,
			TypeOperateZhanLingInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ZhanLingKillDetail), (MessageParser)(object)ZhanLingKillDetail.Parser, new string[3] { "BaseInfo", "ZhanLingInfo", "GrandExtraList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
