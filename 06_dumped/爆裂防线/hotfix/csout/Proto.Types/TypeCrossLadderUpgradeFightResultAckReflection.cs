using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderUpgradeFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderUpgradeFightResultAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLkNyb3NzTGFkZGVyVXBncmFkZUZpZ2h0UmVzdWx0QWNrLnByb3Rv" + "EgV0eXBlcxobdHlwZS5CYXR0bGVSZXN1bHRJbmZvLnByb3RvGhp0eXBlLkNy" + "b3NzTGFkZGVySW5mby5wcm90byKCAQogQ3Jvc3NMYWRkZXJVcGdyYWRlRmln" + "aHRSZXN1bHRBY2sSLwoPQ3Jvc3NMYWRkZXJJbmZvGAEgASgLMhYudHlwZXMu" + "Q3Jvc3NMYWRkZXJJbmZvEi0KDEJhdHRsZVNldHRsZRgCIAEoCzIXLnR5cGVz" + "LkJhdHRsZVJlc3VsdEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleResultInfoReflection.Descriptor,
			TypeCrossLadderInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderUpgradeFightResultAck), (MessageParser)(object)CrossLadderUpgradeFightResultAck.Parser, new string[2] { "CrossLadderInfo", "BattleSettle" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
