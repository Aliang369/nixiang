using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleTeamInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleTeamInfoReflection()
	{
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkJhdHRsZVRlYW1JbmZvLnByb3RvEgV0eXBlcxoedHlwZS5CYXR0" + "bGVEZWZlbnNlRGV0YWlsLnByb3RvGhp0eXBlLkJhdHRsZU1lY2hhSW5mby5w" + "cm90bxogdHlwZS5CYXR0bGVSZXN1bHRNb2REZXRhaWwucHJvdG8aHnR5cGUu" + "QmF0dGxlU2VydmFudERldGFpbC5wcm90bxocdHlwZS5CYXR0bGVTa2lsbERl" + "dGFpbC5wcm90bxoddHlwZS5CYXR0bGVUYWN0aWNEZXRhaWwucHJvdG8aH3R5" + "cGUuQmF0dGxlVGVhbUF0dHJEZXRhaWwucHJvdG8aIXR5cGUuQmF0dGxlVGVh" + "bUZvb3Rob2xkSW5mby5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8i" + "ywMKDkJhdHRsZVRlYW1JbmZvEikKDFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5" + "cGVzLlJvbGVCYXNlSW5mbxIrCglTa2lsbExpc3QYAiADKAsyGC50eXBlcy5C" + "YXR0bGVTa2lsbERldGFpbBItCghBdHRyTGlzdBgDIAMoCzIbLnR5cGVzLkJh" + "dHRsZVRlYW1BdHRyRGV0YWlsEg8KB01vZExpc3QYBCADKAUSMAoKU2VsZWN0" + "TGlzdBgFIAMoCzIcLnR5cGVzLkJhdHRsZVJlc3VsdE1vZERldGFpbBIpCglN" + "ZWNoYUluZm8YBiABKAsyFi50eXBlcy5CYXR0bGVNZWNoYUluZm8SLwoLRGVm" + "ZW5zZUluZm8YByABKAsyGi50eXBlcy5CYXR0bGVEZWZlbnNlRGV0YWlsEjMK" + "DEZvb3Rob2xkSW5mbxgIIAEoCzIdLnR5cGVzLkJhdHRsZVRlYW1Gb290aG9s" + "ZEluZm8SLwoLU2VydmFudExpc3QYCSADKAsyGi50eXBlcy5CYXR0bGVTZXJ2" + "YW50RGV0YWlsEi0KClRhY3RpY0xpc3QYCiADKAsyGS50eXBlcy5CYXR0bGVU" + "YWN0aWNEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[9]
		{
			TypeBattleDefenseDetailReflection.Descriptor,
			TypeBattleMechaInfoReflection.Descriptor,
			TypeBattleResultModDetailReflection.Descriptor,
			TypeBattleServantDetailReflection.Descriptor,
			TypeBattleSkillDetailReflection.Descriptor,
			TypeBattleTacticDetailReflection.Descriptor,
			TypeBattleTeamAttrDetailReflection.Descriptor,
			TypeBattleTeamFootholdInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleTeamInfo), (MessageParser)(object)BattleTeamInfo.Parser, new string[10] { "RoleBaseInfo", "SkillList", "AttrList", "ModList", "SelectList", "MechaInfo", "DefenseInfo", "FootholdInfo", "ServantList", "TacticList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
