using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleDefenseDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleDefenseDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkJhdHRsZURlZmVuc2VEZXRhaWwucHJvdG8SBXR5cGVzGhdlbnVt" + "LkVEZWZlbnNlVHlwZS5wcm90bxoidHlwZS5CYXR0bGVEZWZlbnNlQXR0ckRl" + "dGFpbC5wcm90byKzAQoTQmF0dGxlRGVmZW5zZURldGFpbBIvCgtEZWZlbnNl" + "VHlwZRgBIAEoDjIaLkVEZWZlbnNlVHlwZS5FRGVmZW5zZVR5cGUSEgoKRGVm" + "ZW5zZVBjdBgCIAEoAhIwCghBdHRyTGlzdBgDIAMoCzIeLnR5cGVzLkJhdHRs" + "ZURlZmVuc2VBdHRyRGV0YWlsEg4KBldhbGxJZBgEIAEoBRIVCg1XYWxsU2tp" + "bGxMaXN0GAUgAygFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEDefenseTypeReflection.Descriptor,
			TypeBattleDefenseAttrDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleDefenseDetail), (MessageParser)(object)BattleDefenseDetail.Parser, new string[5] { "DefenseType", "DefensePct", "AttrList", "WallId", "WallSkillList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
