using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateMechaSubjectFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateMechaSubjectFightResultReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLk9wZXJhdGVNZWNoYVN1YmplY3RGaWdodFJlc3VsdFJlcS5wcm90" + "bxIFdHlwZXMaG3R5cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90byJxCiFPcGVy" + "YXRlTWVjaGFTdWJqZWN0RmlnaHRSZXN1bHRSZXESDAoEVVVJRBgBIAEoCRIP" + "CgdMZXZlbElkGAIgASgFEi0KDEJhdHRsZVJlc3VsdBgDIAEoCzIXLnR5cGVz" + "LkJhdHRsZVJlc3VsdEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateMechaSubjectFightResultReq), (MessageParser)(object)OperateMechaSubjectFightResultReq.Parser, new string[3] { "UUID", "LevelId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
