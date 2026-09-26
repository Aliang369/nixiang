using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2RoomUpgradeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2RoomUpgradeAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkhvbWVsYW5kVjJSb29tVXBncmFkZUFjay5wcm90bxIFdHlwZXMa" + "FXR5cGUuRGF0YUNoYW5nZS5wcm90bxoWdHlwZS5EZWZlbnNlSW5mby5wcm90" + "bxoXdHlwZS5Ib21lbGFuZEluZm8ucHJvdG8aFnR5cGUuV2FyUm9vbUluZm8u" + "cHJvdG8ivgEKGEhvbWVsYW5kVjJSb29tVXBncmFkZUFjaxIpCgxIb21lbGFu" + "ZEluZm8YASABKAsyEy50eXBlcy5Ib21lbGFuZEluZm8SJwoLV2FyUm9vbUlu" + "Zm8YAiABKAsyEi50eXBlcy5XYXJSb29tSW5mbxInCgtEZWZlbnNlSW5mbxgD" + "IAEoCzISLnR5cGVzLkRlZmVuc2VJbmZvEiUKCkRhdGFDaGFuZ2UYBCABKAsy" + "ES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2RoomUpgradeAck), (MessageParser)(object)HomelandV2RoomUpgradeAck.Parser, new string[4] { "HomelandInfo", "WarRoomInfo", "DefenseInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
