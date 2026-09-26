using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2DataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2DataAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkhvbWVsYW5kVjJEYXRhQWNrLnByb3RvEgV0eXBlcxoWdHlwZS5E" + "ZWZlbnNlSW5mby5wcm90bxoXdHlwZS5Ib21lbGFuZEluZm8ucHJvdG8aFnR5" + "cGUuV2FyUm9vbUluZm8ucHJvdG8ikAEKEUhvbWVsYW5kVjJEYXRhQWNrEikK" + "DEhvbWVsYW5kSW5mbxgBIAEoCzITLnR5cGVzLkhvbWVsYW5kSW5mbxInCgtX" + "YXJSb29tSW5mbxgCIAEoCzISLnR5cGVzLldhclJvb21JbmZvEicKC0RlZmVu" + "c2VJbmZvGAMgASgLMhIudHlwZXMuRGVmZW5zZUluZm9CLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDefenseInfoReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2DataAck), (MessageParser)(object)HomelandV2DataAck.Parser, new string[3] { "HomelandInfo", "WarRoomInfo", "DefenseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
