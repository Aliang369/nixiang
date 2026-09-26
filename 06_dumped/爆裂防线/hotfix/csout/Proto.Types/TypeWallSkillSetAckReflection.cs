using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallSkillSetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallSkillSetAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLldhbGxTa2lsbFNldEFjay5wcm90bxIFdHlwZXMaFnR5cGUuRGVm" + "ZW5zZUluZm8ucHJvdG8aE3R5cGUuV2FsbEluZm8ucHJvdG8iXQoPV2FsbFNr" + "aWxsU2V0QWNrEiEKCFdhbGxJbmZvGAEgASgLMg8udHlwZXMuV2FsbEluZm8S" + "JwoLRGVmZW5zZUluZm8YAiABKAsyEi50eXBlcy5EZWZlbnNlSW5mb0IuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDefenseInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallSkillSetAck), (MessageParser)(object)WallSkillSetAck.Parser, new string[2] { "WallInfo", "DefenseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
