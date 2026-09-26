using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessHordeFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessHordeFightStartAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkVuZGxlc3NIb3JkZUZpZ2h0U3RhcnRBY2sucHJvdG8SBXR5cGVz" + "GhV0eXBlLkJhdHRsZUluZm8ucHJvdG8iUwoZRW5kbGVzc0hvcmRlRmlnaHRT" + "dGFydEFjaxIPCgdMZXZlbElkGAEgASgFEiUKCkJhdHRsZUluZm8YAiABKAsy" + "ES50eXBlcy5CYXR0bGVJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessHordeFightStartAck), (MessageParser)(object)EndlessHordeFightStartAck.Parser, new string[2] { "LevelId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
