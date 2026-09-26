using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallActiveAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLldhbGxBY3RpdmVBY2sucHJvdG8SBXR5cGVzGhV0eXBlLkRhdGFD" + "aGFuZ2UucHJvdG8aFnR5cGUuRGVmZW5zZUluZm8ucHJvdG8aE3R5cGUuV2Fs" + "bEluZm8ucHJvdG8ikgEKDVdhbGxBY3RpdmVBY2sSDgoGV2FsbElkGAEgASgF" + "EiEKCFdhbGxJbmZvGAIgASgLMg8udHlwZXMuV2FsbEluZm8SJwoLRGVmZW5z" + "ZUluZm8YAyABKAsyEi50eXBlcy5EZWZlbnNlSW5mbxIlCgpEYXRhQ2hhbmdl" + "GAQgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallActiveAck), (MessageParser)(object)WallActiveAck.Parser, new string[4] { "WallId", "WallInfo", "DefenseInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
