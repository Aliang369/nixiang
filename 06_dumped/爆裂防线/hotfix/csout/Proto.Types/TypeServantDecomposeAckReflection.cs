using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantDecomposeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantDecomposeAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlNlcnZhbnREZWNvbXBvc2VBY2sucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFnR5" + "cGUuU2VydmFudEluZm8ucHJvdG8ijAEKE1NlcnZhbnREZWNvbXBvc2VBY2sS" + "JwoLU2VydmFudEluZm8YASABKAsyEi50eXBlcy5TZXJ2YW50SW5mbxIlCglB" + "d2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hh" + "bmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeServantInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantDecomposeAck), (MessageParser)(object)ServantDecomposeAck.Parser, new string[3] { "ServantInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
