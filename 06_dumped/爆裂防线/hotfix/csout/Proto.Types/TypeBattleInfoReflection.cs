using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleInfoReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkJhdHRsZUluZm8ucHJvdG8SBXR5cGVzGhl0eXBlLkJhdHRsZUJh" + "c2VJbmZvLnByb3RvGhx0eXBlLkJhdHRsZU1vbnN0ZXJJbmZvLnByb3RvGh10" + "eXBlLkJhdHRsZVNpZGVUZWFtSW5mby5wcm90byLUAQoKQmF0dGxlSW5mbxIt" + "Cg5CYXR0bGVCYXNlSW5mbxgBIAEoCzIVLnR5cGVzLkJhdHRsZUJhc2VJbmZv" + "EjMKEEF0dGFja2VyVGVhbUluZm8YAiABKAsyGS50eXBlcy5CYXR0bGVTaWRl" + "VGVhbUluZm8SMwoQRGVmZW5kZXJUZWFtSW5mbxgDIAEoCzIZLnR5cGVzLkJh" + "dHRsZVNpZGVUZWFtSW5mbxItCgtNb25zdGVySW5mbxgEIAEoCzIYLnR5cGVz" + "LkJhdHRsZU1vbnN0ZXJJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeBattleBaseInfoReflection.Descriptor,
			TypeBattleMonsterInfoReflection.Descriptor,
			TypeBattleSideTeamInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleInfo), (MessageParser)(object)BattleInfo.Parser, new string[4] { "BattleBaseInfo", "AttackerTeamInfo", "DefenderTeamInfo", "MonsterInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
