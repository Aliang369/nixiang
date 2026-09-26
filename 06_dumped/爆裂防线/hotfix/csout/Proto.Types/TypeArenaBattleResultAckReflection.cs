using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaBattleResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaBattleResultAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkFyZW5hQmF0dGxlUmVzdWx0QWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5CYXR0bGVJbmZvLnByb3RvGht0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJv" + "dG8ibAoUQXJlbmFCYXR0bGVSZXN1bHRBY2sSJQoKQmF0dGxlSW5mbxgBIAEo" + "CzIRLnR5cGVzLkJhdHRsZUluZm8SLQoMQmF0dGxlU2V0dGxlGAIgASgLMhcu" + "dHlwZXMuQmF0dGxlUmVzdWx0SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleInfoReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaBattleResultAck), (MessageParser)(object)ArenaBattleResultAck.Parser, new string[2] { "BattleInfo", "BattleSettle" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
