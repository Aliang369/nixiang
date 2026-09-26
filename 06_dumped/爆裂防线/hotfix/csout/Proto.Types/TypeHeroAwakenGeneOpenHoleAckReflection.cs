using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroAwakenGeneOpenHoleAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneOpenHoleAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkhlcm9Bd2FrZW5HZW5lT3BlbkhvbGVBY2sucHJvdG8SBXR5cGVz" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGXR5cGUuSGVyb0F3YWtlbkluZm8u" + "cHJvdG8icQoZSGVyb0F3YWtlbkdlbmVPcGVuSG9sZUFjaxItCg5IZXJvQXdh" + "a2VuSW5mbxgBIAEoCzIVLnR5cGVzLkhlcm9Bd2FrZW5JbmZvEiUKCkRhdGFD" + "aGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHeroAwakenInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneOpenHoleAck), (MessageParser)(object)HeroAwakenGeneOpenHoleAck.Parser, new string[2] { "HeroAwakenInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
