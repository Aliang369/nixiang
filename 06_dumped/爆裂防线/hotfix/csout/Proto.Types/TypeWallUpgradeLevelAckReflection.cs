using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallUpgradeLevelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallUpgradeLevelAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldhbGxVcGdyYWRlTGV2ZWxBY2sucHJvdG8SBXR5cGVzGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8aFnR5cGUuRGVmZW5zZUluZm8ucHJvdG8aE3R5" + "cGUuV2FsbEluZm8ucHJvdG8iiAEKE1dhbGxVcGdyYWRlTGV2ZWxBY2sSIQoI" + "V2FsbEluZm8YASABKAsyDy50eXBlcy5XYWxsSW5mbxInCgtEZWZlbnNlSW5m" + "bxgCIAEoCzISLnR5cGVzLkRlZmVuc2VJbmZvEiUKCkRhdGFDaGFuZ2UYAyAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallUpgradeLevelAck), (MessageParser)(object)WallUpgradeLevelAck.Parser, new string[3] { "WallInfo", "DefenseInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
