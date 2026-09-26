using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonMoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonMoveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldvcmxkRHVuZ2Vvbk1vdmVBY2sucHJvdG8SBXR5cGVzGhp0eXBl" + "LkR1bmdlb25CYXNlSW5mby5wcm90bxoZdHlwZS5EdW5nZW9uTWFwSW5mby5w" + "cm90byJ1ChNXb3JsZER1bmdlb25Nb3ZlQWNrEi8KD0R1bmdlb25CYXNlSW5m" + "bxgBIAEoCzIWLnR5cGVzLkR1bmdlb25CYXNlSW5mbxItCg5EdW5nZW9uTWFw" + "SW5mbxgCIAEoCzIVLnR5cGVzLkR1bmdlb25NYXBJbmZvQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonMoveAck), (MessageParser)(object)WorldDungeonMoveAck.Parser, new string[2] { "DungeonBaseInfo", "DungeonMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
