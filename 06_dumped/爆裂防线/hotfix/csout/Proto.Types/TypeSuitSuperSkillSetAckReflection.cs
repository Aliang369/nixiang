using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSuitSuperSkillSetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSuitSuperSkillSetAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlN1aXRTdXBlclNraWxsU2V0QWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5EYXRhQ2hhbmdlLnByb3RvGhl0eXBlLlBsYXllclN1aXRJbmZvLnByb3Rv" + "ImYKFFN1aXRTdXBlclNraWxsU2V0QWNrEicKCFN1aXRJbmZvGAEgASgLMhUu" + "dHlwZXMuUGxheWVyU3VpdEluZm8SJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5" + "cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerSuitInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SuitSuperSkillSetAck), (MessageParser)(object)SuitSuperSkillSetAck.Parser, new string[2] { "SuitInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
