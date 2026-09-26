using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleSkillDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleSkillDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkJhdHRsZVNraWxsRGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5F" + "U2tpbGxUeXBlLnByb3RvGiB0eXBlLkJhdHRsZVNraWxsQXR0ckRldGFpbC5w" + "cm90byKhAQoRQmF0dGxlU2tpbGxEZXRhaWwSKQoJU2tpbGxUeXBlGAEgASgO" + "MhYuRVNraWxsVHlwZS5FU2tpbGxUeXBlEg8KB1NraWxsSWQYAiABKAUSDwoH" + "SGVyb1RpZBgDIAEoBRIuCghBdHRyTGlzdBgEIAMoCzIcLnR5cGVzLkJhdHRs" + "ZVNraWxsQXR0ckRldGFpbBIPCgdNb2RMaXN0GAUgAygFQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESkillTypeReflection.Descriptor,
			TypeBattleSkillAttrDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleSkillDetail), (MessageParser)(object)BattleSkillDetail.Parser, new string[5] { "SkillType", "SkillId", "HeroTid", "AttrList", "ModList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
