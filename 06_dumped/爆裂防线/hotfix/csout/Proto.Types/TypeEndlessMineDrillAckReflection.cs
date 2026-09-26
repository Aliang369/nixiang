using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessMineDrillAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMineDrillAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkVuZGxlc3NNaW5lRHJpbGxBY2sucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGnR5" + "cGUuRW5kbGVzc01pbmVJbmZvLnByb3RvIpQBChNFbmRsZXNzTWluZURyaWxs" + "QWNrEi8KD0VuZGxlc3NNaW5lSW5mbxgBIAEoCzIWLnR5cGVzLkVuZGxlc3NN" + "aW5lSW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFp" + "bBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeEndlessMineInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMineDrillAck), (MessageParser)(object)EndlessMineDrillAck.Parser, new string[3] { "EndlessMineInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
