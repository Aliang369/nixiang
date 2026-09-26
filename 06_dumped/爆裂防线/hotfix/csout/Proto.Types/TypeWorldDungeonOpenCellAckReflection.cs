using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonOpenCellAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonOpenCellAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLldvcmxkRHVuZ2Vvbk9wZW5DZWxsQWNrLnByb3RvEgV0eXBlcxoa" + "dHlwZS5EdW5nZW9uQmFzZUluZm8ucHJvdG8aGXR5cGUuRHVuZ2Vvbk1hcElu" + "Zm8ucHJvdG8ieQoXV29ybGREdW5nZW9uT3BlbkNlbGxBY2sSLwoPRHVuZ2Vv" + "bkJhc2VJbmZvGAEgASgLMhYudHlwZXMuRHVuZ2VvbkJhc2VJbmZvEi0KDkR1" + "bmdlb25NYXBJbmZvGAIgASgLMhUudHlwZXMuRHVuZ2Vvbk1hcEluZm9CLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonOpenCellAck), (MessageParser)(object)WorldDungeonOpenCellAck.Parser, new string[2] { "DungeonBaseInfo", "DungeonMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
