using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonBattleResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonBattleResultAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlNlYXNvbkJhdHRsZVJlc3VsdEFjay5wcm90bxIFdHlwZXMaG3R5" + "cGUuQmF0dGxlUmVzdWx0SW5mby5wcm90byJUChVTZWFzb25CYXR0bGVSZXN1" + "bHRBY2sSDAoEVXVpZBgBIAEoCRItCgxCYXR0bGVTZXR0bGUYAiABKAsyFy50" + "eXBlcy5CYXR0bGVSZXN1bHRJbmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonBattleResultAck), (MessageParser)(object)SeasonBattleResultAck.Parser, new string[2] { "Uuid", "BattleSettle" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
