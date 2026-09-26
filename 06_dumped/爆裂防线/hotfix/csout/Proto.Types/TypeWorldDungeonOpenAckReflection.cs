using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonOpenAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonOpenAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldvcmxkRHVuZ2Vvbk9wZW5BY2sucHJvdG8SBXR5cGVzGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8aG3R5cGUuRHVuZ2VvbkF3YXJkSW5mby5wcm90" + "bxoadHlwZS5EdW5nZW9uQmFzZUluZm8ucHJvdG8aGXR5cGUuRHVuZ2Vvbk1h" + "cEluZm8ucHJvdG8aFHR5cGUuV29ybGRJbmZvLnByb3RvIvQBChNXb3JsZER1" + "bmdlb25PcGVuQWNrEi8KD0R1bmdlb25CYXNlSW5mbxgBIAEoCzIWLnR5cGVz" + "LkR1bmdlb25CYXNlSW5mbxItCg5EdW5nZW9uTWFwSW5mbxgCIAEoCzIVLnR5" + "cGVzLkR1bmdlb25NYXBJbmZvEjEKEER1bmdlb25Bd2FyZEluZm8YAyABKAsy" + "Fy50eXBlcy5EdW5nZW9uQXdhcmRJbmZvEiMKCVdvcmxkSW5mbxgEIAEoCzIQ" + "LnR5cGVzLldvcmxkSW5mbxIlCgpEYXRhQ2hhbmdlGAUgASgLMhEudHlwZXMu" + "RGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDungeonAwardInfoReflection.Descriptor,
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor,
			TypeWorldInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonOpenAck), (MessageParser)(object)WorldDungeonOpenAck.Parser, new string[5] { "DungeonBaseInfo", "DungeonMapInfo", "DungeonAwardInfo", "WorldInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
