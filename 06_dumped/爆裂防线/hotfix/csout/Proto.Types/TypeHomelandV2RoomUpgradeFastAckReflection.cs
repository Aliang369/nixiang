using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2RoomUpgradeFastAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2RoomUpgradeFastAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkhvbWVsYW5kVjJSb29tVXBncmFkZUZhc3RBY2sucHJvdG8SBXR5" + "cGVzGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFnR5cGUuRGVmZW5zZUluZm8u" + "cHJvdG8aF3R5cGUuSG9tZWxhbmRJbmZvLnByb3RvGhZ0eXBlLldhclJvb21J" + "bmZvLnByb3RvIsIBChxIb21lbGFuZFYyUm9vbVVwZ3JhZGVGYXN0QWNrEikK" + "DEhvbWVsYW5kSW5mbxgBIAEoCzITLnR5cGVzLkhvbWVsYW5kSW5mbxInCgtX" + "YXJSb29tSW5mbxgCIAEoCzISLnR5cGVzLldhclJvb21JbmZvEicKC0RlZmVu" + "c2VJbmZvGAMgASgLMhIudHlwZXMuRGVmZW5zZUluZm8SJQoKRGF0YUNoYW5n" + "ZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2RoomUpgradeFastAck), (MessageParser)(object)HomelandV2RoomUpgradeFastAck.Parser, new string[4] { "HomelandInfo", "WarRoomInfo", "DefenseInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
