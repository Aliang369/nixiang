using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2RoomComposeAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2RoomComposeAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkhvbWVsYW5kVjJSb29tQ29tcG9zZUFjay5wcm90bxIFdHlwZXMa" + "FnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90" + "bxoWdHlwZS5EZWZlbnNlSW5mby5wcm90bxoXdHlwZS5Ib21lbGFuZEluZm8u" + "cHJvdG8aFnR5cGUuV2FyUm9vbUluZm8ucHJvdG8i5QEKGEhvbWVsYW5kVjJS" + "b29tQ29tcG9zZUFjaxIpCgxIb21lbGFuZEluZm8YASABKAsyEy50eXBlcy5I" + "b21lbGFuZEluZm8SJwoLV2FyUm9vbUluZm8YAiABKAsyEi50eXBlcy5XYXJS" + "b29tSW5mbxInCgtEZWZlbnNlSW5mbxgDIAEoCzISLnR5cGVzLkRlZmVuc2VJ" + "bmZvEiUKCUF3YXJkTGlzdBgEIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUK" + "CkRhdGFDaGFuZ2UYBSABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2RoomComposeAck), (MessageParser)(object)HomelandV2RoomComposeAck.Parser, new string[5] { "HomelandInfo", "WarRoomInfo", "DefenseInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
