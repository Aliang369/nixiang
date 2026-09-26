using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSuitSuperSkillRemoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSuitSuperSkillRemoveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLlN1aXRTdXBlclNraWxsUmVtb3ZlQWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5EYXRhQ2hhbmdlLnByb3RvGhl0eXBlLlBsYXllclN1aXRJbmZvLnBy" + "b3RvImkKF1N1aXRTdXBlclNraWxsUmVtb3ZlQWNrEicKCFN1aXRJbmZvGAEg" + "ASgLMhUudHlwZXMuUGxheWVyU3VpdEluZm8SJQoKRGF0YUNoYW5nZRgCIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePlayerSuitInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SuitSuperSkillRemoveAck), (MessageParser)(object)SuitSuperSkillRemoveAck.Parser, new string[2] { "SuitInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
