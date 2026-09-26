using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldEventFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldEventFightResultReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLldvcmxkRXZlbnRGaWdodFJlc3VsdFJlcS5wcm90bxIFdHlwZXMa" + "G3R5cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90byJZChhXb3JsZEV2ZW50Rmln" + "aHRSZXN1bHRSZXESDgoGR3JpZElkGAEgASgFEi0KDEJhdHRsZVJlc3VsdBgC" + "IAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm9CLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldEventFightResultReq), (MessageParser)(object)WorldEventFightResultReq.Parser, new string[2] { "GridId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
