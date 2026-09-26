using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleTipsInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleTipsInfoReflection()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlJvbGVUaXBzSW5mby5wcm90bxIFdHlwZXMaGXR5cGUuTGVnaW9u" + "RmxhZ0luZm8ucHJvdG8aFnR5cGUuUm9sZUF1eEluZm8ucHJvdG8aF3R5cGUu" + "Um9sZUJhc2VJbmZvLnByb3RvGhx0eXBlLlJvbGVUaXBzRXF1aXBJbmZvLnBy" + "b3RvGht0eXBlLlJvbGVUaXBzU3VpdEluZm8ucHJvdG8aG3R5cGUuUm9sZVRp" + "cHNXYWxsSW5mby5wcm90byKgAgoMUm9sZVRpcHNJbmZvEikKDFJvbGVCYXNl" + "SW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxItCg5MZWdpb25GbGFn" + "SW5mbxgCIAEoCzIVLnR5cGVzLkxlZ2lvbkZsYWdJbmZvEicKC1JvbGVBdXhJ" + "bmZvGAMgASgLMhIudHlwZXMuUm9sZUF1eEluZm8SLwoNRXF1aXBTbG90SW5m" + "bxgEIAEoCzIYLnR5cGVzLlJvbGVUaXBzRXF1aXBJbmZvEi0KDFJvbGVTdWl0" + "SW5mbxgFIAEoCzIXLnR5cGVzLlJvbGVUaXBzU3VpdEluZm8SLQoMUm9sZVdh" + "bGxJbmZvGAYgASgLMhcudHlwZXMuUm9sZVRpcHNXYWxsSW5mb0IuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			TypeLegionFlagInfoReflection.Descriptor,
			TypeRoleAuxInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor,
			TypeRoleTipsEquipInfoReflection.Descriptor,
			TypeRoleTipsSuitInfoReflection.Descriptor,
			TypeRoleTipsWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleTipsInfo), (MessageParser)(object)RoleTipsInfo.Parser, new string[6] { "RoleBaseInfo", "LegionFlagInfo", "RoleAuxInfo", "EquipSlotInfo", "RoleSuitInfo", "RoleWallInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
