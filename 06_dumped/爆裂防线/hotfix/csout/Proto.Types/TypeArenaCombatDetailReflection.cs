using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaCombatDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaCombatDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkFyZW5hQ29tYmF0RGV0YWlsLnByb3RvEgV0eXBlcxoYdHlwZS5B" + "cmVuYVJhbmtJbmZvLnByb3RvGhd0eXBlLlJvbGVCYXNlSW5mby5wcm90byJr" + "ChFBcmVuYUNvbWJhdERldGFpbBIpCgxSb2xlQmFzZUluZm8YASABKAsyEy50" + "eXBlcy5Sb2xlQmFzZUluZm8SKwoNQXJlbmFSYW5rSW5mbxgCIAEoCzIULnR5" + "cGVzLkFyZW5hUmFua0luZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeArenaRankInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaCombatDetail), (MessageParser)(object)ArenaCombatDetail.Parser, new string[2] { "RoleBaseInfo", "ArenaRankInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
