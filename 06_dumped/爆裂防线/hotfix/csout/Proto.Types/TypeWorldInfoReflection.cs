using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldInfoReflection()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLldvcmxkSW5mby5wcm90bxIFdHlwZXMaGnR5cGUuRHVuZ2VvbkJh" + "c2VJbmZvLnByb3RvGiF0eXBlLldvcmxkQnVpbGRpbmdSZXNEZXRhaWwucHJv" + "dG8aGXR5cGUuV29ybGRFdmVudEluZm8ucHJvdG8aF3R5cGUuV29ybGRNYXBJ" + "bmZvLnByb3RvGht0eXBlLldvcmxkUmVjb25EZXRhaWwucHJvdG8aG3R5cGUu" + "V29ybGRSZXNJZGxlSW5mby5wcm90byKjAgoJV29ybGRJbmZvEikKDFdvcmxk" + "TWFwSW5mbxgBIAEoCzITLnR5cGVzLldvcmxkTWFwSW5mbxI2Cg9CdWlsZGlu" + "Z1Jlc0xpc3QYAiADKAsyHS50eXBlcy5Xb3JsZEJ1aWxkaW5nUmVzRGV0YWls" + "EiwKC1Jlc0lkbGVJbmZvGAMgASgLMhcudHlwZXMuV29ybGRSZXNJZGxlSW5m" + "bxIqCglSZWNvbkxpc3QYBCADKAsyFy50eXBlcy5Xb3JsZFJlY29uRGV0YWls" + "EigKCUV2ZW50SW5mbxgFIAEoCzIVLnR5cGVzLldvcmxkRXZlbnRJbmZvEi8K" + "D0R1bmdlb25CYXNlSW5mbxgGIAEoCzIWLnR5cGVzLkR1bmdlb25CYXNlSW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeWorldBuildingResDetailReflection.Descriptor,
			TypeWorldEventInfoReflection.Descriptor,
			TypeWorldMapInfoReflection.Descriptor,
			TypeWorldReconDetailReflection.Descriptor,
			TypeWorldResIdleInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldInfo), (MessageParser)(object)WorldInfo.Parser, new string[6] { "WorldMapInfo", "BuildingResList", "ResIdleInfo", "ReconList", "EventInfo", "DungeonBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
