using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionBossKillRecordDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionBossKillRecordDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkxlZ2lvbkJvc3NLaWxsUmVjb3JkRGV0YWlsLnByb3RvEgV0eXBl" + "cxobdHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvGhd0eXBlLlJvbGVCYXNl" + "SW5mby5wcm90byKSAQoaTGVnaW9uQm9zc0tpbGxSZWNvcmREZXRhaWwSKQoM" + "Um9sZUJhc2VJbmZvGAEgASgLMhMudHlwZXMuUm9sZUJhc2VJbmZvEi0KDEJh" + "dHRsZVJlc3VsdBgCIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm8SDAoE" + "S2lsbBgDIAEoBRIMCgRUaW1lGAQgASgDQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleResultInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionBossKillRecordDetail), (MessageParser)(object)LegionBossKillRecordDetail.Parser, new string[4] { "RoleBaseInfo", "BattleResult", "Kill", "Time" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
