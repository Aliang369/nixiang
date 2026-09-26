using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBattleMechaSkillDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleMechaSkillDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkJhdHRsZU1lY2hhU2tpbGxEZXRhaWwucHJvdG8SBXR5cGVzImwK" + "FkJhdHRsZU1lY2hhU2tpbGxEZXRhaWwSDQoFSW5kZXgYASABKAUSDQoFTGV2" + "ZWwYAiABKAUSDwoHU2tpbGxJZBgDIAEoBRIPCgdTa2lsbENkGAQgASgCEhIK" + "CkRhbWFnZVJhdGUYBSABKAJCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleMechaSkillDetail), (MessageParser)(object)BattleMechaSkillDetail.Parser, new string[5] { "Index", "Level", "SkillId", "SkillCd", "DamageRate" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
