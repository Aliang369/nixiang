using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHomelandV2FloorUnlockAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHomelandV2FloorUnlockAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkhvbWVsYW5kVjJGbG9vclVubG9ja0Fjay5wcm90bxIFdHlwZXMa" + "FnR5cGUuRGVmZW5zZUluZm8ucHJvdG8aF3R5cGUuSG9tZWxhbmRJbmZvLnBy" + "b3RvGhZ0eXBlLldhclJvb21JbmZvLnByb3RvIpcBChhIb21lbGFuZFYyRmxv" + "b3JVbmxvY2tBY2sSKQoMSG9tZWxhbmRJbmZvGAEgASgLMhMudHlwZXMuSG9t" + "ZWxhbmRJbmZvEicKC1dhclJvb21JbmZvGAIgASgLMhIudHlwZXMuV2FyUm9v" + "bUluZm8SJwoLRGVmZW5zZUluZm8YAyABKAsyEi50eXBlcy5EZWZlbnNlSW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDefenseInfoReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HomelandV2FloorUnlockAck), (MessageParser)(object)HomelandV2FloorUnlockAck.Parser, new string[3] { "HomelandInfo", "WarRoomInfo", "DefenseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
