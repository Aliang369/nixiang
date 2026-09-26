using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventFightStartAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLldvcmxkRHVuZ2VvbkV2ZW50RmlnaHRTdGFydEFjay5wcm90bxIF" + "dHlwZXMaFXR5cGUuQmF0dGxlSW5mby5wcm90bxoadHlwZS5EdW5nZW9uQmFz" + "ZUluZm8ucHJvdG8aGXR5cGUuRHVuZ2Vvbk1hcEluZm8ucHJvdG8iugEKHldv" + "cmxkRHVuZ2VvbkV2ZW50RmlnaHRTdGFydEFjaxIRCglEdW5nZW9uSWQYASAB" + "KAUSLwoPRHVuZ2VvbkJhc2VJbmZvGAIgASgLMhYudHlwZXMuRHVuZ2VvbkJh" + "c2VJbmZvEi0KDkR1bmdlb25NYXBJbmZvGAMgASgLMhUudHlwZXMuRHVuZ2Vv" + "bk1hcEluZm8SJQoKQmF0dGxlSW5mbxgEIAEoCzIRLnR5cGVzLkJhdHRsZUlu" + "Zm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeBattleInfoReflection.Descriptor,
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventFightStartAck), (MessageParser)(object)WorldDungeonEventFightStartAck.Parser, new string[4] { "DungeonId", "DungeonBaseInfo", "DungeonMapInfo", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
