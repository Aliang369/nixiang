using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateMechaSubjectBarDrawAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateMechaSubjectBarDrawAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLk9wZXJhdGVNZWNoYVN1YmplY3RCYXJEcmF3QWNrLnByb3RvEgV0" + "eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdl" + "LnByb3RvGh10eXBlLk1lY2hhU3ViamVjdERldGFpbC5wcm90byK1AQodT3Bl" + "cmF0ZU1lY2hhU3ViamVjdEJhckRyYXdBY2sSMwoQTWVjaGFTdWJqZWN0TGlz" + "dBgBIAMoCzIZLnR5cGVzLk1lY2hhU3ViamVjdERldGFpbBIRCglSZWxpY0xp" + "c3QYAiADKAUSJQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRh" + "aWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMechaSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateMechaSubjectBarDrawAck), (MessageParser)(object)OperateMechaSubjectBarDrawAck.Parser, new string[4] { "MechaSubjectList", "RelicList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
