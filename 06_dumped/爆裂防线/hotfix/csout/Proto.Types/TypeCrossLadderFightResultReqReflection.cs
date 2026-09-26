using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderFightResultReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkNyb3NzTGFkZGVyRmlnaHRSZXN1bHRSZXEucHJvdG8SBXR5cGVz" + "Ght0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8iXAoZQ3Jvc3NMYWRkZXJG" + "aWdodFJlc3VsdFJlcRIQCghQbGF5ZXJJZBgBIAEoBRItCgxCYXR0bGVSZXN1" + "bHQYAiABKAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZvQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderFightResultReq), (MessageParser)(object)CrossLadderFightResultReq.Parser, new string[2] { "PlayerId", "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
