using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePvpInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePvpInfoReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChJ0eXBlLlB2cEluZm8ucHJvdG8SBXR5cGVzGhV0eXBlLlB2cE1vZEluZm8u" + "cHJvdG8ilQEKB1B2cEluZm8SJwoMVmFsaWRNb2RJbmZvGAEgASgLMhEudHlw" + "ZXMuUHZwTW9kSW5mbxIlCgpTZXRNb2RJbmZvGAIgASgLMhEudHlwZXMuUHZw" + "TW9kSW5mbxIQCghTZXRDb3VudBgDIAEoBRISCgpTZXRNb2RMaXN0GAQgAygF" + "EhQKDFJlZnJlc2hDb3VudBgFIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypePvpModInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PvpInfo), (MessageParser)(object)PvpInfo.Parser, new string[5] { "ValidModInfo", "SetModInfo", "SetCount", "SetModList", "RefreshCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
