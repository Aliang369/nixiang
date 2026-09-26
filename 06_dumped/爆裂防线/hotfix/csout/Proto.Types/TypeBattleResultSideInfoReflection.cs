using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleResultSideInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleResultSideInfoReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkJhdHRsZVJlc3VsdFNpZGVJbmZvLnByb3RvEgV0eXBlcxoVZW51" + "bS5FQm9vbFN0YXRlLnByb3RvIq4BChRCYXR0bGVSZXN1bHRTaWRlSW5mbxIN" + "CgVMZXZlbBgBIAEoBRINCgVSb3VuZBgCIAEoBRISCgpEZWZlbnNlUGN0GAMg" + "ASgCEicKB0tpbGxBbGwYBCABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGUS" + "KAoIS2lsbEJvc3MYBSABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGUSEQoJ" + "S2lsbENvdW50GAYgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleResultSideInfo), (MessageParser)(object)BattleResultSideInfo.Parser, new string[6] { "Level", "Round", "DefensePct", "KillAll", "KillBoss", "KillCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
