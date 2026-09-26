using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantSubSkillFlushAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantSubSkillFlushAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLlNlcnZhbnRTdWJTa2lsbEZsdXNoQWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5EYXRhQ2hhbmdlLnByb3RvGhZ0eXBlLlNlcnZhbnRJbmZvLnByb3Rv" + "ImkKF1NlcnZhbnRTdWJTa2lsbEZsdXNoQWNrEicKC1NlcnZhbnRJbmZvGAEg" + "ASgLMhIudHlwZXMuU2VydmFudEluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIR" + "LnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeServantInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantSubSkillFlushAck), (MessageParser)(object)ServantSubSkillFlushAck.Parser, new string[2] { "ServantInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
