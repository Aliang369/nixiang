using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventModSelectAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventModSelectAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLldvcmxkRHVuZ2VvbkV2ZW50TW9kU2VsZWN0QWNrLnByb3RvEgV0" + "eXBlcxoadHlwZS5EdW5nZW9uQmFzZUluZm8ucHJvdG8aGXR5cGUuRHVuZ2Vv" + "bk1hcEluZm8ucHJvdG8ifwodV29ybGREdW5nZW9uRXZlbnRNb2RTZWxlY3RB" + "Y2sSLwoPRHVuZ2VvbkJhc2VJbmZvGAEgASgLMhYudHlwZXMuRHVuZ2VvbkJh" + "c2VJbmZvEi0KDkR1bmdlb25NYXBJbmZvGAIgASgLMhUudHlwZXMuRHVuZ2Vv" + "bk1hcEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventModSelectAck), (MessageParser)(object)WorldDungeonEventModSelectAck.Parser, new string[2] { "DungeonBaseInfo", "DungeonMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
