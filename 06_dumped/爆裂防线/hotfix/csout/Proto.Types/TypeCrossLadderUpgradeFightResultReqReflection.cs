using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderUpgradeFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderUpgradeFightResultReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLkNyb3NzTGFkZGVyVXBncmFkZUZpZ2h0UmVzdWx0UmVxLnByb3Rv" + "EgV0eXBlcxobdHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvImMKIENyb3Nz" + "TGFkZGVyVXBncmFkZUZpZ2h0UmVzdWx0UmVxEhAKCFBsYXllcklkGAEgASgF" + "Ei0KDEJhdHRsZVJlc3VsdBgCIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdElu" + "Zm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderUpgradeFightResultReq), (MessageParser)(object)CrossLadderUpgradeFightResultReq.Parser, new string[2] { "PlayerId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
