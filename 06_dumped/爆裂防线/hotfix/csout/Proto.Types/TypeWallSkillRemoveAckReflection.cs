using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallSkillRemoveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallSkillRemoveAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLldhbGxTa2lsbFJlbW92ZUFjay5wcm90bxIFdHlwZXMaFnR5cGUu" + "RGVmZW5zZUluZm8ucHJvdG8aE3R5cGUuV2FsbEluZm8ucHJvdG8iYAoSV2Fs" + "bFNraWxsUmVtb3ZlQWNrEiEKCFdhbGxJbmZvGAEgASgLMg8udHlwZXMuV2Fs" + "bEluZm8SJwoLRGVmZW5zZUluZm8YAiABKAsyEi50eXBlcy5EZWZlbnNlSW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDefenseInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallSkillRemoveAck), (MessageParser)(object)WallSkillRemoveAck.Parser, new string[2] { "WallInfo", "DefenseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
