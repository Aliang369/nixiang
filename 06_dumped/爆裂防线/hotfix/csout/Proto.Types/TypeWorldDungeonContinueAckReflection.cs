using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonContinueAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonContinueAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLldvcmxkRHVuZ2VvbkNvbnRpbnVlQWNrLnByb3RvEgV0eXBlcxob" + "dHlwZS5EdW5nZW9uQXdhcmRJbmZvLnByb3RvGhp0eXBlLkR1bmdlb25CYXNl" + "SW5mby5wcm90bxoZdHlwZS5EdW5nZW9uTWFwSW5mby5wcm90byKsAQoXV29y" + "bGREdW5nZW9uQ29udGludWVBY2sSLwoPRHVuZ2VvbkJhc2VJbmZvGAEgASgL" + "MhYudHlwZXMuRHVuZ2VvbkJhc2VJbmZvEi0KDkR1bmdlb25NYXBJbmZvGAIg" + "ASgLMhUudHlwZXMuRHVuZ2Vvbk1hcEluZm8SMQoQRHVuZ2VvbkF3YXJkSW5m" + "bxgDIAEoCzIXLnR5cGVzLkR1bmdlb25Bd2FyZEluZm9CLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDungeonAwardInfoReflection.Descriptor,
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonContinueAck), (MessageParser)(object)WorldDungeonContinueAck.Parser, new string[3] { "DungeonBaseInfo", "DungeonMapInfo", "DungeonAwardInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
