using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEquipSlotGemReplaceAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipSlotGemReplaceAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkVxdWlwU2xvdEdlbVJlcGxhY2VBY2sucHJvdG8SBXR5cGVzGhV0" + "eXBlLkRhdGFDaGFuZ2UucHJvdG8aGHR5cGUuRXF1aXBTbG90SW5mby5wcm90" + "byJsChZFcXVpcFNsb3RHZW1SZXBsYWNlQWNrEisKDUVxdWlwU2xvdEluZm8Y" + "ASABKAsyFC50eXBlcy5FcXVpcFNsb3RJbmZvEiUKCkRhdGFDaGFuZ2UYAiAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeEquipSlotInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipSlotGemReplaceAck), (MessageParser)(object)EquipSlotGemReplaceAck.Parser, new string[2] { "EquipSlotInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
