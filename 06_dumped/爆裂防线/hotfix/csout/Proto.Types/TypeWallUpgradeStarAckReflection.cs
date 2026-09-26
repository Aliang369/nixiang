using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallUpgradeStarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallUpgradeStarAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLldhbGxVcGdyYWRlU3RhckFjay5wcm90bxIFdHlwZXMaFXR5cGUu" + "RGF0YUNoYW5nZS5wcm90bxoWdHlwZS5EZWZlbnNlSW5mby5wcm90bxoTdHlw" + "ZS5XYWxsSW5mby5wcm90byKHAQoSV2FsbFVwZ3JhZGVTdGFyQWNrEiEKCFdh" + "bGxJbmZvGAEgASgLMg8udHlwZXMuV2FsbEluZm8SJwoLRGVmZW5zZUluZm8Y" + "AiABKAsyEi50eXBlcy5EZWZlbnNlSW5mbxIlCgpEYXRhQ2hhbmdlGAMgASgL" + "MhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDataChangeReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallUpgradeStarAck), (MessageParser)(object)WallUpgradeStarAck.Parser, new string[3] { "WallInfo", "DefenseInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
