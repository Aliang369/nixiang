using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkFyZW5hRmlnaHRSZXN1bHRSZXEucHJvdG8SBXR5cGVzGht0eXBl" + "LkJhdHRsZVJlc3VsdEluZm8ucHJvdG8iZAoTQXJlbmFGaWdodFJlc3VsdFJl" + "cRIQCghQbGF5ZXJJZBgBIAEoBRIMCgRSYW5rGAIgASgFEi0KDEJhdHRsZVJl" + "c3VsdBgDIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm9CLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaFightResultReq), (MessageParser)(object)ArenaFightResultReq.Parser, new string[3] { "PlayerId", "Rank", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
