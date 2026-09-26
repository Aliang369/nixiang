using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeIncubateCompleteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeIncubateCompleteAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkluY3ViYXRlQ29tcGxldGVBY2sucHJvdG8SBXR5cGVzGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8aF3R5cGUuSW5jdWJhdGVJbmZvLnByb3RvGhZ0" + "eXBlLlNlcnZhbnRJbmZvLnByb3RvIpABChNJbmN1YmF0ZUNvbXBsZXRlQWNr" + "EicKC1NlcnZhbnRJbmZvGAEgASgLMhIudHlwZXMuU2VydmFudEluZm8SKQoM" + "SW5jdWJhdGVJbmZvGAIgASgLMhMudHlwZXMuSW5jdWJhdGVJbmZvEiUKCkRh" + "dGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeIncubateInfoReflection.Descriptor,
			TypeServantInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(IncubateCompleteAck), (MessageParser)(object)IncubateCompleteAck.Parser, new string[3] { "ServantInfo", "IncubateInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
