using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeIncubateStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeIncubateStartAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkluY3ViYXRlU3RhcnRBY2sucHJvdG8SBXR5cGVzGhV0eXBlLkRh" + "dGFDaGFuZ2UucHJvdG8aF3R5cGUuSW5jdWJhdGVJbmZvLnByb3RvImQKEElu" + "Y3ViYXRlU3RhcnRBY2sSKQoMSW5jdWJhdGVJbmZvGAEgASgLMhMudHlwZXMu" + "SW5jdWJhdGVJbmZvEiUKCkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRh" + "Q2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeIncubateInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(IncubateStartAck), (MessageParser)(object)IncubateStartAck.Parser, new string[2] { "IncubateInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
