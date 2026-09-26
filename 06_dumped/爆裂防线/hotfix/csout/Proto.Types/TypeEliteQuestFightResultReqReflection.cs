using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestFightResultReqReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkVsaXRlUXVlc3RGaWdodFJlc3VsdFJlcS5wcm90bxIFdHlwZXMa" + "G3R5cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90byJ7ChhFbGl0ZVF1ZXN0Rmln" + "aHRSZXN1bHRSZXESDwoHTGV2ZWxJZBgBIAEoBRINCgVDb3VudBgCIAEoBRIQ" + "CghQbGF5ZXJJZBgDIAEoAxItCgxCYXR0bGVSZXN1bHQYBCABKAsyFy50eXBl" + "cy5CYXR0bGVSZXN1bHRJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestFightResultReq), (MessageParser)(object)EliteQuestFightResultReq.Parser, new string[4] { "LevelId", "Count", "PlayerId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
