using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroAwakenGeneRefreshHoleAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenGeneRefreshHoleAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkhlcm9Bd2FrZW5HZW5lUmVmcmVzaEhvbGVBY2sucHJvdG8SBXR5" + "cGVzGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGXR5cGUuSGVyb0F3YWtlbklu" + "Zm8ucHJvdG8idAocSGVyb0F3YWtlbkdlbmVSZWZyZXNoSG9sZUFjaxItCg5I" + "ZXJvQXdha2VuSW5mbxgBIAEoCzIVLnR5cGVzLkhlcm9Bd2FrZW5JbmZvEiUK" + "CkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeHeroAwakenInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenGeneRefreshHoleAck), (MessageParser)(object)HeroAwakenGeneRefreshHoleAck.Parser, new string[2] { "HeroAwakenInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
