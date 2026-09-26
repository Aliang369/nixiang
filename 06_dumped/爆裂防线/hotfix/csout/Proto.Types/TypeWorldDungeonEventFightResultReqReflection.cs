using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventFightResultReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventFightResultReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLldvcmxkRHVuZ2VvbkV2ZW50RmlnaHRSZXN1bHRSZXEucHJvdG8S" + "BXR5cGVzGht0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8iUAofV29ybGRE" + "dW5nZW9uRXZlbnRGaWdodFJlc3VsdFJlcRItCgxCYXR0bGVSZXN1bHQYASAB" + "KAsyFy50eXBlcy5CYXR0bGVSZXN1bHRJbmZvQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleResultInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventFightResultReq), (MessageParser)(object)WorldDungeonEventFightResultReq.Parser, new string[1] { "BattleResult" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
