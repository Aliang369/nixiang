using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePeerageInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePeerageInfoReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLlBlZXJhZ2VJbmZvLnByb3RvEgV0eXBlcxoVdHlwZS5UYXNrRGV0" + "YWlsLnByb3RvIkUKC1BlZXJhZ2VJbmZvEhEKCVBlZXJhZ2VJZBgBIAEoBRIj" + "CghUYXNrTGlzdBgCIAMoCzIRLnR5cGVzLlRhc2tEZXRhaWxCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeTaskDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PeerageInfo), (MessageParser)(object)PeerageInfo.Parser, new string[2] { "PeerageId", "TaskList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
