using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairHelpFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairHelpFightResultReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLldvcmxkTGFpckhlbHBGaWdodFJlc3VsdFJlcS5wcm90bxIFdHlw" + "ZXMaG3R5cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90byJaChtXb3JsZExhaXJI" + "ZWxwRmlnaHRSZXN1bHRSZXESDAoEVVVJRBgBIAEoCRItCgxCYXR0bGVSZXN1" + "bHQYAiABKAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZvQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairHelpFightResultReq), (MessageParser)(object)WorldLairHelpFightResultReq.Parser, new string[2] { "UUID", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
