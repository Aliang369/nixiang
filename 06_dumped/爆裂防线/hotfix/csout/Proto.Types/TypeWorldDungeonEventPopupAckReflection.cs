using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventPopupAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventPopupAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLldvcmxkRHVuZ2VvbkV2ZW50UG9wdXBBY2sucHJvdG8SBXR5cGVz" + "Ghp0eXBlLkR1bmdlb25CYXNlSW5mby5wcm90bxoZdHlwZS5EdW5nZW9uTWFw" + "SW5mby5wcm90byJ7ChlXb3JsZER1bmdlb25FdmVudFBvcHVwQWNrEi8KD0R1" + "bmdlb25CYXNlSW5mbxgBIAEoCzIWLnR5cGVzLkR1bmdlb25CYXNlSW5mbxIt" + "Cg5EdW5nZW9uTWFwSW5mbxgCIAEoCzIVLnR5cGVzLkR1bmdlb25NYXBJbmZv" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventPopupAck), (MessageParser)(object)WorldDungeonEventPopupAck.Parser, new string[2] { "DungeonBaseInfo", "DungeonMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
