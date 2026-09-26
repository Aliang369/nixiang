using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleResultTeamInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleResultTeamInfoReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkJhdHRsZVJlc3VsdFRlYW1JbmZvLnByb3RvEgV0eXBlcxohdHlw" + "ZS5CYXR0bGVSZXN1bHRIZXJvRGV0YWlsLnByb3RvGiB0eXBlLkJhdHRsZVJl" + "c3VsdE1vZERldGFpbC5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8i" + "yQEKFEJhdHRsZVJlc3VsdFRlYW1JbmZvEikKDFJvbGVCYXNlSW5mbxgBIAEo" + "CzITLnR5cGVzLlJvbGVCYXNlSW5mbxIRCglLaWxsQ291bnQYAiABKAUSLwoI" + "SGVyb0xpc3QYAyADKAsyHS50eXBlcy5CYXR0bGVSZXN1bHRIZXJvRGV0YWls" + "Ei0KB01vZExpc3QYBCADKAsyHC50eXBlcy5CYXR0bGVSZXN1bHRNb2REZXRh" + "aWwSEwoLTWVjaGFFbmVyZ3kYBSABKAVCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeBattleResultHeroDetailReflection.Descriptor,
			TypeBattleResultModDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleResultTeamInfo), (MessageParser)(object)BattleResultTeamInfo.Parser, new string[5] { "RoleBaseInfo", "KillCount", "HeroList", "ModList", "MechaEnergy" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
